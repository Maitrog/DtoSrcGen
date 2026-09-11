using System;

namespace DtoSrcGen
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class UnionAttribute : Attribute
    {
        public UnionAttribute(params Type[] types)
        {
            Types = types;
        }
        
        public bool GenerateDefaultCtor { get; set; } = true;

        public Type[] Types { get; private set; }
    }
}