using System;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace RobloxStudioModManager
{
    public class FVariable // : IComparable
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
            if (Value != value)
            {
                if (Editor != null)
                    Editor.SetValue("Value", value);

                Value = value;
                Dirty = true;
            } 
        }
        
        public void SetEditor(RegistryKey editor)
        {
            if (editor != null)
            {
                string value = editor.GetString("Value", Value);
                SetValue(value);

                editor.SetValue("Name", Name);
                editor.SetValue("Type", Type);
                editor.SetValue("Reset", Reset);
            }

            Editor = editor;
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

        /*public int CompareTo(object obj)
        {
            if (obj is FVariable)
            {
                var other = obj as FVariable;
                return Key.CompareTo(other.Key);
            }

            throw new NotImplementedException();
        }*/
    }
}
