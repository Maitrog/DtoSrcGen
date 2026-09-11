namespace DtoSrcGenSample.Entities;

public class User
{
    public Guid Id { get; set; }
    
    public string Name { get; set; }
    
    public int Age;
    
    public string Email { get; set; }
    
    internal FlagCollection Flags { get; set; }

    public class FlagCollection
    {
        public bool Deleted { get; set; }
        
        public bool IsBot { get; set; }
    }
}

