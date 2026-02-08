using FluentValidation;
using InvoiceMenecerApi.DTOs.InvoiceDto;

namespace InvoiceMenecerApi.Validators;

public class InvoiceQueryParamsValidator : AbstractValidator<InvoiceQueryParams>
{
    public InvoiceQueryParamsValidator()
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
            .Must(x => x == null || new[] { "customerid", "startdate", "enddate", "totalsum", "status", "createdat", "updatedat" }
                .Contains(x.ToLower()))
            .WithMessage("Invalid sort field. Valid values: CustomerId, StartDate, EndDate, TotalSum, Status, CreatedAt, UpdatedAt")
            .When(x => !string.IsNullOrWhiteSpace(x.Sort));

        RuleFor(x => x.MinTotalSum)
            .GreaterThanOrEqualTo(0).WithMessage("Minimum total sum cannot be negative")
            .When(x => x.MinTotalSum.HasValue);

        RuleFor(x => x.MaxTotalSum)
            .GreaterThanOrEqualTo(0).WithMessage("Maximum total sum cannot be negative")
            .GreaterThanOrEqualTo(x => x.MinTotalSum)
                .WithMessage("Maximum total sum must be greater than or equal to minimum total sum")
            .When(x => x.MaxTotalSum.HasValue && x.MinTotalSum.HasValue);

        RuleFor(x => x.StartDateFrom)
            .LessThanOrEqualTo(x => x.StartDateTo)
            .WithMessage("Start date from must be before or equal to start date to")
            .When(x => x.StartDateFrom.HasValue && x.StartDateTo.HasValue);

        RuleFor(x => x.EndDateFrom)
            .LessThanOrEqualTo(x => x.EndDateTo)
            .WithMessage("End date from must be before or equal to end date to")
            .When(x => x.EndDateFrom.HasValue && x.EndDateTo.HasValue);
    }
}
