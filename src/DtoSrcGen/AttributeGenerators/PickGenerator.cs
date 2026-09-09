using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace DtoSrcGen
{
    internal class PickGenerator : GeneratorBase
    {
        public override string AttributeName => "PickAttribute";

        protected override IReadOnlyList<SymbolWithAlias> GetMembers(SourceProductionContext context, INamedTypeSymbol symbol)
        {
            var properties = AttributeData.ConstructorArguments[1].Values.Select(x => x.Value as string).ToList();
            var members = new List<SymbolWithAlias>();

            foreach (var property in properties)
            {
                var originalName = property;
                var alias = property;
                var splitName = property.Split(' ');
                if (splitName.Length == 3 && splitName[1] == "as")
                {
                    alias = splitName[2];
                    originalName = splitName[0];
                }

                var member = TargetType.GetMembers(originalName).FirstOrDefault(x
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
                        TargetType.Name,
                        originalName));
                    continue;
                }

                members.Add(new SymbolWithAlias { Symbol = member, Alias = alias });
            }

            return members;
        }
    }
}