using System.Diagnostics.Contracts;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace RobloxStudioModManager
{
    public class FVariable
    {
        private static readonly Regex flagTypes = new Regex("((F|DF|SF)(Flag|String|Int|Log))");

        public string Name = "";
        public string Type = "";
        public string Reset = "";
        public string Value = "";
        public bool Custom = false;

        internal bool Dirty = true;
        internal string Key => Type + Name;

        public FVariable()
        {
        }

        public FVariable(string key, string value, bool custom = false)
        {
            Contract.Requires(key != null && value != null);
            var match = flagTypes.Match(key);

            Type = match.Value;
            Name = key.Substring(Type.Length);

            if (Type.EndsWith("Flag"))
                value = value.ToLowerInvariant();

            Reset = value;
            Value = value;
            Custom = custom;

            Dirty = true;
        }

        public void SetValue(string value)
        {
            if (Value != value)
            {
                Value = value;
                Dirty = true;
            }
        }

        public void Clear()
        {
            SetValue(Reset);
        }

        public override string ToString()
        {
            return Key;
        }
    }
}
