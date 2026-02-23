namespace InvoiceMenecerApi.Models;

public class Invoice
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset EndDate { get; set; }
    public decimal TotalSum { get; set; }
    public string Comment { get; set; } = string.Empty;
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Created;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    public Customer Customer { get; set; } = null!;
    public ICollection<InvoiceRow> Rows { get; set; } = new List<InvoiceRow>();

}
