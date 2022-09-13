using System.Text.Json.Serialization;

namespace Example.EntityFramework.Tree.Models;

public class UpdateItemModel : AddItemModel
{
    [JsonIgnore]
    public Guid Id { get; set; }
}