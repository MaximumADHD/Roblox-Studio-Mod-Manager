// Forked from:
// https://github.com/pizzaboxer/rbxfflagdumper/blob/main/RbxFFlagDumper.Lib/RawFFlagData.cs

namespace RobloxStudioModManager
{
    internal class RawFFlagData
    {
        public int DataTypeId;

        public int ByteParam;

        public string Name;

        public RawFFlagData(int dataTypeId, int byteParam, string name)
        {
            DataTypeId = dataTypeId;
            ByteParam = byteParam;
            Name = name;
        }
    }
}
