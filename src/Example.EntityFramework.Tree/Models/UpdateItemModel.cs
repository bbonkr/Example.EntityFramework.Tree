using System.Text.Json.Serialization;
using FluentValidation;

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

public class UpdateItemModelValidator : AbstractValidator<UpdateItemModel>
{
    public UpdateItemModelValidator()
    {
        RuleFor(x => x.LanguageCode).NotEmpty()
            .WithErrorCode("languagecode_required")
            .WithMessage(x => $"{nameof(UpdateItemModel.LanguageCode)} is required");
        RuleFor(x => x.Name).NotEmpty()
            .WithErrorCode("name_required")
            .WithMessage(x => $"{nameof(UpdateItemModel.Name)} is required");
        RuleFor(x => x.Order).GreaterThan(0)
            .WithErrorCode("order_invalid")
            .WithMessage(x => $"{nameof(UpdateItemModel.Order)} is greater than 0");
    }
}