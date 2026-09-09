using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace DtoSrcGen
{
    internal class ReadonlyGenerator : GeneratorBase
    {
        public override string AttributeName => "ReadonlyAttribute";

        protected override IReadOnlyList<SymbolWithAlias> GetMembers(SourceProductionContext context, INamedTypeSymbol symbol)
        {
            var members = TargetType.GetMembers();

            return members.Where(x
                => x.Kind is SymbolKind.Property or SymbolKind.Field
                   && x.DeclaredAccessibility is Accessibility.Public or Accessibility.Internal or Accessibility.ProtectedOrInternal
                   && !x.IsStatic
                   && !x.IsImplicitlyDeclared)
                          .Select(x => new SymbolWithAlias { Symbol = x, Alias = x.Name })
                          .ToList();
        }

        protected override string GetFormatedPropertyLine(int indent, string accessibility, string memberType, string memberName)
        {
            return $"{GeneratorUtils.Indent(indent)}{accessibility} {memberType} {memberName} {{ get; }}";
        }
    }
}