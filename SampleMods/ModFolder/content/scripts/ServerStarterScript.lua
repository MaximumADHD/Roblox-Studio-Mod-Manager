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

local CoreGui = game:GetService("CoreGui")
local RobloxGui = CoreGui:WaitForChild("RobloxGui")
local Modules = RobloxGui:WaitForChild("Modules")
local Custom = Modules:WaitForChild("Custom")
local Proxies = Custom:WaitForChild("Proxies")

local proxyStarterScript = Proxies:WaitForChild("Proxy_ServerStarterScript")
local starterScript = requestGetAsyncImpl("https://raw.githubusercontent.com/CloneTrooper1019/Roblox-Client-Watch/master/scripts/CoreScripts/ServerStarterScript.lua")
proxyStarterScript.Source = starterScript .. "\nreturn 0"

require(proxyStarterScript)

------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------