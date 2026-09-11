namespace DtoSrcGen.Tests;

public class ReadonlyGeneratorTests : DtoGeneratorTestsBase
{
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
    public void Readonly_NestedDtoClass_GeneratesGetOnlyProperties()
    {
        var (_, generated, errors) = RunAndCompile("""
            using Test.Entities;

            namespace Test
            {
                public partial class Outer
                {
                    [DtoSrcGen.Readonly(typeof(Chat))]
                    public partial class ReadonlyChatDto { }
                }
            }
            """);

        var text = generated("ReadonlyAttribute");
        Assert.Contains("public partial class Outer", text);
        Assert.Contains("public int Id { get; }", text);
        Assert.DoesNotContain("{ get; set; }", text);
        Assert.True(errors.Length == 0, string.Join(Environment.NewLine, errors));
    }

    [Fact]
    public void Readonly_NestedEntity_UsesFullyQualifiedTypeInCtor()
    {
        var (_, generated) = Run("""
            namespace Test
            {
                [DtoSrcGen.Readonly(typeof(Test.Entities.Container.Chat))]
                public partial class ReadonlyChatDto { }
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

        var text = generated("ReadonlyAttribute");
        Assert.Contains("public ReadonlyChatDto(Test.Entities.Container.Chat value)", text);
        Assert.Contains("public int Id { get; }", text);
    }
}
