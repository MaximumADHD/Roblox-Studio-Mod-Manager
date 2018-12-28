local SUPPORTED_BRANCHES = 
{
	["roblox"] = true;
	["gametest1.robloxlabs"] = true;
	["gametest2.robloxlabs"] = true;
}

local ScriptContext = game:GetService("ScriptContext")
local GetModManagerBranch = ScriptContext:WaitForChild("GetModManagerBranch")

local branch = GetModManagerBranch:Invoke()
if not SUPPORTED_BRANCHES[branch] then
	branch = "roblox"
end

return "https://raw.githubusercontent.com/CloneTrooper1019/Roblox-Client-Watch/" .. branch .. "/"