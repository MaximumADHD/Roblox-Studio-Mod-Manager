-------------------------------------------------------------------------------------------------------------------------------------
-- Inject Default StarterScript
-------------------------------------------------------------------------------------------------------------------------------------

local RobloxReplicatedStorage = game:GetService("RobloxReplicatedStorage")
local requestGetAsync = RobloxReplicatedStorage:WaitForChild("RequestGetAsync")	

local CoreGui = game:GetService("CoreGui")
local Custom = CoreGui.RobloxGui.Modules.Common.Custom
local Required = Custom.Required

local gitBaseUrl = require(Required.CoreScriptBaseUrl)
local starterScriptInject = Required.ProxyStarterScript
local starterScript = requestGetAsync:InvokeServer(gitBaseUrl .. "scripts/CoreScripts/StarterScript.lua")
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
				warn("Error while executing CoreScript Module",module.Name,"-",errorMsg)
			end
		end)
	end
end

for _,module in pairs(Custom:GetChildren()) do
	executeCustomModule(module)
end

Custom.ChildAdded:connect(executeCustomModule)

-------------------------------------------------------------------------------------------------------------------------------------