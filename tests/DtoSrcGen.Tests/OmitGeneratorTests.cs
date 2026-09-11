namespace DtoSrcGen.Tests;

public class OmitGeneratorTests : DtoGeneratorTestsBase
{
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
    public void Omit_NestedDtoClass_GeneratesPropertiesInsideOuterClass()
    {
        var (_, generated, errors) = RunAndCompile("""
            using Test.Entities;

            namespace Test
            {
                public partial class Outer
                {
                    [DtoSrcGen.Omit(typeof(User), "Flags")]
                    public partial class UserWithoutFlagsDto { }
                }
            }
            """);

        var text = generated("OmitAttribute");
        Assert.Contains("public partial class Outer", text);
        Assert.Contains("public int Id { get; set; }", text);
        Assert.DoesNotContain("public string Flags { get; set; }", text);
        Assert.True(errors.Length == 0, string.Join(Environment.NewLine, errors));
    }

    [Fact]
    public void Omit_NestedEntity_UsesFullyQualifiedTypeInCtor()
    {
        var (_, generated) = Run("""
            namespace Test
            {
                [DtoSrcGen.Omit(typeof(Test.Entities.Container.User), "Flags")]
                public partial class UserWithoutFlagsDto { }
            }
            """, """
            namespace Test.Entities
            {
                public class Container
                {
                    public class User
                    {
                        public int Id { get; set; }
                        public string Flags { get; set; }
                    }
                }
            }
            """);

        var text = generated("OmitAttribute");
        Assert.Contains("public UserWithoutFlagsDto(Test.Entities.Container.User value)", text);
        Assert.Contains("public int Id { get; set; }", text);
        Assert.DoesNotContain("public string Flags { get; set; }", text);
    }
}
