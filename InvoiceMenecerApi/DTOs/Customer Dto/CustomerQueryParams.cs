namespace InvoiceMenecerApi.DTOs.CustomerDto;

/// <summary>
/// Represents query parameters for filtering and paginating customer data.
/// </summary>
public class CustomerQueryParams
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
    /// The field to sort by (Name, Email, CreatedAt, UpdatedAt). Default is "Name".
    /// </summary>
    public string? Sort { get; set; } = "Name";
    
    /// <summary>
    /// The direction of sorting ("asc" or "desc"). Default is "asc".
    /// </summary>
    public string? SortDirection { get; set; } = "asc";
    
    /// <summary>
    /// Filter by customer name (partial match).
    /// </summary>
    public string? Name { get; set; }
    
    /// <summary>
    /// Filter by customer email (partial match).
    /// </summary>
    public string? Email { get; set; }
    
    /// <summary>
    /// Filter by customer phone number (partial match).
    /// </summary>
    public string? PhoneNumber { get; set; }
    
    /// <summary>
    /// The search keyword for filtering customers (searches in Name, Email, PhoneNumber).
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
        
        if (string.IsNullOrWhiteSpace(SortDirection))   SortDirection = "asc";
        
        SortDirection = SortDirection.ToLower();
        
        if (SortDirection != "asc" && SortDirection != "desc")  SortDirection = "asc";
        
        if (string.IsNullOrWhiteSpace(Sort))    Sort = "Name";
    }
}