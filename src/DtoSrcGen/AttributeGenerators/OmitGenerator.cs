using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace DtoSrcGen
{
    internal class OmitGenerator : SingleTypeGeneratorBase
    {
        public override string AttributeName => "OmitAttribute";

        protected override IReadOnlyList<SymbolWithAlias> GetMembers(SourceProductionContext context, INamedTypeSymbol symbol)
        {
            var properties = AttributeData.ConstructorArguments[1].Values.Select(x => x.Value as string).ToList();

            var members = TargetType.GetMembers();

            return members.Where(x
                => x.Kind is SymbolKind.Property or SymbolKind.Field
                   && x.DeclaredAccessibility is Accessibility.Public or Accessibility.Internal or Accessibility.ProtectedOrInternal
                   && !x.IsImplicitlyDeclared
                   && !x.IsStatic
                   && !properties.Contains(x.Name))
                          .Select(x => new SymbolWithAlias { Symbol = x, Alias = x.Name })
                          .ToList();
        }
    }
}