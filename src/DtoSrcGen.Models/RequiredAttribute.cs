using System;

namespace DtoSrcGen
{
    [AttributeUsage(AttributeTargets.Class)]
    public class RequiredAttribute : Attribute
    {
        public RequiredAttribute(Type sourceType)
        {
            SourceType = sourceType;
        }

        public Type SourceType { get; private set; }
    }
}