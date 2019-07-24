using System;
using System.Collections.Generic;
using System.Text;

// Custom attributes are used to annotate user-defined custom functions.

namespace WindwardCustom
{
    [AttributeUsage(AttributeTargets.Method)]
    public class FunctionDescriptionAttribute : System.Attribute
    {
        public FunctionDescriptionAttribute(string description)
        {
            this.description = description;
        }
        public string Description
        {
            get { return description; }
        }
        private string description;
    }

    [AttributeUsage(AttributeTargets.Parameter)]
    public class ParameterDescriptionAttribute : System.Attribute
    {
        public ParameterDescriptionAttribute(string description)
        {
            this.description = description;
        }
        public string Description
        {
            get { return description; }
        }
        private string description;
    }
}
