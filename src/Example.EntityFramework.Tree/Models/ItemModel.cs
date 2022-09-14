using System;
namespace Example.EntityFramework.Tree.Models;

public class ItemModel
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Url { get; set; }

    public int Order { get; set; } = 1;

    public List<ItemModel> SubItems { get; set; } = new();
}

