using FluentValidation;
using InvoiceMenecerApi.DTOs.InvoiceDto;

namespace InvoiceMenecerApi.Validators;

public class ChangeInvoiceStatusDtoValidator : AbstractValidator<ChangeInvoiceStatusDto>
{
    public ChangeInvoiceStatusDtoValidator()
    {
        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid invoice status");
    }
}
