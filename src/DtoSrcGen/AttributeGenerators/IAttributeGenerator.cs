using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace DtoSrcGen
{
    public interface IAttributeGenerator
    {
        public string AttributeName { get; }
        
        public string AttributeNameWithNamespace { get; }

        bool GetGenerateDefaultCtor();
        
        void Pre(SourceProductionContext context, LanguageVersion currentLanguageVersion, INamedTypeSymbol symbol, AttributeData attributeData);

        void AppendConstructors(SourceProductionContext context, StringBuilder sb, INamedTypeSymbol symbol, int indent);

        void AppendProperties(SourceProductionContext context, StringBuilder sb, INamedTypeSymbol symbol, int indent);
    }
}