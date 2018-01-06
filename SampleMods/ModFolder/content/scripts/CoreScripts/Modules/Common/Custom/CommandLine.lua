-------------------------------------------------------------------------------------------------------------------------------------
-- @CloneTrooper1019, 2017
-- CoreGui Command Bar
-------------------------------------------------------------------------------------------------------------------------------------
-- Setup
-------------------------------------------------------------------------------------------------------------------------------------

local coreGui = game:GetService("CoreGui")
local robloxGui = coreGui:WaitForChild("RobloxGui")
local http = game:GetService("HttpService")
local userInput = game:GetService("UserInputService")
local guiService = game:GetService("GuiService")
local rs = game:GetService("RunService")
local history = {}

local cmd = Instance.new("TextBox")
cmd.Name = "Cmd"
cmd.Size = UDim2.new(1,0,0,29)
cmd.Position = UDim2.new(0,0,1,0)
cmd.TextXAlignment = "Left"
cmd.FontSize = Enum.FontSize.Size28
cmd.Font = "SourceSans"
cmd.Text = ""
cmd.Parent = robloxGui

-------------------------------------------------------------------------------------------------------------------------------------
-- Input and Tweening
-------------------------------------------------------------------------------------------------------------------------------------

local inset = 0
local goalInset = 0
local isActive = false

local function moveTowards(value,goal,rate)
	if value < goal then
		return math.min(goal,value + rate)
	elseif value > goal then
		return math.max(goal,value - rate)
	else
		return goal
	end
end

local function renderUpdate()
	if inset ~= goalInset then
		local yOffset = 0
		if robloxGui:FindFirstChild("TopBarContainer") then
			yOffset = robloxGui.TopBarContainer.Size.Y.Offset
		end
		inset = moveTowards(inset,goalInset,6)
		guiService:SetGlobalGuiInset(0,yOffset,0,inset)
	end
end

local function setActive(active)
	isActive = active
	if isActive then
		goalInset = 30
		cmd:CaptureFocus()
	else
		goalInset = 0
		cmd:ReleaseFocus()
		cmd.Text = ""
	end
end

local function onInputBegan(input)
	if input.KeyCode == Enum.KeyCode.Backquote then
		setActive(not isActive)
	end
end

spawn(function ()
	local topBar = robloxGui:WaitForChild("TopBarContainer",5)
	if topBar then
		inset = inset - 1 -- bump the inset
	end
end)

rs:BindToRenderStep(http:GenerateGUID(),0,renderUpdate)
userInput.InputBegan:connect(onInputBegan)

-------------------------------------------------------------------------------------------------------------------------------------
-- Syntax Highlighting
-------------------------------------------------------------------------------------------------------------------------------------
local primitivesRaw = "and break do else elseif end false for function if in local nil not or repeat return then true until while"
local operators = "+ - * / %% ^ # = ~ < > %( %) { } %[ %] ; : , %."
local env = getfenv()
local studio = settings().Studio

local primitives = {}
for primitive in primitivesRaw:gmatch("[^ ]+") do
	primitives[primitive] = true
end

local deprecatedBuiltInKeywords =
{
	Game = true;
	PluginManager = true;
	settings = true;
	Workspace = true;
}

local function isPrimitiveKeyword(keyword,s)
	return primitives[keyword]
end

local function isBuiltInKeyword(keyword)
	return (env[keyword] ~= nil and rawget(env,keyword) == nil and not deprecatedBuiltInKeywords[keyword])
end

local function indexGmatch(str,pattern)
	local indexAt = 1
	return function ()
		local startIndex,endIndex = str:find(pattern,indexAt)
		if startIndex and endIndex and startIndex <= endIndex then
			indexAt = endIndex + 1
			return startIndex,endIndex,str:sub(startIndex,endIndex)
		end
	end
end

-- Main --

local blankC3 = Color3.new()

