namespace Example.EntityFramework.Tree.Entities;

public class Item
{
    public Guid Id { get; set; }

    public string LanguageCode { get; set; } = "en";

    public string Name { get; set; } = string.Empty;

    public int Order { get; set; } = 1;

    public int Level { get; set; } = 1;

    public Guid? ParentId { get; set; }

    public virtual Item? Parent { get; set; }

    public virtual ICollection<Item> Children { get; set; } = new List<Item>();
}
