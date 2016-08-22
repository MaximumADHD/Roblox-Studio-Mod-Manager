local file = io.open("RobloxStudioBeta.exe", "rb")
local data = file:read("*a")
file:close()

local uniqueStrings = {}
local paths = {}

print("Getting FVars...")

for str in data:gmatch("[%w%s%p]+") do
	if #str > 10 and not (str:sub(1,1) == "?") and not (str:find("?AV?")) then
		str = str:gsub("[\r\n]+"," ")
		local char = str:sub(1,1)
		if char:match("%a") then
			local uniquePath = str:gsub(":%d+$","")
			if uniquePath:sub(1,12) == "PlaceFilter_" then
				uniquePath = uniquePath:sub(13)
				if #uniquePath > 0 and not uniqueStrings[uniquePath:lower()] then
					uniqueStrings[uniquePath:lower()] = true
					table.insert(paths,uniquePath)
				end
			end
		end
	end
end

table.sort(paths)

local file = io.open("fvars.txt","w")
file:write(table.concat(paths,"\n"))
file:close()