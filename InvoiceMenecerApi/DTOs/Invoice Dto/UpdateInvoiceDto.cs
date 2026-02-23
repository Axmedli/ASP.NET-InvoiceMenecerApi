namespace InvoiceMenecerApi.DTOs.InvoiceDto;

/// <summary>
/// DTO for updating an invoice, including date range, comment, and invoice rows.
/// </summary>
public class UpdateInvoiceDto
{
    /// <summary>
    /// The start date of the invoice period.
    /// </summary>
    public DateTimeOffset StartDate { get; set; }

    /// <summary>
    /// The end date of the invoice period.
    /// </summary>
    public DateTimeOffset EndDate { get; set; }

    /// <summary>
    /// Optional comment for the invoice.
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// List of invoice row details to update.
    /// </summary>
    public List<CreateInvoiceRowDto> Rows { get; set; } = new();
}
