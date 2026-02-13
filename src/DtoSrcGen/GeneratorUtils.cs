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
    }
}