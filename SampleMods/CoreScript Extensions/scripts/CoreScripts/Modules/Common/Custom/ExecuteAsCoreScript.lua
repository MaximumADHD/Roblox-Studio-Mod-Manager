local RobloxReplicatedStorage = game:GetService("RobloxReplicatedStorage")
local Selection = game:GetService("Selection")

local plugin = PluginManager():CreatePlugin()
local toolbar = plugin:CreateToolbar("CloneTrooper1019")
local button = toolbar:CreateButton("Execute Selection as CoreScript", "Executes the selected LuaSourceContainer at the CoreScript security level.", "rbxasset://textures/plugins/CoreScript.png")

local function executeScript(object)
	warn("Executing as CoreScript:", object:GetFullName())
	
	local run = loadstring(object.Source)
	local env = getfenv(run)
	env.script = object
	
	run()
end

local function onClick()
	if game:FindService("NetworkServer") or game:FindService("NetworkClient") then
		warn("This plugin cannot be used while a NetworkPeer is present.")
		return
	end
	local object = Selection:Get()[1]
	if object then
		if object:IsA("ModuleScript") then
			require(object)
		elseif object:IsA("BaseScript") then
			executeScript(object)
		else
			warn("Selected object must be a LuaSourceContainer!")
		end
	else
		warn("No object selected!")
	end
end

button.Click:connect(onClick)