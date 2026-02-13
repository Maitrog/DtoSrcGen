using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace DtoSrcGen
{
    internal interface IAttributeGenerator
    {
        public string AttributeName { get; }
        
        public string AttributeNameWithNamespace { get; }
        
        void Pre(SourceProductionContext context, LanguageVersion currentLanguageVersion, INamedTypeSymbol symbol);

        void AppendConstructors(SourceProductionContext context, StringBuilder sb, INamedTypeSymbol symbol, int indent);

        void AppendProperties(SourceProductionContext context, StringBuilder sb, INamedTypeSymbol symbol, int indent);
    }
}