namespace DtoSrcGen.Tests;

public class RecordGeneratorTests : DtoGeneratorTestsBase
{
    private const string EnumEntities = """
        namespace Test.Entities
        {
            public enum Gender
            {
                Male,
                Female,
                Other,
            }

            public class User
            {
                public int Id { get; set; }
                public string Name { get; set; }
            }
        }
        """;

    [Fact]
    public void Record_GeneratesPropertyPerEnumMember()
    {
        var (_, generated) = Run("""
            using Test.Entities;

            namespace Test
            {
                [DtoSrcGen.Record(typeof(Gender), typeof(User))]
                public partial class RecordGenderDto { }
            }
            """, EnumEntities);

        var text = generated("RecordAttribute");
        Assert.Contains("public Test.Entities.User Male { get; set; }", text);
        Assert.Contains("public Test.Entities.User Female { get; set; }", text);
        Assert.Contains("public Test.Entities.User Other { get; set; }", text);
        Assert.DoesNotContain("{ get; }", text);
    }

    [Fact]
    public void Record_ValidEnum_ReportsNoDiagnostic()
    {
        var (result, _, errors) = RunAndCompile("""
            using Test.Entities;

            namespace Test
            {
                [DtoSrcGen.Record(typeof(Gender), typeof(User))]
                public partial class RecordGenderDto { }
            }
            """, EnumEntities);

        Assert.False(HasDiagnostic(result, "DSG3003"));
        Assert.True(errors.Length == 0, string.Join(Environment.NewLine, errors));
    }

    [Fact]
    public void Record_NonEnumSourceType_ReportsDiagnostic()
    {
        var (result, _) = Run("""
            using Test.Entities;

            namespace Test
            {
                [DtoSrcGen.Record(typeof(User), typeof(User))]
                public partial class RecordGenderDto { }
            }
            """, EnumEntities);

        Assert.True(HasDiagnostic(result, "DSG3003", "User"));
    }

    [Fact]
    public void Record_NonEnumSourceType_DiagnosticPointsToFirstArgument()
    {
        var (result, _) = Run("""
            using Test.Entities;

            namespace Test
            {
                [DtoSrcGen.Record(typeof(User), typeof(User))]
                public partial class RecordGenderDto { }
            }
            """, EnumEntities);

        var diagnostic = result.Diagnostics.First(d => d.Id == "DSG3003");
        var text = diagnostic.Location.SourceTree!.GetText().ToString(diagnostic.Location.SourceSpan);

        Assert.Equal("typeof(User)", text);
    }

    [Fact]
    public void Record_GenerateDefaultCtorDefault_EmitsDefaultCtor()
    {
        var (_, generated) = Run("""
            using Test.Entities;

            namespace Test
            {
                [DtoSrcGen.Record(typeof(Gender), typeof(User))]
                public partial class RecordGenderDto { }
            }
            """, EnumEntities);

        Assert.Contains("public RecordGenderDto() {}", generated("RecordAttribute"));
    }

    [Fact]
    public void Record_GenerateDefaultCtorFalse_OmitsDefaultCtor()
    {
        var (_, generated) = Run("""
            using Test.Entities;

            namespace Test
            {
                [DtoSrcGen.Record(typeof(Gender), typeof(User), GenerateDefaultCtor = false)]
                public partial class RecordGenderDto { }
            }
            """, EnumEntities);

        Assert.DoesNotContain("public RecordGenderDto() {}", generated("RecordAttribute"));
    }
}
