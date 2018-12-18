------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
-- HttpService Query
------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

local RobloxReplicatedStorage = game:GetService("RobloxReplicatedStorage")
local HttpService = game:GetService("HttpService")

local requestGetAsync = Instance.new("RemoteFunction")
requestGetAsync.Name = "RequestGetAsync"
requestGetAsync.Archivable = false
requestGetAsync.Parent = RobloxReplicatedStorage

local function requestGetAsyncImpl(url)
	local response do
		local enabled = HttpService.HttpEnabled
		HttpService.HttpEnabled = true
		response = HttpService:GetAsync(url)
		HttpService.HttpEnabled = enabled
	end
	return response
end

function requestGetAsync.OnServerInvoke(player,url)
	return requestGetAsyncImpl(url)
end

------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
-- Inject Default ServerStarterScript
------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

local ScriptContext = game:GetService("ScriptContext")
local CoreGui = game:GetService("CoreGui")

local Custom = CoreGui.RobloxGui.Modules.Common.Custom
local Required = Custom.Required

local baseUrl = require(Required.CoreScriptBaseUrl)
local proxyStarterScript = Required.ProxyServerStarterScript
local starterScript = requestGetAsyncImpl(baseUrl .. "scripts/CoreScripts/ServerStarterScript.lua")
proxyStarterScript.Name = "ServerStarterScript"
proxyStarterScript.Source = starterScript .. "\nreturn 0"
proxyStarterScript.Parent = ScriptContext

spawn(function ()
	require(proxyStarterScript)
end)

wait()
proxyStarterScript.Source = "" -- This stops the script analyzer from reporting issues in this script.
script:Destroy()

------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------