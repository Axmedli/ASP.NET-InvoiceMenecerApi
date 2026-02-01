namespace InvoiceMenecerApi.DTOs.CustomerDto;

/// <summary>
/// Represents the response data transfer object for a customer.
/// </summary>
public class CustomerResponseDto
{
    /// <summary>
    /// Gets or sets the unique identifier of the customer.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the customer.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the address of the customer.
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Gets or sets the email address of the customer.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the phone number of the customer.
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the customer was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the customer was last updated.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; set; }
}
    