using System;
namespace Example.EntityFramework.Tree.Options;

public class TreeOptions
{
    public const string Name = "Tree";

    public int MaxLevel { get; set; } = 3;

    public int TopLevelItemsCount { get; set; } = 5;
}

