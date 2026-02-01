using InvoiceMenecer.Models;

namespace InvoiceMenecerApi.DTOs.InvoiceDto;

/// <summary>
/// DTO for changing the status of an invoice.
/// </summary>
public class ChangeInvoiceStatusDto
{
    /// <summary>
    /// The new status to set for the invoice.
    /// </summary>
    public InvoiceStatus Status { get; set; }
}
