using System.Text.Json.Serialization;
using FluentValidation;

namespace Example.EntityFramework.Tree.Models;

public class AddItemModel
{
    public string LanguageCode { get; set; } = "en";

    public string Name { get; set; } = string.Empty;

    public string? Url { get; set; }

    public int Order { get; set; } = 0;

    public Guid? ParentId { get; set; } = null;
}

public class AddItemModelValidator : AbstractValidator<AddItemModel>
{
    public AddItemModelValidator()
    {
        RuleFor(x => x.LanguageCode).NotEmpty()
            .WithErrorCode("languagecode_required")
            .WithMessage(x => $"{nameof(AddItemModel.LanguageCode)} is required");
        RuleFor(x => x.Name).NotEmpty()
            .WithErrorCode("name_required")
            .WithMessage(x => $"${nameof(AddItemModel.Name)} is required");
        RuleFor(x => x.Order).GreaterThan(0)
            .WithErrorCode("order_invalid")
            .WithMessage(x => $"{nameof(AddItemModel.Order)} is greater than 0");
    }
}