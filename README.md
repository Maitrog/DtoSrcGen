# DtoSrcGen

This project is inspired by Utility Types from Typescript.

Source generator that builds DTO partial classes from existing models using attributes.

## What it does
- Generates constructors and properties for partial classes annotated with attributes from `DtoSrcGen.Models`.
- Supports `Pick`, `Omit`, `Readonly`, `Required`, and `Union` patterns to shape DTOs without hand-written boilerplate.
- Supports `GenerateDefaultCtor` option on all attributes to control whether an empty DTO constructor is generated.
- Emits diagnostics when members are missing/duplicated and when language features (e.g., `required`) are unavailable.

## Getting started
1) Add references to your project file:
```
  <ItemGroup>
    <ProjectReference Include="..\src\DtoSrcGen.Models\DtoSrcGen.Models.csproj" />
    <ProjectReference Include="..\src\DtoSrcGen\DtoSrcGen.csproj"
                      OutputItemType="Analyzer"
                      ReferenceOutputAssembly="false" />
  </ItemGroup>
```

2) Annotate partial DTOs:
```csharp
using DtoSrcGen;

[Omit(typeof(User), nameof(User.Id), nameof(User.Age))]
public partial class DtoAge { }

[Pick(typeof(User), nameof(User.Id), nameof(User.Name))]
public partial class DtoName { }

[Readonly(typeof(User))]
public partial class ReadonlyUser { }

[Required(typeof(User))] // requires C# 11+
public partial class RequiredUser { }

[Union(typeof(User), typeof(Chat))]
public partial class UserChat { }

// Optional: disable generated empty ctor
[Pick(typeof(User), nameof(User.Id), nameof(User.Name), GenerateDefaultCtor = false)]
public partial class UserSummaryNoDefaultCtor { }

// using multiple attributes (GenerateDefaultCtor = false is required for all except one)
[Readonly(typeof(User))] 
[Omit(typeof(Chat), nameof(Chat.Id), GenerateDefaultCtor = false)]
public partial class ReadonlyUserWithChat { }
```

3) Use generated DTOs:
```csharp
var summary = new UserSummaryNoDefaultCtor(user);
// var summary2 = new UserSummaryNoDefaultCtor(); // no parameterless ctor when GenerateDefaultCtor = false
```

## Attribute behavior
- **Pick**: include only the listed fields/properties from the source type; generated ctor copies those members.
- **Omit**: include all eligible members except the listed ones.
- **Readonly**: include all eligible members with getters only.
- **Required**: include all public members and mark them `required`; emits an error if language version < C# 11.
- **Union**: merges members from multiple types; warns on duplicate names with same type, errors when types differ.

Common option (`GenerateDefaultCtor`) available on every attribute. Default is `true`. Set `GenerateDefaultCtor = false` to skip generating `public DtoName() { }`.

Eligible members are public/internal/protected-internal fields or properties that are non-static and not compiler-generated.

## Sample project
- `samples/DtoSrcGenSample` demonstrates all attributes. Run `dotnet build samples/DtoSrcGenSample` to see generated output and basic usage.

## Development notes
- Keep your consumer project language version at least 9; C# 11 is needed for `RequiredAttribute`.
- Diagnostics IDs: `DSG3000` (missing member), `DSG3001` (type mismatch in union), `DSG2000` (duplicate union member), `DSG2001` (internal members ignored), `DSG3002` (`Required` unsupported language version).

## Contributing
- Open issues or PRs with reproducible scenarios. Add samples that cover new behaviors.
