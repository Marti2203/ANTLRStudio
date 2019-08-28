using System;
namespace ANTLRStudio.Models
{
    [Serializable]
    public class CompilerOption
    {
        public string Name { get; }
        public bool Value { get; set; }
        public string ActiveFlag { get; }
        public string InactiveFlag { get; private set; } = string.Empty;
        public CompilerOption(string name, bool value, string activeFlag) : this(name, value, activeFlag, string.Empty)
        {
        }
        public CompilerOption(string name, bool value, string activeFlag, string inactiveFlag)
        {
            Name = name;
            Value = value;
            ActiveFlag = activeFlag;
            InactiveFlag = inactiveFlag;
        }
        public void InverseValue()
        {
            Value = !Value;
        }
    }
}
