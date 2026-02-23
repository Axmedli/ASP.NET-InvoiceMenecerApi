using System.ComponentModel.DataAnnotations;

namespace InvoiceMenecerApi.DTOs.InvoiceDto;

public class CreateInvoiceRowDto
{
    public string Service { get; set; } = string.Empty;
    public decimal Quantity { get; set; }

    public decimal Amount { get; set; }

}