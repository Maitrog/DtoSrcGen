namespace DtoSrcGen.Tests;

public class PickGeneratorTests : DtoGeneratorTestsBase
{
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
    public void Pick_NestedDtoClass_GeneratesPropertiesInsideOuterClass()
    {
        var (result, generated, errors) = RunAndCompile("""
            using Test.Entities;

            namespace Test
            {
                public partial class Outer
                {
                    [DtoSrcGen.Pick(typeof(User), "Id as UserId", "Name")]
                    public partial class UserPickDto { }
                }
            }
            """);

        var text = generated("PickAttribute");
        Assert.Contains("public partial class Outer", text);
        Assert.Contains("public partial class UserPickDto", text);
        Assert.Contains("public int UserId { get; set; }", text);
        Assert.Contains("UserId = value.Id;", text);
        Assert.False(HasDiagnostic(result, "DSG3000"));
        Assert.True(errors.Length == 0, string.Join(Environment.NewLine, errors));
    }

    [Fact]
    public void Pick_MultipleAttributesOnSameClass_GeneratesOneFilePerAttribute()
    {
        var (result, _, errors) = RunAndCompile("""
            using Test.Entities;

            namespace Test
            {
                [DtoSrcGen.Pick(typeof(User), "Id as UserId")]
                [DtoSrcGen.Pick(typeof(Chat), "Id as ChatId", "Created", GenerateDefaultCtor = false)]
                public partial class MultiDto { }
            }
            """);

        var pickFiles = result.Results
                              .SelectMany(r => r.GeneratedSources)
                              .Where(s => s.HintName.Contains("PickAttribute"))
                              .OrderBy(s => s.HintName)
                              .ToList();

        Assert.Equal(2, pickFiles.Count);
        Assert.EndsWith(".PickAttribute.1.Fields.g.cs", pickFiles[0].HintName);
        Assert.EndsWith(".PickAttribute.2.Fields.g.cs", pickFiles[1].HintName);

        var first = pickFiles[0].SourceText.ToString();
        var second = pickFiles[1].SourceText.ToString();

        Assert.Contains("public int UserId { get; set; }", first);
        Assert.Contains("public MultiDto(Test.Entities.User value)", first);
        Assert.Contains("public int ChatId { get; set; }", second);
        Assert.Contains("public string Created { get; set; }", second);
        Assert.Contains("public MultiDto(Test.Entities.Chat value)", second);

        Assert.False(HasDiagnostic(result, "DSG3000"));
        Assert.True(errors.Length == 0, string.Join(Environment.NewLine, errors));
    }

    [Fact]
    public void Pick_NestedEntity_UsesFullyQualifiedTypeInCtor()
    {
        var (_, generated) = Run("""
            namespace Test
            {
                [DtoSrcGen.Pick(typeof(Test.Entities.Container.User), "Id as UserId")]
                public partial class UserPickDto { }
            }
            """, """
            namespace Test.Entities
            {
                public class Container
                {
                    public class User
                    {
                        public int Id { get; set; }
                    }
                }
            }
            """);

        var text = generated("PickAttribute");
        Assert.Contains("public UserPickDto(Test.Entities.Container.User value)", text);
        Assert.Contains("public int UserId { get; set; }", text);
        Assert.Contains("UserId = value.Id;", text);
    }
}
