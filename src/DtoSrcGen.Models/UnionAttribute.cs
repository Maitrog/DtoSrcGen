using System;

namespace DtoSrcGen
{
    [AttributeUsage(AttributeTargets.Class)]
    public class UnionAttribute : Attribute
    {
        public UnionAttribute(params Type[] types)
        {
            Types = types;
        }
        
        public Type[] Types { get; private set; }
    }
}