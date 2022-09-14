using System.Text.Json.Serialization;

namespace Example.EntityFramework.Tree.Models;

public class UpdateItemModel
{
    [JsonIgnore]
    public Guid Id { get; set; }

    public string LanguageCode { get; set; } = "en";

    public string Name { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;

    public int Order { get; set; } = 0;

    public Guid? ParentId { get; set; } = null;
}