using System.Linq;
using Microsoft.CodeAnalysis;

namespace DtoSrcGen
{
    internal static class GeneratorUtils
    {
        public static string Indent(int level) => new(' ', level * 4);

        public static string AccessibilityToString(ISymbol symbol)
        {
            var acc = symbol.DeclaredAccessibility;

            if (acc == Accessibility.NotApplicable)
                return symbol.ContainingType is null ? "internal" : "private";

            return acc switch
                   {
                       Accessibility.Public => "public",
                       Accessibility.Internal => "internal",
                       Accessibility.Private => "private",
                       Accessibility.Protected => "protected",
                       Accessibility.ProtectedAndInternal => "private protected",
                       Accessibility.ProtectedOrInternal => "protected internal",
                       _ => symbol.ContainingType is null ? "internal" : "private",
                   };
        }
        
        public static Accessibility GetEffectiveAccessibility(ISymbol symbol)
        {
            var acc = symbol.DeclaredAccessibility;

            for (var type = symbol.ContainingType; type is not null; type = type.ContainingType)
            {
                acc = Restrict(acc, type.DeclaredAccessibility);

                if (acc == Accessibility.Private)
                    break;
            }

            return acc;
        }

        private static Accessibility Restrict(Accessibility acc, Accessibility container)
        {
            return container switch
                   {
                       Accessibility.Internal => acc switch
                       {
                           Accessibility.Public or Accessibility.ProtectedOrInternal => Accessibility.Internal,
                           Accessibility.Protected => Accessibility.ProtectedAndInternal,
                           _ => acc,
                       },
                       Accessibility.Protected => acc switch
                       {
                           Accessibility.Public or Accessibility.ProtectedOrInternal => Accessibility.Protected,
                           Accessibility.Internal => Accessibility.ProtectedAndInternal,
                           _ => acc,
                       },
                       Accessibility.ProtectedOrInternal => acc is Accessibility.Public
                           ? Accessibility.ProtectedOrInternal
                           : acc,
                       Accessibility.ProtectedAndInternal => acc is Accessibility.Private
                           ? Accessibility.Private
                           : Accessibility.ProtectedAndInternal,
                       Accessibility.Private => Accessibility.Private,
                       _ => acc,
                   };
        }

        public static string GetMemberType(ISymbol member)
        {
            var memberType = member.Kind switch
                             {
                                 SymbolKind.Property => (member as IPropertySymbol)?.Type.ToDisplayString(),
                                 SymbolKind.Field => (member as IFieldSymbol)?.Type.ToDisplayString(),
                             };
            return memberType;
        }
    }
}