using System.Text.Json.Serialization;

namespace Example.EntityFramework.Tree.Models;

public class AddItemModel
{
    public string LanguageCode { get; set; } = "en";

    public string Name { get; set; } = string.Empty;

    public int Order { get; set; } = 1;

    public Guid? ParentId { get; set; } = null;
}
