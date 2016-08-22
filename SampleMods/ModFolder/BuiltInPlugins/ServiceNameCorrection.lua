-- @CloneTrooper1019, 2015
-- The purpose of this plugin is to fix some naming issues with the game services.
-- Its not actually a plugin, I'm just using plugins since they are guarenteed to run when a place is opened.
-- This plugin is obsolete if you don't have any ReflectionMetadata mod.

function markService(service)
	pcall(function ()
		-- In a pcall because this doesn't always work.
		service.Name = service.ClassName
	end)
end

for _,v in pairs(game:GetChildren()) do
	markService(v)
end

game.ServiceAdded:connect(markService)