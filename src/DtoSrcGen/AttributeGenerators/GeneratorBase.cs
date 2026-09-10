using System.Linq;
using Microsoft.CodeAnalysis;

namespace DtoSrcGen
{
    internal abstract class GeneratorBase
    {
        public virtual string AttributeName => string.Empty;
        
        public string AttributeNameWithNamespace => $"DtoSrcGen.{AttributeName}";

        protected AttributeData AttributeData { get; set; }

        public bool GetGenerateDefaultCtor()
        {
            if (AttributeData is null)
                return true;

            var namedArgument = AttributeData.NamedArguments.FirstOrDefault(x => x.Key == "GenerateDefaultCtor");
            if (namedArgument is { Key: "GenerateDefaultCtor", Value: { Value: bool generateDefaultCtor } })
                return generateDefaultCtor;

            return true;
        }
    }
}