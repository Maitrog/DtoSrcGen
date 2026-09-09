using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace DtoSrcGen
{
    internal class RequiredGenerator : GeneratorBase
    {
        public override string AttributeName => "RequiredAttribute";

        protected override LanguageVersion MinLanguageVersion => LanguageVersion.CSharp11;

        protected override IReadOnlyList<SymbolWithAlias> GetMembers(SourceProductionContext context, INamedTypeSymbol symbol)
        {
            var members = TargetType.GetMembers();

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
                    TargetType.Name));
            }
            
            return members.Where(x
                => x.Kind is SymbolKind.Property or SymbolKind.Field
                   && x.DeclaredAccessibility is Accessibility.Public
                   && !x.IsStatic
                   && !x.IsImplicitlyDeclared)
                          .Select(x => new SymbolWithAlias { Symbol = x, Alias = x.Name })
                          .ToList();
        }

        protected override string GetFormatedPropertyLine(int indent, string accessibility, string memberType, string memberName)
        {
            return $"{GeneratorUtils.Indent(indent)}{accessibility} required {memberType} {memberName} {{ get; set; }}";
        }
    }
}