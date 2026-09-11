using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DtoSrcGen
{
    internal abstract class SingleTypeGeneratorBase : GeneratorBase, IAttributeGenerator
    {
        private bool _languageIsSupported = true;

        protected virtual LanguageVersion MinLanguageVersion => LanguageVersion.CSharp8;

        private IReadOnlyList<SymbolWithAlias> Members { get; set; }
        
        protected INamedTypeSymbol TargetType { get; private set; }

        public void Pre(SourceProductionContext context, LanguageVersion currentLanguageVersion, INamedTypeSymbol symbol, AttributeData attributeData)
        {
            if (currentLanguageVersion <  MinLanguageVersion)
            {
                var location = symbol.Locations.First();
                if (attributeData.ApplicationSyntaxReference?.GetSyntax() is AttributeSyntax attributeSyntax)
                    location = attributeSyntax.GetLocation();
                
                context.ReportDiagnostic(Diagnostic.Create(
                    new DiagnosticDescriptor(
                        "DSG3002",
                        "Not supported",
                        $"\'{AttributeName}\' is supported from C# {MinLanguageVersion.ToDisplayString()}.",
                        "DtoSrcGen",
                        DiagnosticSeverity.Error,
                        isEnabledByDefault: true),
                    location));
                _languageIsSupported = false;
                return;
            }

            AttributeData = attributeData;

            TargetType = AttributeData.ConstructorArguments[0].Value as INamedTypeSymbol;

            Members = GetMembers(context, symbol);
        }

        public void AppendConstructors(SourceProductionContext context, StringBuilder sb, INamedTypeSymbol symbol, int indent)
        {
            if (!_languageIsSupported)
                return;

            var ctorSb = new StringBuilder();
            ctorSb.Append($"{GeneratorUtils.Indent(indent)}public {symbol.Name}");

            var ns = TargetType.ContainingNamespace;
            var nsName = TargetType.ContainingType == null
                ? ns.IsGlobalNamespace ? "" : $"{ns.ToDisplayString()}."
                : $"{TargetType.ContainingType.ToDisplayString()}.";
            ctorSb.AppendLine($"({nsName}{TargetType.Name} value)");
            ctorSb.AppendLine($"{GeneratorUtils.Indent(indent)}{{");
            indent++;

            foreach (var member in Members)
            {
                ctorSb.AppendLine($"{GeneratorUtils.Indent(indent)}{member.Alias} = value.{member.Symbol.Name};");
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
                var accessibility = GeneratorUtils.AccessibilityToString(member.Symbol);

                var memberType = GeneratorUtils.GetMemberType(member.Symbol);

                sb.AppendLine(GetFormatedPropertyLine(indent, accessibility, memberType, member.Alias));
            }
        }

        protected virtual string GetFormatedPropertyLine(int indent, string accessibility, string memberType, string memberName)
        {
            return $"{GeneratorUtils.Indent(indent)}{accessibility} {memberType} {memberName} {{ get; set; }}";
        }

        protected abstract IReadOnlyList<SymbolWithAlias> GetMembers(SourceProductionContext context, INamedTypeSymbol symbol);

        protected class SymbolWithAlias
        {
            public ISymbol Symbol { get; set; }
            
            public string Alias { get; set; }
        }
    }
}