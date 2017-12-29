-------------------------------------------------------------------------------------------------------------------------------------
-- Inject Default StarterScript
-------------------------------------------------------------------------------------------------------------------------------------

local RobloxReplicatedStorage = game:GetService("RobloxReplicatedStorage")
local requestGetAsync = RobloxReplicatedStorage:WaitForChild("RequestGetAsync")	

local CoreGui = game:GetService("CoreGui")
local RobloxGui = CoreGui:WaitForChild("RobloxGui")
local Modules = RobloxGui:WaitForChild("Modules")
local Custom = Modules:WaitForChild("Custom")
local Proxies = Custom:WaitForChild("Proxies")

local starterScriptInject = Proxies:WaitForChild("Proxy_StarterScript")
local starterScript = requestGetAsync:InvokeServer("https://raw.githubusercontent.com/CloneTrooper1019/Roblox-Client-Watch/master/scripts/CoreScripts/StarterScript.lua")
starterScriptInject.Source = starterScript .. "\nreturn 0"

require(starterScriptInject)

-------------------------------------------------------------------------------------------------------------------------------------
-- Run Custom Modules
-------------------------------------------------------------------------------------------------------------------------------------

local function executeCustomModule(module)
	if module:IsA("ModuleScript") then
		module.Source = module.Source .. "\nreturn 0"
		spawn(function ()
			local success,errorMsg = pcall(function ()
				require(module)
			end)
			if not success then
				warn("Error while executing CoreModule",module.Name,"-",errorMsg)
			end
		end)
	end
end

for _,module in pairs(Custom:GetChildren()) do
	executeCustomModule(module)
end

Custom.ChildAdded:connect(executeCustomModule)

-------------------------------------------------------------------------------------------------------------------------------------