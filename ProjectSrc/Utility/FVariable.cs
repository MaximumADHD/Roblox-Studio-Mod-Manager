using System;
using System.Diagnostics.Contracts;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace RobloxStudioModManager
{
    public class FVariable
    {
        private static readonly Regex flagTypes = new Regex("((F|DF|SF)(Flag|String|Int|Log))");

        public string Name { get; private set; }
        public string Type { get; private set; }
        public string Reset { get; private set; }

        public string Key => Type + Name;
        public string Value { get; private set; }

        public RegistryKey Editor { get; private set; }
        public bool Dirty { get; set; } = false;

        public FVariable(string key, string value)
        {
            Contract.Requires(key != null && value != null);
            var match = flagTypes.Match(key);

            Type = match.Value;
            Name = key.Substring(Type.Length);

            Reset = value;
            Value = value;

            Dirty = true;
        }

        public void SetValue(string value)
        {
            if (Editor != null && Editor.GetString("Value") != value)
                Editor.SetValue("Value", value);

            if (Value != value)
            {
                Value = value;
                Dirty = true;
            }
        }

        public void SetEditor(RegistryKey editor)
        {
            Editor = editor;

            if (editor != null)
            {
                string value = editor.GetString("Value", Value);
                SetValue(value);

                editor.SetValue("Name", Name);
                editor.SetValue("Type", Type);
                editor.SetValue("Reset", Reset);
            }

            Dirty = true;
        }

        public void ClearEditor()
        {
            SetEditor(null);
            SetValue(Reset);
        }

        public override string ToString()
        {
            return Key;
        }
    }
}
