local file = io.open(..., 'rb')
local data = file:read('*a')
file:close()

os.execute("mkdir ImageBakeDump")

local c = 0
local nn = 1
while true do
    local s,f = data:find('\137\80\78\71\13.-\73\69\78\68\174\66\96\130',nn)
    if not s then break end
    nn = f + 1
    content = data:sub(s,f)

    local name = string.format('ImageBakeDump/image-%d.png', c)
    local p = io.open(name, 'wb')
    if p then
        p:write(content)
        p:flush()
        p:close()
        print("wrote `" .. name .. "` from offset " .. s-1 .. " and size " .. f-s+1)
        c = c + 1
    else
        print("could not open " .. name)
    end
end

print("done")