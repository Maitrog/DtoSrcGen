using System;

namespace DtoSrcGen
{
    [AttributeUsage(AttributeTargets.Class)]
    public class OmitAttribute : Attribute
    {
        public OmitAttribute(Type sourceType, params string[] properties)
        {
            Type = sourceType;
            Properties = properties;
        }

        public Type Type { get; private set; }

        public bool GenerateDefaultCtor { get; set; } = true;
        
        public string[] Properties { get; private set; }
    }
}