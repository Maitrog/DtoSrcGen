using DtoSrcGen;
using DtoSrcGenSample.Entities;

namespace DtoSrcGenSample.Dto;

[Pick(typeof(User), $"{nameof(User.Id)} as UserId", nameof(User.Name), nameof(User.Email))]
public partial class UserPickDto
{
}

[Pick(typeof(User), nameof(User.Id), nameof(User.Name), GenerateDefaultCtor = false)]
public partial class UserPickNoDefaultCtorDto
{
}

[Omit(typeof(User), nameof(User.Flags))]
public partial class UserWithoutFlagsDto
{
}

[Readonly(typeof(Chat))]
public partial class ReadonlyChatDto
{
}

[Required(typeof(User))]
public partial class RequiredUserDto
{
}

[Union(typeof(Chat), typeof(User.FlagCollection))]
public partial class ChatWithFlagsDto
{
}

[Pick(typeof(User), nameof(User.Flags))]
[Omit(typeof(Chat), nameof(Chat.Created), nameof(Chat.Updated), GenerateDefaultCtor = false)]
public partial class UserChatDto
{
}

[Pick(typeof(User.FlagCollection), nameof(User.FlagCollection.IsBot))]
public partial class FlagsPickDto
{
}

[Omit(typeof(User))]
public partial class UserDto
{
    [Omit(typeof(User))]
    public partial class NestedUserDto
    {
    }
}