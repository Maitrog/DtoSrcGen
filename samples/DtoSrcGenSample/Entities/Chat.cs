namespace DtoSrcGenSample.Entities;

public class Chat
{
    public Guid Id { get; set; }
    
    public string Name { get; set; }
    
    public string Description { get; set; }

    public DateTime Created { get; set; }

    public DateTime Updated { get; set; }
}