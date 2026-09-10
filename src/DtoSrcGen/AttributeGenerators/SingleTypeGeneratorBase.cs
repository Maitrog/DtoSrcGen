using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace DtoSrcGen
{
    internal abstract class SingleTypeGeneratorBase : GeneratorBase, IAttributeGenerator
    {
        private bool _languageIsSupported = true;

        protected virtual LanguageVersion MinLanguageVersion => LanguageVersion.CSharp8;

        private IReadOnlyList<SymbolWithAlias> Members { get; set; }
        
        protected INamedTypeSymbol TargetType { get; private set; }

        public void Pre(SourceProductionContext context, LanguageVersion currentLanguageVersion, INamedTypeSymbol symbol)
        {
            if (currentLanguageVersion <  MinLanguageVersion)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    new DiagnosticDescriptor(
                        "DSG3002",
                        "Not supported",
                        $"\'{AttributeName}\' is supported from C# {MinLanguageVersion.ToDisplayString()}.",
                        "DtoSrcGen",
                        DiagnosticSeverity.Error,
                        isEnabledByDefault: true),
                    symbol.Locations.First()));
                _languageIsSupported = false;
                return;
            }

            var attributes = symbol.GetAttributes();
            
            AttributeData = attributes.FirstOrDefault(x => x.AttributeClass?.Name == AttributeName);
            
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
            var nsName = ns.IsGlobalNamespace ? "" : $"{ns.ToDisplayString()}.";
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