local function syntaxHighlight(str,fontSize)
	-- Update colors

	local backgroundColor 		= studio["Background Color"]
	local builtInFunctionColor 	= studio["Built-in Function Color"]
	local commentColor 			= studio["Comment Color"]
	local keywordColor 			= studio["Keyword Color"]
	local numberColor 			= studio["Number Color"]
	local operatorColor 		= studio["Operator Color"]
	local stringColor 			= studio["String Color"]
	local textColor 			= studio["Text Color"]
	
	cmd.BackgroundColor3 = backgroundColor
	cmd.BorderColor3 = blankC3:lerp(Color3.new(1-backgroundColor.r,1-backgroundColor.g,1-backgroundColor.b),0.4)
	cmd.TextColor3 = cmd.BorderColor3
	

	-- Build the individual characters

	local fontHeight = tonumber(fontSize.Name:match("%d+$"))
	local charMap = {}
	local activeWidth = 0
	
	for _,v in pairs(cmd:GetChildren()) do
		if tonumber(v.Name) then
			v:Destroy()
		end
	end
	
	for char in str:gmatch(".") do
		local label = Instance.new("TextLabel",cmd)
		label.BackgroundTransparency = 1
		label.FontSize = fontSize
		label.Font = "SourceSans"
		label.Text = ((char == "\n" or char == "\t") and " " or char)
		label.TextColor3 = textColor
		label.ZIndex = 2
		local bounds = label.TextBounds
		label.Size = UDim2.new(0,bounds.X,0,bounds.Y)
		label.Position = UDim2.new(0,activeWidth-1,0,1)
		activeWidth = activeWidth + bounds.X
		table.insert(charMap,{
			Character = char;
			Label = label;
		})
		label.Name = #charMap
	end
	
	local function markCharMap(tag,color,startIndex,endIndex,setBold)
		for i = startIndex,endIndex do
			local char = charMap[i]
			char[tag] = true
			char.Label.TextColor3 = color
			if setBold ~= nil then
				if setBold then
					char.Label.Font = "SourceSansBold"
				else
					char.Label.Font = "SourceSans"
				end
			end
		end
	end
	
	-- Process Numbers
	
	for startIndex,endIndex in indexGmatch(str,"%d+") do
		local canMark = true
		if startIndex-1 > 0 then
			local charToLeft = str:sub(startIndex-1,startIndex-1)
			if charToLeft:match("%w") then
				canMark = false
			end
		end
		if canMark then
			while endIndex < #str do
				local n = endIndex + 1
				if str:sub(n,n):match("[%s%)%}%]]") then
					break
				else
					endIndex = n 
				end
			end
			markCharMap("Number",numberColor,startIndex,endIndex)
		end
	end
	
	-- Process built-in functions and primitives.
	
	for startIndex,endIndex,keyword in indexGmatch(str,"%w+") do
		if #keyword > 1 then
			if isPrimitiveKeyword(keyword) then
				markCharMap("Primitive",keywordColor,startIndex,endIndex,true)
			elseif isBuiltInKeyword(keyword) then
				local canMark = true
				if startIndex-1 > 0 then
					local char = str:sub(startIndex-1,startIndex-1)
					if char == "." or char == ":" then
						canMark = false
					end
				end
				if canMark then
					markCharMap("BuiltIn",builtInFunctionColor,startIndex,endIndex,true)
				end
			end
		end
	end
	
	-- Process basic strings
	
	local i = 1
	local readingStr = false
	local activeChar = ""
	
	while i <= #str do
		local char = str:sub(i,i)
		if char == '"' or char == "'" then
			markCharMap("String",stringColor,i,i,false)
			if not readingStr then
				readingStr = true
				activeChar = char
			elseif activeChar == char then
				readingStr = false
			end
		end
		if readingStr then
			markCharMap("String",stringColor,i,i,false)
			if char == "\\" then
				-- Mark the next character as a string and skip it.
				-- Don't even ask questions.
				i = i + 1
				if i <= #str then
					markCharMap("String",stringColor,i,i,false)
				end
			elseif char == "\n" then
				-- Code execution will result in an error, but stop reading anyway.
				readingStr = false
				activeChar = ""
			end
		end
		i = i + 1
	end
	
	-- Process Blocks

	local i = 1
	local startBlock = ""
	local inBlock = false
	
	while i <= #str do
		local char = str:sub(i,i)
		if char == "[" then
			local startedAt = i
			local invalid = false
			startBlock = "["
			while true do
				i = i + 1
				if i > #str then
					break
				end
				local nextChar = str:sub(i,i)
				startBlock = startBlock .. nextChar
				if nextChar == "[" then
					break
				elseif nextChar ~= "=" then
					invalid = true
					break
				end
			end
			if not invalid and #startBlock > 1 then
				local endBlock = startBlock:gsub("%[","%]")
				local endedAt = #str
				local ebStart,ebEnd = str:find(endBlock,startedAt)
				if ebStart and ebEnd and ebStart < ebEnd then
					endedAt = ebEnd
					i = ebEnd
				end
				markCharMap("Block",stringColor,startedAt,endedAt,false)
			end
		end
		i = i + 1
	end

	-- Process Comments
	
	local inBlock = false
	
	for startIndex,endIndex,comment in indexGmatch(str,"%-%-") do
		if not charMap[startIndex].Block then
			while endIndex < #str do
				endIndex = endIndex + 1
				local char = charMap[endIndex].Character
				if charMap[endIndex].Block then
					inBlock = true
				elseif inBlock or char == "\n" then
					break
				end
			end
			markCharMap("Comment",commentColor,startIndex,endIndex,false)
		end
	end
		
	-- Process Operators
	
	for operator in operators:gmatch("[^ ]+") do
		for startIndex,endIndex in indexGmatch(str,operator) do
			local m = charMap[startIndex]
			if not m.Comment and not m.Block and not m.String and not m.Number then
				markCharMap("Operator",operatorColor,startIndex,endIndex)
			end
		end
	end

	lastCharTyped = tick()
