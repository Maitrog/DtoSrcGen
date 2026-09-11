namespace DtoSrcGen.Tests;

public class UnionGeneratorTests : DtoGeneratorTestsBase
{
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

    [Fact]
    public void Union_NestedDtoClass_MergesPropertiesInsideOuterClass()
    {
        var (_, generated, errors) = RunAndCompile("""
            using Test.Entities;

            namespace Test
            {
                public partial class Outer
                {
                    [DtoSrcGen.Union(typeof(Chat), typeof(Flags))]
                    public partial class ChatWithFlagsDto { }
                }
            }
            """);

        var text = generated("UnionAttribute");
        Assert.Contains("public partial class Outer", text);
        Assert.Contains("public int Id { get; set; }", text);
        Assert.Contains("public string Value { get; set; }", text);
        Assert.Contains("public ChatWithFlagsDto(Test.Entities.Chat value_1, Test.Entities.Flags value_2)", text);
        Assert.True(errors.Length == 0, string.Join(Environment.NewLine, errors));
    }

    [Fact]
    public void Union_NestedEntity_UsesFullyQualifiedTypeInCtor()
    {
        var (_, generated) = Run("""
            namespace Test
            {
                [DtoSrcGen.Union(typeof(Test.Entities.Container.Chat), typeof(Test.Entities.Container.Flags))]
                public partial class ChatWithFlagsDto { }
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

                    public class Flags
                    {
                        public string Value { get; set; }
                    }
                }
            }
            """);

        var text = generated("UnionAttribute");
        Assert.Contains("public ChatWithFlagsDto(Test.Entities.Container.Chat value_1, Test.Entities.Container.Flags value_2)", text);
        Assert.Contains("public int Id { get; set; }", text);
        Assert.Contains("public string Value { get; set; }", text);
    }
}
