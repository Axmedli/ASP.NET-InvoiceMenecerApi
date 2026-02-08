using FluentValidation;
using InvoiceMenecerApi.DTOs.InvoiceDto;

namespace InvoiceMenecerApi.Validators;

public class CreateInvoiceRowDtoValidator : AbstractValidator<CreateInvoiceRowDto>
{
    public CreateInvoiceRowDtoValidator()
    {
        RuleFor(x => x.Service)
            .NotEmpty().WithMessage("Service name is required")
            .MaximumLength(300).WithMessage("Service name cannot exceed 300 characters");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than 0");
    }
}