end

local function onChanged(property)
	if property == "Text" or pcall(function () return studio[property] end) then
		if cmd.Text:find("\n") then
			cmd.Text = cmd.Text:gsub("\n"," ")
		else
			syntaxHighlight(cmd.Text,cmd.FontSize)
		end
	end
end

onChanged("Text")
cmd.Changed:connect(onChanged)
studio.Changed:connect(onChanged)

--------------------------------------------------------------------------------------------------------------------------------------
-- Command Execution 
--------------------------------------------------------------------------------------------------------------------------------------
local historyIndex = -1

local function onMouseWheelBackward()
	if isActive then
		if historyIndex == -1 then
			historyIndex = 1
		else
			historyIndex = math.min(#history,historyIndex+1)
		end
		if history[historyIndex] then
			cmd.Text = history[historyIndex]
		end
	end
end

local function onMouseWheelForward()
	if isActive then
		if historyIndex == -1 then
			historyIndex = #history
		else
			historyIndex = math.max(1,historyIndex-1)
		end
		if history[historyIndex] then
			cmd.Text = history[historyIndex]
		end
	end
end

local function onFocusLost(enterPressed)
	if enterPressed then
		historyIndex = -1
		local text = cmd.Text
		if #text > 0 then
			table.insert(history,text)
			warn(">",text)
			spawn(function ()
				local func,errorMsg = loadstring(text)
				if func then
					func()
				else
					error("Error in script: "..errorMsg)
				end
			end)
		end
	end
	if historyIndex == -1 then
		setActive(false)
	end
end

cmd.FocusLost:connect(onFocusLost)
cmd.MouseWheelBackward:connect(onMouseWheelBackward)
cmd.MouseWheelForward:connect(onMouseWheelForward)
--------------------------------------------------------------------------------------------------------------------------------------