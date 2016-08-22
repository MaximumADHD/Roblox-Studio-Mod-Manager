@echo off
lua.exe GetFVars.lua
type fvars.txt| clip
RobloxStudioBeta.exe -fileLocation %~dp0Blank.rbxl -script "require(458546362)"