local SUPPORTED_BRANCHES = 
{
	["roblox"] = "master";
	["gametest1.robloxlabs"] = true;
	["gametest2.robloxlabs"] = true;
}

local ScriptContext = game:GetService("ScriptContext")
local GetModManagerBranch = ScriptContext:WaitForChild("GetModManagerBranch")

local branch = GetModManagerBranch:Invoke()
local support = SUPPORTED_BRANCHES[branch]
local fork = "master"

if support then
	if typeof(support) == "string" then
		fork = support
	else
		fork = branch
	end
end

return "https://raw.githubusercontent.com/CloneTrooper1019/Roblox-Client-Watch/" .. fork .. "/"