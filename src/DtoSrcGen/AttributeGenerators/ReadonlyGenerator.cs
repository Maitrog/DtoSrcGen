using System.Collections.Generic;
using System.Text;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace DtoSrcGen
{
    internal class ReadonlyGenerator : IAttributeGenerator
    {
        public string AttributeName => "ReadonlyAttribute";

        public string AttributeNameWithNamespace => "DtoSrcGen.ReadonlyAttribute";

        private IReadOnlyList<ISymbol> Members { get; set; }

        public void Pre(SourceProductionContext context, LanguageVersion currentLanguageVersion, INamedTypeSymbol symbol)
        {
            var attributes = symbol.GetAttributes();
            
            var attributeData = attributes.FirstOrDefault(x => x.AttributeClass?.Name == AttributeName);
            
            var type = attributeData.ConstructorArguments[0].Value as INamedTypeSymbol;
            
            var members = type.GetMembers();
            Members = members.Where(x
                => x.Kind is SymbolKind.Property or SymbolKind.Field
                   && x.DeclaredAccessibility is Accessibility.Public or Accessibility.Internal or Accessibility.ProtectedOrInternal
                   && !x.IsStatic
                   && !x.IsImplicitlyDeclared).ToList();
        }


        public void AppendConstructors(SourceProductionContext context, StringBuilder sb, INamedTypeSymbol symbol, int indent)
        {
            var ctorSb = new StringBuilder();
            ctorSb.Append($"{GeneratorUtils.Indent(indent)}public {symbol.Name}");

            var attributes = symbol.GetAttributes();

            var attributeData = attributes.FirstOrDefault(x => x.AttributeClass?.Name == AttributeName);

            var type = attributeData.ConstructorArguments[0].Value as INamedTypeSymbol;

            var ns = type.ContainingNamespace;
            var nsName = ns.IsGlobalNamespace ? "" : $"{ns.ToDisplayString()}.";
            ctorSb.AppendLine($"({nsName}{type.Name} value)");
            ctorSb.AppendLine($"{GeneratorUtils.Indent(indent)}{{");
            indent++;

            foreach (var member in Members)
            {
                ctorSb.AppendLine($"{GeneratorUtils.Indent(indent)}{member.Name} = value.{member.Name};");
            }

            indent--;
            ctorSb.AppendLine($"{GeneratorUtils.Indent(indent)}}}");

            sb.AppendLine(ctorSb.ToString());
        }

        public void AppendProperties(SourceProductionContext context, StringBuilder sb, INamedTypeSymbol symbol, int indent)
        {
            foreach (var member in Members)
            {
                var accessibility = GeneratorUtils.AccessibilityToString(member);
            
                var memberType = member.Kind switch
                                 {
                                     SymbolKind.Property => (member as IPropertySymbol)?.Type.ToDisplayString(),
                                     SymbolKind.Field => (member as IFieldSymbol)?.Type.ToDisplayString(),
                                 };
            
                sb.AppendLine($"{GeneratorUtils.Indent(indent)}{accessibility} {memberType} {member.Name} {{ get; }}");
            }
        }
    }
}