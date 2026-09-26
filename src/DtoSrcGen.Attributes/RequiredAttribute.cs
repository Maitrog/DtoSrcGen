using System;

namespace DtoSrcGen
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class RequiredAttribute : Attribute
    {
        public RequiredAttribute(Type sourceType)
        {
            SourceType = sourceType;
        }

        public Type SourceType { get; private set; }

        public bool GenerateDefaultCtor { get; set; } = true;
    }
}