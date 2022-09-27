using System;
using System.Linq;
using System.Text.Json;
using FluentValidation;
using FluentValidation.AspNetCore;
using FluentValidation.Results;
using kr.bbon.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace Example.EntityFramework.Tree.Infrastructure;

public class CustomValidatorInterceptor : IValidatorInterceptor
{
    public ValidationResult AfterAspNetValidation(ActionContext actionContext, IValidationContext validationContext, ValidationResult result)
    {
        if (result.Errors.Any())
        {
            var failures = result.Errors.Select(x => new ValidationFailure(x.PropertyName, Serialize(x)));

            return new ValidationResult(failures);
        }

        return result;
    }

    public IValidationContext BeforeAspNetValidation(ActionContext actionContext, IValidationContext commonContext)
    {
        return commonContext;
    }

    private static string Serialize(ValidationFailure failure)
    {
        var model = new ErrorModel(failure.ErrorMessage, failure.ErrorCode, failure.PropertyName);
        return JsonSerializer.Serialize(model);
    }
}

