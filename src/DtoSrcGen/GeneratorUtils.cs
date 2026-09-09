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

        public static bool GetGenerateDefaultCtor(AttributeData attributeData)
        {
            if (attributeData is null)
                return true;

            var namedArgument = attributeData.NamedArguments.FirstOrDefault(x => x.Key == "GenerateDefaultCtor");
            if (namedArgument.Key == "GenerateDefaultCtor" && namedArgument.Value.Value is bool generateDefaultCtor)
                return generateDefaultCtor;

            return true;
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