namespace InvoiceMenecerApi.DTOs.CustomerDto;

/// <summary>
/// Data Transfer Object for creating a new customer.
/// </summary>
public class CreateCustomerDto
{
    /// <summary>
    /// Gets or sets the customer's name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the customer's address.
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Gets or sets the customer's email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the customer's phone number.
    /// </summary>
    public string? PhoneNumber { get; set; }
}
