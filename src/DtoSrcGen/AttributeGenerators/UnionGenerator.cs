using System.Collections.Generic;
using System.Text;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace DtoSrcGen
{
    public class UnionGenerator : IAttributeGenerator
    {
        public string AttributeName => "UnionAttribute";

        public string AttributeNameWithNamespace => "DtoSrcGen.UnionAttribute";

        private readonly Dictionary<string, int> _types = new();
        private readonly Dictionary<string, PropertyInfo> _properties = new();

        public void Pre(SourceProductionContext context, LanguageVersion currentLanguageVersion, INamedTypeSymbol symbol)
        {
            var attributes = symbol.GetAttributes();

            var attributeData = attributes.FirstOrDefault(x => x.AttributeClass?.Name == AttributeName);

            var types = attributeData.ConstructorArguments[0].Values.Select(x => x.Value as INamedTypeSymbol).ToList();

            var i = 1;
            foreach (var type in types)
            {
                var ns = type.ContainingNamespace;
                var nsName = ns.IsGlobalNamespace ? "" : $"{ns.ToDisplayString()}.";
                var typeName = $"{nsName}{type.Name}";
                if (_types.ContainsKey(typeName))
                    continue;

                _types.Add(typeName, i);

                var members = type.GetMembers();
                foreach (var member in members.Where(x
                             => x.Kind is SymbolKind.Property or SymbolKind.Field
                                && x.DeclaredAccessibility is Accessibility.Public or Accessibility.Internal or Accessibility.ProtectedOrInternal
                                && !x.IsStatic
                                && !x.IsImplicitlyDeclared))
                {
                    var memberType = member.Kind switch
                                     {
                                         SymbolKind.Property => (member as IPropertySymbol)?.Type.ToDisplayString(),
                                         SymbolKind.Field => (member as IFieldSymbol)?.Type.ToDisplayString(),
                                     };

                    var keyExists = _properties.ContainsKey(member.Name);
                    switch (keyExists)
                    {
                        case true when _properties[member.Name].TypeName != memberType:
                            context.ReportDiagnostic(Diagnostic.Create(
                                new DiagnosticDescriptor(
                                    "DSG3001",
                                    "Duplicate member declaration",
                                    "Type '{0}' contains a member named '{1}' already declared in another type with a different type.",
                                    "DtoSrcGen",
                                    DiagnosticSeverity.Error,
                                    isEnabledByDefault: true),
                                symbol.Locations.First(),
                                type.Name,
                                member.Name));
                            continue;
                        case true when _properties[member.Name].TypeName == memberType:
                            context.ReportDiagnostic(Diagnostic.Create(
                                new DiagnosticDescriptor(
                                    "DSG2000",
                                    "Duplicate member declaration",
                                    "Type '{0}' contains a member named '{1}' already declared in another type.",
                                    "DtoSrcGen",
                                    DiagnosticSeverity.Warning,
                                    isEnabledByDefault: true),
                                symbol.Locations.First(),
                                type.Name,
                                member.Name));
                            continue;
                        case false:
                            var accessibility = GeneratorUtils.AccessibilityToString(member);
                            _properties.Add(member.Name, new PropertyInfo(memberType, i, accessibility));
                            break;
                    }
                }

                i += 1;
            }
        }

        public void AppendConstructors(SourceProductionContext context, StringBuilder sb, INamedTypeSymbol symbol, int indent)
        {
            var ctorSb = new StringBuilder();
            ctorSb.Append($"{GeneratorUtils.Indent(indent)}public {symbol.Name}(");

            foreach (var type in _types)
            {
                ctorSb.Append($"{type.Key} value_{type.Value}, ");
            }
            ctorSb = ctorSb.Remove(ctorSb.Length - 2, 2);
            ctorSb.AppendLine($")\r\n{GeneratorUtils.Indent(indent)}{{");
            indent++;

            foreach (var property in _properties)
            {
                var name = property.Key;
                var valuePosition = property.Value.ConstructorValuePosition;
                ctorSb.AppendLine($"{GeneratorUtils.Indent(indent)}{name} = value_{valuePosition}.{name};");
            }

            indent--;
            ctorSb.AppendLine($"{GeneratorUtils.Indent(indent)}}}");

            sb.AppendLine(ctorSb.ToString());
        }

        public void AppendProperties(SourceProductionContext context, StringBuilder sb, INamedTypeSymbol symbol, int indent)
        {
            foreach (var property in _properties)
            {
                var name = property.Key;
                var accessibility = property.Value.Accessibility;
                var memberType = property.Value.TypeName;
                sb.AppendLine($"{GeneratorUtils.Indent(indent)}{accessibility} {memberType} {name} {{ get; set; }}");
            }
        }

        private class PropertyInfo
        {
            public PropertyInfo(string typeName, int constructorValuePosition, string accessibility)
            {
                TypeName = typeName;
                ConstructorValuePosition = constructorValuePosition;
                Accessibility = accessibility;
            }

            public string TypeName { get; set; }
            
            public int ConstructorValuePosition { get; set; }
            
            public string Accessibility { get; set; }
        }
    }
}