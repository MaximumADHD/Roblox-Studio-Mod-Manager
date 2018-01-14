--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
-- @CloneTrooper1019, 2018
-- Mod Manager FVariable Extractor
-- This plugin communicates over the HttpService with the Mod Manager to identify FVariables that the program can use.
-- It will only run while Studio is in a specific uncopylocked place that has the HttpService pre-enabled.
--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

local EXTRACTION_PLACE_ID = 1327419651

do
	-- To identify for certain that we're actually in this place, wait for Roblox to log that the DataModel is loading this placeId in particular.
	local LogService = game:GetService("LogService")
	local inPlace = false
	
	local function checkLog(message,messageType)
		if messageType == Enum.MessageType.MessageInfo and message:sub(1,17) == "DataModel Loading" then
			local placeId = tonumber(message:match("%d+$"))
			if placeId == EXTRACTION_PLACE_ID then
				inPlace = true
			end
		end
	end
	
	for _,logEntry in pairs(LogService:GetLogHistory()) do
		checkLog(logEntry.messageType,logEntry.message)
	end
	
	while not inPlace do
		checkLog(LogService.MessageOut:Wait())
	end
end

--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

local HttpService = game:GetService("HttpService")
local ModManager = "http://localhost:20326/"

local function queryModManager(request,query)
	local header = query or {}
	header.Query = request
	return HttpService:GetAsync(ModManager,true,header)
end

local strings = queryModManager("GetStringList")
local at = 0
local registeredFvars = {}
local payload = {}

local function pushPayload()
	local chunk = table.concat(payload,";")
	payload = {}
	queryModManager("SendFVariables",{FVariables = chunk})
end

local function pingProgress()
	queryModManager("PingProgress",{Count = tostring(at)})
end

for word in strings:gmatch("[^\n]+") do
	local isFVar,fVar = pcall(function ()
		return settings():GetFVariable(word)
	end)
	if isFVar and not registeredFvars[word] then
		registeredFvars[word] = true
		table.insert(payload,word)
		if #payload == 50 then
			pushPayload()
		end
	end
	at = at + 1
	if at%100 == 0 then
		pingProgress()
	end
	if at%300 == 0 then
		wait()
	end
end

pingProgress()
wait(1)
queryModManager("Finished")

--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------