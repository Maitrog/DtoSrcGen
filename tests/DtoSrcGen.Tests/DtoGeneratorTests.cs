using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using DtoSrcGen;

namespace DtoSrcGen.Tests;

public class DtoGeneratorTests
{
    private const string Entities = """
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

    private static (GeneratorDriverRunResult Result, Func<string, string> Generated) Run(
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

        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out _);

        var result = driver.GetRunResult();
        return (result, attributeName => result.Results
            .SelectMany(r => r.GeneratedSources)
            .First(s => s.HintName.EndsWith($".{attributeName}.Fields.g.cs"))
            .SourceText.ToString());
    }

    private static bool HasDiagnostic(GeneratorDriverRunResult result, string id, string contains = "")
    {
        return result.Diagnostics.Any(d => d.Id == id && d.GetMessage().Contains(contains));
    }

    [Fact]
    public void Pick_GeneratesOnlyPickedProperties()
    {
        var (result, generated) = Run("""
            using Test.Entities;

            namespace Test
            {
                [DtoSrcGen.Pick(typeof(User), "Id as UserId", "Name")]
                public partial class UserPickDto { }
            }
            """);

        var text = generated("PickAttribute");
        Assert.Contains("public int UserId { get; set; }", text);
        Assert.Contains("public string Name { get; set; }", text);
        Assert.Contains("public UserPickDto(Test.Entities.User value)", text);
        Assert.Contains("UserId = value.Id;", text);
        Assert.Contains("Name = value.Name;", text);
        Assert.DoesNotContain("Email", text);
        Assert.DoesNotContain("Flags", text);
        Assert.DoesNotContain("Secret", text);
        Assert.False(HasDiagnostic(result, "DSG3000"));
    }

    [Fact]
    public void Pick_ReportsDiagnosticForMissingMember()
    {
        var (result, _) = Run("""
            using Test.Entities;

            namespace Test
            {
                [DtoSrcGen.Pick(typeof(User), "Nope")]
                public partial class UserPickDto { }
            }
            """);

        Assert.True(HasDiagnostic(result, "DSG3000", "Nope"));
    }

    [Fact]
    public void Pick_GenerateDefaultCtorFalse_OmitsDefaultCtor()
    {
        var (_, generated) = Run("""
            using Test.Entities;

            namespace Test
            {
                [DtoSrcGen.Pick(typeof(User), "Id", GenerateDefaultCtor = false)]
                public partial class UserPickDto { }
            }
            """);

        Assert.DoesNotContain("public UserPickDto() {}", generated("PickAttribute"));
    }

    [Fact]
    public void Pick_GenerateDefaultCtorDefault_EmitsDefaultCtor()
    {
        var (_, generated) = Run("""
            using Test.Entities;

            namespace Test
            {
                [DtoSrcGen.Pick(typeof(User), "Id")]
                public partial class UserPickDto { }
            }
            """);

        Assert.Contains("public UserPickDto() {}", generated("PickAttribute"));
    }

    [Fact]
    public void Omit_GeneratesAllPropertiesExceptOmitted()
    {
        var (_, generated) = Run("""
            using Test.Entities;

            namespace Test
            {
                [DtoSrcGen.Omit(typeof(User), "Flags")]
                public partial class UserWithoutFlagsDto { }
            }
            """);

        var text = generated("OmitAttribute");
        Assert.Contains("public int Id { get; set; }", text);
        Assert.Contains("public string Name { get; set; }", text);
        Assert.Contains("public string Email { get; set; }", text);
        Assert.Contains("internal string Secret { get; set; }", text);
        Assert.DoesNotContain("public string Flags { get; set; }", text);
    }

    [Fact]
    public void Readonly_GeneratesGetOnlyProperties()
    {
        var (_, generated) = Run("""
            using Test.Entities;

            namespace Test
            {
                [DtoSrcGen.Readonly(typeof(Chat))]
                public partial class ReadonlyChatDto { }
            }
            """);

        var text = generated("ReadonlyAttribute");
        Assert.Contains("public int Id { get; }", text);
        Assert.Contains("public string Created { get; }", text);
        Assert.Contains("public string Updated { get; }", text);
        Assert.DoesNotContain("{ get; set; }", text);
    }

    [Fact]
    public void Required_GeneratesRequiredProperties()
    {
        var (result, generated) = Run("""
            using Test.Entities;

            namespace Test
            {
                [DtoSrcGen.Required(typeof(Chat))]
                public partial class RequiredChatDto { }
            }
            """);

        var text = generated("RequiredAttribute");
        Assert.Contains("public required int Id { get; set; }", text);
        Assert.Contains("public required string Created { get; set; }", text);
        Assert.False(HasDiagnostic(result, "DSG2001"));
    }

    [Fact]
    public void Required_WarnsOnInternalMembers()
    {
        var (result, _) = Run("""
            using Test.Entities;

            namespace Test
            {
                [DtoSrcGen.Required(typeof(User))]
                public partial class RequiredUserDto { }
            }
            """);

        var text = result.Results
            .SelectMany(r => r.GeneratedSources)
            .First(s => s.HintName.EndsWith(".RequiredAttribute.Fields.g.cs"))
            .SourceText.ToString();
        Assert.DoesNotContain("Secret", text);
        Assert.True(HasDiagnostic(result, "DSG2001", "User"));
    }

    [Fact]
    public void Union_MergesPropertiesFromAllTypes()
    {
        var (_, generated) = Run("""
            using Test.Entities;

            namespace Test
            {
                [DtoSrcGen.Union(typeof(Chat), typeof(Flags))]
                public partial class ChatWithFlagsDto { }
            }
            """);

        var text = generated("UnionAttribute");
        Assert.Contains("public int Id { get; set; }", text);
        Assert.Contains("public string Created { get; set; }", text);
        Assert.Contains("public string Value { get; set; }", text);
        Assert.Contains("public ChatWithFlagsDto(Test.Entities.Chat value_1, Test.Entities.Flags value_2)", text);
        Assert.Contains("Id = value_1.Id;", text);
        Assert.Contains("Value = value_2.Value;", text);
    }

    [Fact]
    public void Union_WarnsOnDuplicateMemberWithSameType()
    {
        var (result, generated) = Run("""
            namespace Test.Entities
            {
                public class A { public int Id { get; set; } }
                public class B { public int Id { get; set; } }
            }

            namespace Test
            {
                [DtoSrcGen.Union(typeof(Test.Entities.A), typeof(Test.Entities.B))]
                public partial class UnionDto { }
            }
            """);

        Assert.True(HasDiagnostic(result, "DSG2000", "Id"));
        Assert.Equal(1, generated("UnionAttribute").Split("public int Id { get; set; }").Length - 1);
    }

    [Fact]
    public void Union_ReportsErrorOnDuplicateMemberWithDifferentType()
    {
        var (result, _) = Run("""
            namespace Test.Entities
            {
                public class A { public int Id { get; set; } }
                public class B { public string Id { get; set; } }
            }

            namespace Test
            {
                [DtoSrcGen.Union(typeof(Test.Entities.A), typeof(Test.Entities.B))]
                public partial class UnionDto { }
            }
            """);

        Assert.True(HasDiagnostic(result, "DSG3001", "Id"));
    }
}
