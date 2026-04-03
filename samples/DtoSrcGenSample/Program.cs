using DtoSrcGenSample.Dto;
using DtoSrcGenSample.Entities;

var user = new User
{
    Id = Guid.NewGuid(),
    Name = "Alex",
    Age = 28,
    Email = "alex@example.com",
    Flags = new Flags
    {
        Deleted = false,
        IsBot = true
    }
};

var chat = new Chat
{
    Id = Guid.NewGuid(),
    Name = "General",
    Description = "Main team chat",
    Created = DateTime.UtcNow.AddDays(-10),
    Updated = DateTime.UtcNow,
};

var flags = new Flags
{
    Deleted = false,
    IsBot = true,
};

// [Pick + GenerateDefaultCtor = true] -> only selected members from User
var picked = new UserPickDto
{
    Id = user.Id,
    Email = user.Email,
    Name = "Alex"
};

// [Pick + GenerateDefaultCtor = false] -> only mapping ctor is generated
// var pickedNoDefaultCtor = new UserPickNoDefaultCtorDto(); Error default constructor doesn't exist
var pickedNoDefaultCtor = new UserPickNoDefaultCtorDto(user);

// [Omit] -> all User members except Flags
var withoutFlags = new UserWithoutFlagsDto(user);

// [Readonly] -> generated properties are get-only
var readonlyChat = new ReadonlyChatDto(chat);

// [Required] -> generated properties are required and must be initialized
var requiredUser = new RequiredUserDto
{
    Id = user.Id,
    Name = user.Name,
    Age = user.Age,
    Email = user.Email,
};

// [Union] -> combines members from multiple source types
var union = new ChatWithFlagsDto(chat, flags);

// [Pick + Omit] -> combine two attributes
var userChat = new UserChatDto(chat)
{
    // Flags property from User
    Flags = new Flags
    {
        Deleted = user.Flags.Deleted,
        IsBot = user.Flags.IsBot,
    }
};

var userChat2 = new UserChatDto(user)
{
    // Properties from Chat
    Id = chat.Id,
    Name = chat.Name,
    Description = chat.Description
};

Console.WriteLine($"Pick DTO: {picked.Id} | {picked.Name} | {picked.Email}");
Console.WriteLine($"Pick (NoDefaultCtor): {pickedNoDefaultCtor.Id} | {pickedNoDefaultCtor.Name}");
Console.WriteLine($"Omit DTO: {withoutFlags.Id} | {withoutFlags.Name} | {withoutFlags.Email} | Age={withoutFlags.Age}");
Console.WriteLine($"Readonly DTO: {readonlyChat.Name} ({readonlyChat.Created:u})");
Console.WriteLine($"Required DTO: {requiredUser.Name} | {requiredUser.Email}");
Console.WriteLine($"Union DTO: {union.Name} | Bot={union.IsBot}");
Console.WriteLine($"UserChat DTO (two attributes): {userChat.Id} | {userChat.Name} | {userChat.Description} | {userChat.Flags.Deleted} | {userChat.Flags.IsBot})");
Console.WriteLine($"UserChat2 DTO (two attributes): {userChat2.Id} | {userChat2.Name} | {userChat2.Description} | {userChat2.Flags.Deleted} | {userChat2.Flags.IsBot})");