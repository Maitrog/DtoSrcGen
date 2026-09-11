namespace DtoSrcGen.Tests;

public class PartialGeneratorTests : DtoGeneratorTestsBase
{
    [Fact]
    public void Partial_GeneratesAllPublicPropertiesAsNullable()
    {
        var (result, generated) = Run("""
            using Test.Entities;

            namespace Test
            {
                [DtoSrcGen.Partial(typeof(User))]
                public partial class PartialUserDto { }
            }
            """);

        var text = generated("PartialAttribute");
        Assert.Contains("public int? Id { get; set; }", text);
        Assert.Contains("public string? Name { get; set; }", text);
        Assert.Contains("public string? Email { get; set; }", text);
        Assert.Contains("internal string? Secret { get; set; }", text);
        Assert.Contains("public PartialUserDto(Test.Entities.User value)", text);
        Assert.Contains("Id = value.Id;", text);
        Assert.Contains("Name = value.Name;", text);
        Assert.False(HasDiagnostic(result, "DSG3000"));
    }

    [Fact]
    public void Partial_IncludesFieldsAndSkipsStaticMembers()
    {
        var (_, generated) = Run("""
            using Test.Entities;

            namespace Test
            {
                [DtoSrcGen.Partial(typeof(Entity))]
                public partial class EntityDto { }
            }
            """, """
            namespace Test.Entities
            {
                public class Entity
                {
                    public int Count;
                    public string Label { get; set; }
                    public static int StaticCount;
                    public static string StaticLabel { get; set; }
                }
            }
            """);

        var text = generated("PartialAttribute");
        Assert.Contains("public int? Count { get; set; }", text);
        Assert.Contains("public string? Label { get; set; }", text);
        Assert.DoesNotContain("StaticCount", text);
        Assert.DoesNotContain("StaticLabel", text);
    }

    [Fact]
    public void Partial_AlreadyNullableType_NotDoubled()
    {
        var (_, generated) = Run("""
            using Test.Entities;

            namespace Test
            {
                [DtoSrcGen.Partial(typeof(Entity))]
                public partial class EntityDto { }
            }
            """, """
            namespace Test.Entities
            {
                public class Entity
                {
                    public string? Nickname { get; set; }
                }
            }
            """);

        var text = generated("PartialAttribute");
        Assert.Contains("public string? Nickname { get; set; }", text);
        Assert.DoesNotContain("??", text);
    }

    [Fact]
    public void Partial_GenerateDefaultCtorDefault_EmitsDefaultCtor()
    {
        var (_, generated) = Run("""
            using Test.Entities;

            namespace Test
            {
                [DtoSrcGen.Partial(typeof(User))]
                public partial class PartialUserDto { }
            }
            """);

        Assert.Contains("public PartialUserDto() {}", generated("PartialAttribute"));
    }

    [Fact]
    public void Partial_GenerateDefaultCtorFalse_OmitsDefaultCtor()
    {
        var (_, generated) = Run("""
            using Test.Entities;

            namespace Test
            {
                [DtoSrcGen.Partial(typeof(User), GenerateDefaultCtor = false)]
                public partial class PartialUserDto { }
            }
            """);

        Assert.DoesNotContain("public PartialUserDto() {}", generated("PartialAttribute"));
    }

    [Fact]
    public void Partial_NestedEntity_UsesFullyQualifiedTypeInCtor()
    {
        var (_, generated) = Run("""
            namespace Test
            {
                [DtoSrcGen.Partial(typeof(Test.Entities.Container.User))]
                public partial class PartialUserDto { }
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

        var text = generated("PartialAttribute");
        Assert.Contains("public PartialUserDto(Test.Entities.Container.User value)", text);
        Assert.Contains("public int? Id { get; set; }", text);
        Assert.Contains("Id = value.Id;", text);
    }

    [Fact]
    public void Partial_NestedDtoClass_GeneratesPropertiesInsideOuterClass()
    {
        var (_, generated, errors) = RunAndCompile("""
            using Test.Entities;

            namespace Test
            {
                public partial class Outer
                {
                    [DtoSrcGen.Partial(typeof(User))]
                    public partial class PartialUserDto { }
                }
            }
            """);

        var text = generated("PartialAttribute");
        Assert.Contains("public partial class Outer", text);
        Assert.Contains("public int? Id { get; set; }", text);
        Assert.True(errors.Length == 0, string.Join(Environment.NewLine, errors));
    }
}
