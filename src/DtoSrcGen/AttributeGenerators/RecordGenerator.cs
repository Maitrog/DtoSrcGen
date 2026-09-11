using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DtoSrcGen
{
    internal class RecordGenerator : GeneratorBase, IAttributeGenerator
    {
        public override string AttributeName => "RecordAttribute";

        private INamedTypeSymbol TargetEnum { get; set; }

        private INamedTypeSymbol TargetType { get; set; }
        
        public void Pre(SourceProductionContext context, LanguageVersion currentLanguageVersion, INamedTypeSymbol symbol, AttributeData attributeData)
        {
            AttributeData = attributeData;
            
            TargetEnum = AttributeData.ConstructorArguments[0].Value as INamedTypeSymbol;
            if (TargetEnum?.TypeKind != TypeKind.Enum)
            {
                var location = symbol.Locations.First();
                if (attributeData.ApplicationSyntaxReference?.GetSyntax() is AttributeSyntax attributeSyntax
                    && attributeSyntax.ArgumentList is { } argumentList
                    && argumentList.Arguments.Count > 0)
                    location = argumentList.Arguments[0].GetLocation();

                context.ReportDiagnostic(Diagnostic.Create(
                    new DiagnosticDescriptor(
                        "DSG3003",
                        "Invalid type kind",
                        "Type '{0}' is not an enum type.",
                        "DtoSrcGen",
                        DiagnosticSeverity.Error,
                        isEnabledByDefault: true),
                    location,
                    TargetEnum?.Name));
            }

            TargetType = AttributeData.ConstructorArguments[1].Value as INamedTypeSymbol;
        }

        public void AppendConstructors(SourceProductionContext context, StringBuilder sb, INamedTypeSymbol symbol, int indent)
        {
            // do nothing
        }

        public void AppendProperties(SourceProductionContext context, StringBuilder sb, INamedTypeSymbol symbol, int indent)
        {
            foreach (var enumMember in TargetEnum.MemberNames)
            {
                sb.AppendLine($"{GeneratorUtils.Indent(indent)}public {GeneratorUtils.GetMemberType(TargetType)} {enumMember} {{ get; set; }}");
            }
        }
    }
}