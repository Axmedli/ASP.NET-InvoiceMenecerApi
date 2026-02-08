using FluentValidation;
using InvoiceMenecerApi.DTOs.CustomerDto;

namespace InvoiceMenecerApi.Validators;

public class CustomerQueryParamsValidator : AbstractValidator<CustomerQueryParams>
{
    public CustomerQueryParamsValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Page must be greater than or equal to 1");

        RuleFor(x => x.Size)
            .GreaterThanOrEqualTo(1).WithMessage("Size must be greater than or equal to 1")
            .LessThanOrEqualTo(100).WithMessage("Size cannot exceed 100");

        RuleFor(x => x.SortDirection)
            .Must(x => x == null || x.ToLower() == "asc" || x.ToLower() == "desc")
            .WithMessage("Sort direction must be 'asc' or 'desc'")
            .When(x => !string.IsNullOrWhiteSpace(x.SortDirection));

        RuleFor(x => x.Sort)
            .Must(x => x == null || new[] { "name", "email", "createdat", "updatedat" }
                .Contains(x.ToLower()))
            .WithMessage("Invalid sort field. Valid values: Name, Email, CreatedAt, UpdatedAt")
            .When(x => !string.IsNullOrWhiteSpace(x.Sort));
    }
}