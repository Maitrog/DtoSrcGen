using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using DtoSrcGen;

namespace DtoSrcGen.Tests;

public abstract class DtoGeneratorTestsBase
{
    protected const string Entities = """
        namespace Test.Entities
        {
            public class User
            {
                public int Id { get; set; }
                public string Name { get; set; }
                public string Email { get; set; }
                public string Flags { get; set; }
                internal string Secret { get; set; }
            }

            public class Chat
            {
                public int Id { get; set; }
                public string Created { get; set; }
                public string Updated { get; set; }
            }

            public class Flags
            {
                public string Value { get; set; }
            }
        }
        """;

    protected static (GeneratorDriverRunResult Result, Func<string, string> Generated) Run(
        string dtoSource,
        string? entities = null)
    {
        var (result, generated, _) = RunAndCompile(dtoSource, entities);
        return (result, generated);
    }

    protected static (GeneratorDriverRunResult Result, Func<string, string> Generated, string[] Errors) RunAndCompile(
        string dtoSource,
        string? entities = null)
    {
        var references = new List<MetadataReference>
        {
            MetadataReference.CreateFromFile(typeof(OmitAttribute).Assembly.Location),
        };
        foreach (var path in ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES"))!.Split(Path.PathSeparator))
            references.Add(MetadataReference.CreateFromFile(path));

        var parseOptions = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp11);
        var compilation = CSharpCompilation.Create(
            "Tests",
            new[]
            {
                CSharpSyntaxTree.ParseText(entities ?? Entities, parseOptions),
                CSharpSyntaxTree.ParseText(dtoSource, parseOptions),
            },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            new[] { new DtoGenerator().AsSourceGenerator() },
            parseOptions: parseOptions);

        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out _);

        var result = driver.GetRunResult();
        var errors = outputCompilation.GetDiagnostics()
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .Select(d => d.ToString())
            .ToArray();
        return (result, attributeName => result.Results
            .SelectMany(r => r.GeneratedSources)
            .First(s => s.HintName.EndsWith($".{attributeName}.Fields.g.cs"))
            .SourceText.ToString(), errors);
    }

    protected static bool HasDiagnostic(GeneratorDriverRunResult result, string id, string contains = "")
    {
        return result.Diagnostics.Any(d => d.Id == id && d.GetMessage().Contains(contains));
    }
}
