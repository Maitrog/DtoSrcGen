namespace DtoSrcGen.Tests;

public class RequiredGeneratorTests : DtoGeneratorTestsBase
{
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
    public void Required_IncludesInternalMembersOfInternalClass()
    {
        var (result, generated) = Run("""
            using Test.Entities;

            namespace Test
            {
                [DtoSrcGen.Required(typeof(InternalUser))]
                internal partial class RequiredInternalUserDto { }
            }
            """, """
            namespace Test.Entities
            {
                internal class InternalUser
                {
                    public int Id { get; set; }
                    internal string Secret { get; set; }
                    protected internal string Token { get; set; }
                    protected string Family { get; set; }
                }
            }
            """);

        var text = generated("RequiredAttribute");
        Assert.Contains("public required int Id { get; set; }", text);
        Assert.Contains("internal required string Secret { get; set; }", text);
        Assert.Contains("protected internal required string Token { get; set; }", text);
        Assert.DoesNotContain("Family", text);
        Assert.False(HasDiagnostic(result, "DSG2001"));
    }

    [Fact]
    public void Required_NestedDtoClass_GeneratesRequiredProperties()
    {
        var (result, generated, errors) = RunAndCompile("""
            using Test.Entities;

            namespace Test
            {
                public partial class Outer
                {
                    [DtoSrcGen.Required(typeof(Chat))]
                    public partial class RequiredChatDto { }
                }
            }
            """);

        var text = generated("RequiredAttribute");
        Assert.Contains("public partial class Outer", text);
        Assert.Contains("public required int Id { get; set; }", text);
        Assert.False(HasDiagnostic(result, "DSG2001"));
        Assert.True(errors.Length == 0, string.Join(Environment.NewLine, errors));
    }

    [Fact]
    public void Required_NestedEntity_UsesFullyQualifiedTypeInCtor()
    {
        var (result, generated) = Run("""
            namespace Test
            {
                [DtoSrcGen.Required(typeof(Test.Entities.Container.Chat))]
                public partial class RequiredChatDto { }
            }
            """, """
            namespace Test.Entities
            {
                public class Container
                {
                    public class Chat
                    {
                        public int Id { get; set; }
                        public string Created { get; set; }
                    }
                }
            }
            """);

        var text = generated("RequiredAttribute");
        Assert.Contains("public RequiredChatDto(Test.Entities.Container.Chat value)", text);
        Assert.Contains("public required int Id { get; set; }", text);
        Assert.False(HasDiagnostic(result, "DSG2001"));
    }
}
