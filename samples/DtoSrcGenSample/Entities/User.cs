namespace DtoSrcGenSample.Entities;

public class User
{
    public Guid Id { get; set; }
    
    public string Name { get; set; }
    
    public int Age;
    
    public string Email { get; set; }
    
    internal Flags Flags { get; set; }
}

public class Flags
{
    public bool Deleted { get; set; }
    
    public bool IsBot { get; set; }
}