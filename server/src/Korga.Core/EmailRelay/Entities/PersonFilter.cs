namespace Korga.EmailRelay.Entities;

public abstract class PersonFilter
{
    public long Id { get; set; }

    public long? ParentId { get; set; }
    public PersonFilter? Parent { get; set; }
}
