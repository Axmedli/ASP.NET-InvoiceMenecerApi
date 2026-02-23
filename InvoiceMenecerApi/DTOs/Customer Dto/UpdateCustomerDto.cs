namespace InvoiceMenecerApi.DTOs.CustomerDto;

/// <summary>
/// DTO for updating customer information.
/// </summary>
public class UpdateCustomerDto
{
    /// <summary>
    /// Customer's name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Customer's address.
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Customer's email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Customer's phone number.
    /// </summary>
    public string? PhoneNumber { get; set; }
}
