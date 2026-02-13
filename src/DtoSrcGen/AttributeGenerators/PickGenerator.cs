using System.Text;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace DtoSrcGen
{
    internal class PickGenerator : IAttributeGenerator
    {
        public string AttributeName => "PickAttribute";

        public string AttributeNameWithNamespace => "DtoSrcGen.PickAttribute";

        public void Pre(SourceProductionContext context, LanguageVersion currentLanguageVersion, INamedTypeSymbol symbol)
        {
            // do nothing
        }

        public void AppendConstructors(SourceProductionContext context, StringBuilder sb, INamedTypeSymbol symbol, int indent)
        {
            var ctorSb = new StringBuilder();
            ctorSb.Append($"{GeneratorUtils.Indent(indent)}public {symbol.Name}");

            var attributes = symbol.GetAttributes();

            var attributeData = attributes.FirstOrDefault(x => x.AttributeClass?.Name == AttributeName);

            var type = attributeData.ConstructorArguments[0].Value as INamedTypeSymbol;
            var properties = attributeData.ConstructorArguments[1].Values.Select(x => x.Value as string).ToList();

            var ns = type.ContainingNamespace;
            var nsName = ns.IsGlobalNamespace ? "" : $"{ns.ToDisplayString()}.";
            ctorSb.AppendLine($"({nsName}{type.Name} value)");
            ctorSb.AppendLine($"{GeneratorUtils.Indent(indent)}{{");
            indent++;

            foreach (var property in properties)
            {
                var member = type.GetMembers(property).FirstOrDefault(x
                    => x.Kind is SymbolKind.Property or SymbolKind.Field
                       && !x.IsImplicitlyDeclared
                       && !x.IsStatic
                       && x.DeclaredAccessibility is Accessibility.Public or Accessibility.Internal or Accessibility.ProtectedOrInternal);
                if (member == null)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        new DiagnosticDescriptor(
                            "DSG3000",
                            "Type doesn't contain member",
                            "Type '{0}' doesn't contain a member named '{1}'.",
                            "DtoSrcGen",
                            DiagnosticSeverity.Error,
                            isEnabledByDefault: true),
                        type.Locations.First(),
                        type.Name,
                        property));
                    continue;
                }

                ctorSb.AppendLine($"{GeneratorUtils.Indent(indent)}{member.Name} = value.{member.Name};");
            }

            indent--;
            ctorSb.AppendLine($"{GeneratorUtils.Indent(indent)}}}");

            sb.AppendLine(ctorSb.ToString());
        }

        public void AppendProperties(SourceProductionContext context, StringBuilder sb, INamedTypeSymbol symbol, int indent)
        {
            var attributes = symbol.GetAttributes();

            var attributeData = attributes.FirstOrDefault(x => x.AttributeClass?.Name == AttributeName);

            var type = attributeData.ConstructorArguments[0].Value as INamedTypeSymbol;
            var properties = attributeData.ConstructorArguments[1].Values.Select(x => x.Value as string).ToList();

            foreach (var property in properties)
            {
                var member = type.GetMembers(property).FirstOrDefault(x
                    => x.Kind is SymbolKind.Property or SymbolKind.Field
                       && !x.IsImplicitlyDeclared
                       && !x.IsStatic
                       && x.DeclaredAccessibility is Accessibility.Public or Accessibility.Internal or Accessibility.ProtectedOrInternal);
                if (member is null)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        new DiagnosticDescriptor(
                            "DSG3000",
                            "Type doesn't contain member",
                            "Type '{0}' doesn't contain a member named '{1}'.",
                            "DtoSrcGen",
                            DiagnosticSeverity.Error,
                            isEnabledByDefault: true),
                        symbol.Locations.First(),
                        type.Name,
                        property));
                    continue;
                }

                var accessibility = GeneratorUtils.AccessibilityToString(member);

                var memberType = member.Kind switch
                                 {
                                     SymbolKind.Property => (member as IPropertySymbol)?.Type.ToDisplayString(),
                                     SymbolKind.Field => (member as IFieldSymbol)?.Type.ToDisplayString(),
                                 };

                sb.AppendLine($"{GeneratorUtils.Indent(indent)}{accessibility} {memberType} {member.Name} {{ get; set; }}");
            }
        }
    }
}