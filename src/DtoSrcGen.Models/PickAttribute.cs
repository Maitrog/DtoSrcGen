using System;

namespace DtoSrcGen
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class PickAttribute : Attribute
    {
        public PickAttribute(Type sourceType, params string[] properties)
        {
            Type = sourceType;
            Properties = properties;
        }

        public Type Type { get; private set; }
        
        public string[] Properties { get; private set; }
    }
}