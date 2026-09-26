using System;

namespace DtoSrcGen
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class RecordAttribute : Attribute
    {
        public RecordAttribute(Type sourceEnum, Type sourceType)
        {
            SourceEnum = sourceEnum;
            SourceType = sourceType;
        }

        public Type SourceEnum { get; private set; }

        public Type SourceType { get; private set; }

        public bool GenerateDefaultCtor { get; set; } = true;
    }
}