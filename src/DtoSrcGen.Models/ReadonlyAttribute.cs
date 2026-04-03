using System;

namespace DtoSrcGen
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ReadonlyAttribute : Attribute
    {
        public ReadonlyAttribute(Type sourceType)
        {
            SourceType = sourceType;
        }

        public Type SourceType { get; private set; }

        public bool GenerateDefaultCtor { get; set; } = true;
    }
}