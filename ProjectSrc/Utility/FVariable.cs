using System;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace RobloxStudioModManager
{
    public class FVariable
    {
        private static Regex flagTypes = new Regex("((F|DF|SF)(Flag|String|Int|Log))");

        public readonly string Name;
        public readonly string Type;
        public readonly string Reset;

        public string Key => Type + Name;
        public string Value { get; private set; }

        public RegistryKey Editor { get; private set; }
        public bool Dirty = false;

        public FVariable(string key, string value)
        {
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
