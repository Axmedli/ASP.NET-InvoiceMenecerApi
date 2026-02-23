namespace InvoiceMenecerApi.DTOs.InvoiceDto;

/// <summary>
/// DTO for creating a new invoice, including customer, date range, comment, and invoice rows.
/// </summary>
public class CreateInvoiceDto
{
    /// <summary>
    /// Gets or sets the unique identifier of the customer.
    /// </summary>
    public Guid CustomerId { get; set; }

    /// <summary>
    /// Gets or sets the start date of the invoice period.
    /// </summary>
    public DateTimeOffset StartDate { get; set; }

    /// <summary>
    /// Gets or sets the end date of the invoice period.
    /// </summary>
    public DateTimeOffset EndDate { get; set; }

    /// <summary>
    /// Gets or sets an optional comment for the invoice.
    /// </summary>
    public string? Comment { get; set; }

    public List<CreateInvoiceRowDto> Rows { get; set; } = new();
}
    