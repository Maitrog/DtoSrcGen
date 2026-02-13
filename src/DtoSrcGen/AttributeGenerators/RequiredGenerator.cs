using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace DtoSrcGen
{
    public class RequiredGenerator : IAttributeGenerator
    {
        public string AttributeName => "RequiredAttribute";

        public string AttributeNameWithNamespace => "DtoSrcGen.RequiredAttribute";

        private bool _languageIsSupported = true;

        public IReadOnlyList<ISymbol> Members { get; set; }

        public void Pre(SourceProductionContext context, LanguageVersion currentLanguageVersion, INamedTypeSymbol symbol)
        {
            if (currentLanguageVersion <  LanguageVersion.CSharp11)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    new DiagnosticDescriptor(
                        "DSG3002",
                        "Not supported",
                        "\'RequiredAttribute\' is supported from C# 11.",
                        "DtoSrcGen",
                        DiagnosticSeverity.Error,
                        isEnabledByDefault: true),
                    symbol.Locations.First()));
                _languageIsSupported = false;
                return;
            }
            
            var attributes = symbol.GetAttributes();
            
            var attributeData = attributes.FirstOrDefault(x => x.AttributeClass?.Name == AttributeName);
            
            var type = attributeData.ConstructorArguments[0].Value as INamedTypeSymbol;
            
            var members = type.GetMembers();
            Members = members.Where(x
                => x.Kind is SymbolKind.Property or SymbolKind.Field
                   && x.DeclaredAccessibility is Accessibility.Public
                   && !x.IsStatic
                   && !x.IsImplicitlyDeclared).ToList();

            if (members.Any(x => x.Kind is SymbolKind.Property or SymbolKind.Field
                                 && x.DeclaredAccessibility is Accessibility.Internal or Accessibility.ProtectedOrInternal
                                 && !x.IsStatic
                                 && !x.IsImplicitlyDeclared))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    new DiagnosticDescriptor(
                        "DSG2001",
                        "Internal members are ignored",
                        "Type \'{0}\' contains Internal or ProtectedInternal members that was ignored.",
                        "DtoSrcGen",
                        DiagnosticSeverity.Warning,
                        isEnabledByDefault: true),
                    symbol.Locations.First(),
                    type.Name));
            }
        }


        public void AppendConstructors(SourceProductionContext context, StringBuilder sb, INamedTypeSymbol symbol, int indent)
        {
            if (!_languageIsSupported)
                return;
            
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
            if (!_languageIsSupported)
                return;
            
            foreach (var member in Members)
            {
                var accessibility = GeneratorUtils.AccessibilityToString(member);
            
                var memberType = member.Kind switch
                                 {
                                     SymbolKind.Property => (member as IPropertySymbol)?.Type.ToDisplayString(),
                                     SymbolKind.Field => (member as IFieldSymbol)?.Type.ToDisplayString(),
                                 };
            
                sb.AppendLine($"{GeneratorUtils.Indent(indent)}{accessibility} required {memberType} {member.Name} {{ get; set; }}");
            }
        }
    }
}