using FluentValidation;
using InvoiceMenecerApi.DTOs.InvoiceDto;

namespace InvoiceMenecerApi.Validators;

public class UpdateInvoiceDtoValidator : AbstractValidator<UpdateInvoiceDto>
{
    public UpdateInvoiceDtoValidator()
    {
        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required")
            .LessThan(x => x.EndDate).WithMessage("Start date must be before end date");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("End date is required")
            .GreaterThan(x => x.StartDate).WithMessage("End date must be after start date");

        RuleFor(x => x.Comment)
            .MaximumLength(1000).WithMessage("Comment cannot exceed 1000 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Comment));

        RuleFor(x => x.Rows)
            .NotEmpty().WithMessage("Invoice must have at least one row")
            .Must(rows => rows.Count > 0).WithMessage("Invoice must have at least one row");

        RuleForEach(x => x.Rows)
            .SetValidator(new CreateInvoiceRowDtoValidator());
    }
}
