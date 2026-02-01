using InvoiceMenecer.Models;

namespace InvoiceMenecerApi.DTOs.InvoiceDto;

/// <summary>
/// Represents the response data transfer object for an invoice, including invoice details and associated rows.
/// </summary>
public class InvoiceResponseDto
{
    /// <summary>
    /// Gets or sets the unique identifier of the invoice.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the customer.
    /// </summary>
    public Guid CustomerId { get; set; }

    /// <summary>
    /// Gets or sets the name of the customer.
    /// </summary>
    public string CustomerName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the start date of the invoice period.
    /// </summary>
    public DateTimeOffset StartDate { get; set; }

    /// <summary>
    /// Gets or sets the end date of the invoice period.
    /// </summary>
    public DateTimeOffset EndDate { get; set; }

    /// <summary>
    /// Gets or sets the total sum of the invoice.
    /// </summary>
    public decimal TotalSum { get; set; }

    /// <summary>
    /// Gets or sets an optional comment for the invoice.
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// Gets or sets the status of the invoice.
    /// </summary>
    public InvoiceStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the creation date and time of the invoice.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the last updated date and time of the invoice.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the list of invoice row details.
    /// </summary>
    public List<InvoiceRowDto> Rows { get; set; } = new();
}
