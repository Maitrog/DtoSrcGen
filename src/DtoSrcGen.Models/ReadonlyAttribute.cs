using System;

namespace DtoSrcGen
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
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