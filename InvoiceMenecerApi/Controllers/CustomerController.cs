using InvoiceMenecerApi.Common;
using InvoiceMenecerApi.DTOs.CustomerDto;
using InvoiceMenecerApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceMenecerApi.Controllers;

/// <summary>
/// Controller for managing customer-related operations such as create, update, delete, archive, and retrieval.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize(Policy = "UserOrAbove")]
public class CustomerController : ControllerBase
{
    private readonly ICustomerService _customerService;

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomerController"/> class.
    /// </summary>
    /// <param name="customerService">Service for customer operations.</param>
    public CustomerController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    /// <summary>
    /// Creates a new customer.
    /// </summary>
    /// <param name="customerCreateDto">Customer creation data.</param>
    /// <returns>The created customer.</returns>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<CustomerResponseDto>>> CreateCustomer(
    [FromBody] CreateCustomerDto customerCreateDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var createdCustomer = await _customerService.CreateCustomerAsync(customerCreateDto);

        return CreatedAtAction(
            nameof(GetCustomerById),
            new { id = createdCustomer.Id },
            ApiResponse<CustomerResponseDto>
                .SuccessResponse(createdCustomer, "Customer created successfully"));
    }

    /// <summary>
    /// Updates an existing customer.
    /// </summary>
    /// <param name="id">Customer ID.</param>
    /// <param name="customerUpdateDto">Customer update data.</param>
    /// <returns>The updated customer.</returns>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<CustomerResponseDto>>> UpdateCustomer(
        Guid id,
        [FromBody] UpdateCustomerDto customerUpdateDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var updatedCustomer = await _customerService.UpdateCustomerAsync(id, customerUpdateDto);

        if (updatedCustomer == null)
            return NotFound($"Customer with id {id} not found");

        return Ok(ApiResponse<CustomerResponseDto>
            .SuccessResponse(updatedCustomer, "Customer updated successfully"));
    }

    /// <summary>
    /// Permanently deletes a customer.
    /// </summary>
    /// <param name="id">Customer ID.</param>
    /// <returns>Status of the deletion.</returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<string>>> HardDeleteCustomer(Guid id)
    {
        var isDeleted = await _customerService.HardDeleteCustomerAsync(id);

        if (!isDeleted)
            return BadRequest("Cannot delete customer with sent invoices or customer not found");

        return Ok(ApiResponse<string>
            .SuccessResponse($"Customer with id {id} has been hard deleted successfully"));
    }

    /// <summary>
    /// Archives a customer.
    /// </summary>
    /// <param name="id">Customer ID.</param>
    /// <returns>Status of the archiving operation.</returns>
    [HttpPost("{id}/archive")]
    public async Task<ActionResult<ApiResponse<string>>> ArchiveCustomer(Guid id)
    {
        var isArchived = await _customerService.ArchiveCustomerAsync(id);

        if (!isArchived)
            return NotFound($"Customer with id {id} not found");

        return Ok(ApiResponse<string>
            .SuccessResponse($"Customer with id {id} has been archived successfully"));
    }

    /// <summary>
    /// Retrieves a customer by ID.
    /// </summary>
    /// <param name="id">Customer ID.</param>
    /// <returns>The customer data.</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<CustomerResponseDto>>> GetCustomerById(Guid id)
    {
        var response = await _customerService.GetCustomerByIdAsync(id);

        if (response == null)
            return NotFound($"Customer with id {id} not found");

        return Ok(ApiResponse<CustomerResponseDto>
            .SuccessResponse(response));
    }

    /// <summary>
    /// Retrieves all customers.
    /// </summary>
    /// <returns>List of all customers.</returns>
    [HttpGet("all")]
    public async Task<ActionResult<ApiResponse<IEnumerable<CustomerResponseDto>>>> GetAllCustomers()
    {
        var customers = await _customerService.GetAllCustomersAsync();
        return Ok(ApiResponse<IEnumerable<CustomerResponseDto>>
            .SuccessResponse(customers));
    }

    /// <summary>
    /// Retrieves a paginated list of customers based on the provided query parameters.
    /// Supports paging, sorting, filtering by name/email/phone, and global searching.
    /// </summary>
    /// <param name="queryParams">The parameters for paging, sorting, filtering and searching customers (Page, Size, Sort, SortDirection, Name, Email, PhoneNumber, Search).</param>
    /// <returns>An ApiResponse containing a PagedResult of CustomerResponseDto with items and paging metadata.</returns>
    /// <response code="200">Returns the paged list of customers.</response>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<CustomerResponseDto>>>> GetAllCustomersPaged(
        [FromQuery] CustomerQueryParams queryParams)
    {
        var result = await _customerService.GetAllCustomersPagedAsync(queryParams);
        return Ok(ApiResponse<PagedResult<CustomerResponseDto>>
            .SuccessResponse(result));
    }
}
