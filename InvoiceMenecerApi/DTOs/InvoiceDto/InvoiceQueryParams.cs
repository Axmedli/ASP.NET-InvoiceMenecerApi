namespace InvoiceMenecerApi.DTOs.InvoiceDto;

/// <summary>
/// Represents query parameters for filtering and paginating invoice data.
/// </summary>
public class InvoiceQueryParams
{
    /// <summary>
    /// The page number for pagination. Default is 1.
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// The number of items per page. Default is 10.
    /// </summary>
    public int Size { get; set; } = 10;

    /// <summary>
    /// The field to sort by (CustomerId, StartDate, EndDate, TotalSum, Status, CreatedAt, UpdatedAt). Default is "CreatedAt".
    /// </summary>
    public string? Sort { get; set; } = "CreatedAt";

    /// <summary>
    /// The direction of sorting ("asc" or "desc"). Default is "desc".
    /// </summary>
    public string? SortDirection { get; set; } = "desc";

    /// <summary>
    /// Filter by customer ID.
    /// </summary>
    public Guid? CustomerId { get; set; }

    /// <summary>
    /// Filter by invoice status (Created, Sent, Received, Paid, Cancelled, Rejected).
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// Filter by start date (from).
    /// </summary>
    public DateTimeOffset? StartDateFrom { get; set; }

    /// <summary>
    /// Filter by start date (to).
    /// </summary>
    public DateTimeOffset? StartDateTo { get; set; }

    /// <summary>
    /// Filter by end date (from).
    /// </summary>
    public DateTimeOffset? EndDateFrom { get; set; }

    /// <summary>
    /// Filter by end date (to).
    /// </summary>
    public DateTimeOffset? EndDateTo { get; set; }

    /// <summary>
    /// Filter by minimum total sum.
    /// </summary>
    public decimal? MinTotalSum { get; set; }

    /// <summary>
    /// Filter by maximum total sum.
    /// </summary>
    public decimal? MaxTotalSum { get; set; }

    /// <summary>
    /// The search keyword for filtering invoices (searches in Comment, Customer Name).
    /// </summary>
    public string? Search { get; set; }

    /// <summary>
    /// Validates and normalizes the query parameters.
    /// </summary>
    public void Validate()
    {
        if (Page < 1) Page = 1;
        if (Size < 1) Size = 10;
        if (Size > 100) Size = 100;

        if (string.IsNullOrWhiteSpace(SortDirection))
            SortDirection = "desc";

        SortDirection = SortDirection.ToLower();

        if (SortDirection != "asc" && SortDirection != "desc")
            SortDirection = "desc";

        if (string.IsNullOrWhiteSpace(Sort))
            Sort = "CreatedAt";
    }
}