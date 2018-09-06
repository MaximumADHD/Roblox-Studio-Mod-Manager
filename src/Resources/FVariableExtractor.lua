--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
-- @CloneTrooper1019, 2018
-- Mod Manager FVariable Extractor
-- This plugin communicates over the HttpService with the Mod Manager to identify FVariables that the program can use.
-- It will only run while Studio is in a specific uncopylocked place that has the HttpService pre-enabled.
--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

local CollectionService = game:GetService("CollectionService")
local HttpService = game:GetService("HttpService")

local BEGIN_EXTRACT_TAG = "FFlagExtract"
	
local function initialized()
	return CollectionService:HasTag(HttpService, BEGIN_EXTRACT_TAG)
end

if not initialized() then
	local addedSignal = CollectionService:GetInstanceAddedSignal(BEGIN_EXTRACT_TAG)
	repeat addedSignal:Wait() until initialized()
end

-- thx roblox
HttpService:SetHttpEnabled(true)

--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

local HttpService = game:GetService("HttpService")
local ModManager = "http://localhost:20326/"

local function queryModManager(request, query)
	local header = query or {}
	header.Query = request
	return HttpService:GetAsync(ModManager, true, header)
end

local strings do
	local success = false
	while not success do
		success, strings = pcall(queryModManager, "GetStringList")
	end
end

local at = 0
local registeredFvars = {}
local payload = {}

local function pushPayload()
	local chunk = table.concat(payload, ";")
	payload = {}
	queryModManager("SendFVariables", {FVariables = chunk})
end

local function pingProgress()
	queryModManager("PingProgress", {Count = tostring(at)})
end

for word in strings:gmatch("[^\n]+") do
	local isFVar,fVar = pcall(function ()
		return settings():GetFVariable(word)
	end)
	if isFVar and not registeredFvars[word] and not word:find("PlaceFilter_") then
		registeredFvars[word] = true
		table.insert(payload, word)
		if #payload == 100 then
			pushPayload()
		end
	end
	at = at + 1
	if at % 300 == 0 then
		pingProgress()
		wait()
	end
end

pingProgress()
wait(1)
queryModManager("Finished")

--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------