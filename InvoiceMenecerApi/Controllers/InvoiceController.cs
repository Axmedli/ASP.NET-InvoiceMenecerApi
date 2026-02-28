using InvoiceMenecerApi.Common;
using InvoiceMenecerApi.DTOs.InvoiceDto;
using InvoiceMenecerApi.Models;
using InvoiceMenecerApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceMenecerApi.Controllers;

/// <summary>
/// Controller for managing invoices. Provides endpoints for creating, updating, deleting, archiving, 
/// changing status, and retrieving invoices.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize(Policy = "UserOrAbove")]
public class InvoiceController : ControllerBase
{
    private readonly IInvoiceService _invoiceService;

    /// <summary>
    /// Initializes a new instance of the <see cref="InvoiceController"/> class.
    /// </summary>
    /// <param name="invoiceService">Service for invoice operations.</param>
    public InvoiceController(IInvoiceService invoiceService)
    {
        _invoiceService = invoiceService;
    }

    /// <summary>
    /// Creates a new invoice.
    /// </summary>
    /// <param name="createInvoiceDto">Invoice data to create.</param>
    /// <returns>The created invoice.</returns>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<InvoiceResponseDto>>> CreateInvoice(
        [FromBody] CreateInvoiceDto createInvoiceDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var createdInvoice = await _invoiceService.CreateInvoiceAsync(createInvoiceDto);

        return CreatedAtAction(
            nameof(GetInvoiceById),
            new { id = createdInvoice.Id },
            ApiResponse<InvoiceResponseDto>
                .SuccessResponse(createdInvoice, "Invoice created successfully"));
    }

    /// <summary>
    /// Updates an existing invoice.
    /// </summary>
    /// <param name="id">Invoice ID.</param>
    /// <param name="updateInvoiceDto">Updated invoice data.</param>
    /// <returns>The updated invoice.</returns>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<InvoiceResponseDto>>> UpdateInvoice(
        Guid id,
        [FromBody] UpdateInvoiceDto updateInvoiceDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var updatedInvoice = await _invoiceService.UpdateInvoiceAsync(id, updateInvoiceDto);

        if (updatedInvoice == null)
            return NotFound($"Invoice with id {id} not found");

        return Ok(ApiResponse<InvoiceResponseDto>
            .SuccessResponse(updatedInvoice, $"Invoice with id {id} updated successfully"));
    }

    /// <summary>
    /// Changes the status of an invoice.
    /// </summary>
    /// <param name="id">Invoice ID.</param>
    /// <param name="newStatus">New status for the invoice.</param>
    /// <returns>The invoice with updated status.</returns>
    [HttpPost("{id}/status")]
    public async Task<ActionResult<ApiResponse<InvoiceResponseDto>>> ChangeInvoiceStatus(
        Guid id,
        [FromBody] InvoiceStatus newStatus)
    {
        var updatedInvoice = await _invoiceService.ChangeInvoiceStatusAsync(id, newStatus);

        if (updatedInvoice == null)
            return NotFound($"Invoice with id {id} not found");

        return Ok(ApiResponse<InvoiceResponseDto>
            .SuccessResponse(updatedInvoice, $"Invoice status with id {id} changed successfully"));
    }

    /// <summary>
    /// Permanently deletes an invoice.
    /// </summary>
    /// <param name="id">Invoice ID.</param>
    /// <returns>True if deleted, otherwise not found.</returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> HardDeleteInvoice(Guid id)
    {
        var isDeleted = await _invoiceService.HardDeleteInvoiceAsync(id);

        if (!isDeleted)
            return NotFound($"Invoice with id {id} not found");

        return Ok(ApiResponse<bool>
            .SuccessResponse(true, $"Invoice with id {id} deleted successfully"));
    }

    /// <summary>
    /// Archives an invoice.
    /// </summary>
    /// <param name="id">Invoice ID.</param>
    /// <returns>True if archived, otherwise not found.</returns>
    [HttpPost("{id}/archive")]
    public async Task<ActionResult<ApiResponse<bool>>> ArchiveInvoice(Guid id)
    {
        var isArchived = await _invoiceService.ArchiveInvoiceAsync(id);

        if (!isArchived)
            return NotFound($"Invoice with id {id} not found");

        return Ok(ApiResponse<bool>
            .SuccessResponse(true, $"Invoice with id {id} archived successfully"));
    }

    /// <summary>
    /// Retrieves all invoices without pagination.
    /// Returns complete list of non-archived invoices.
    /// </summary>
    /// <returns>List of all invoices.</returns>
    /// <response code="200">Returns all invoices.</response>
    [HttpGet("all")]
    public async Task<ActionResult<ApiResponse<IEnumerable<InvoiceResponseDto>>>> GetAllInvoices()
    {
        var invoices = await _invoiceService.GetAllInvoicesAsync();

        return Ok(ApiResponse<IEnumerable<InvoiceResponseDto>>
            .SuccessResponse(invoices));
    }

    /// <summary>
    /// Retrieves an invoice by its ID.
    /// </summary>
    /// <param name="id">Invoice ID.</param>
    /// <returns>The invoice if found, otherwise not found.</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<InvoiceResponseDto>>> GetInvoiceById(Guid id)
    {
        var response = await _invoiceService.GetInvoiceByIdAsync(id);

        if (response == null)
            return NotFound($"Invoice with id {id} not found");

        return Ok(ApiResponse<InvoiceResponseDto>
            .SuccessResponse(response));
    }

    /// <summary>
    /// Retrieves all invoices for a specific customer.
    /// </summary>
    /// <param name="customerId">Customer ID.</param>
    /// <returns>List of invoices for the customer.</returns>
    [HttpGet("customer/{customerId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<InvoiceResponseDto>>>> GetInvoicesByCustomerId(Guid customerId)
    {
        var invoices = await _invoiceService.GetInvoicesByCustomerIdAsync(customerId);

        return Ok(ApiResponse<IEnumerable<InvoiceResponseDto>>
            .SuccessResponse(invoices));
    }

    /// <summary>
    /// Retrieves a paginated list of invoices based on the provided query parameters.
    /// Supports paging, sorting, filtering by customer/status/dates/totalsum, and global searching.
    /// </summary>
    /// <param name="queryParams">The parameters for paging, sorting, filtering and searching invoices.</param>
    /// <returns>An ApiResponse containing a PagedResult of InvoiceResponseDto with items and paging metadata.</returns>
    /// <response code="200">Returns the paged list of invoices.</response>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<InvoiceResponseDto>>>> GetAllInvoicesPaged([FromQuery] InvoiceQueryParams queryParams)
    {
        var pagedInvoices = await _invoiceService.GetAllInvoicesPagedAsync(queryParams);
        return Ok(ApiResponse<PagedResult<InvoiceResponseDto>>
            .SuccessResponse(pagedInvoices));
    }

    /// <summary>
    /// Downloads the invoice as a PDF file.
    /// </summary>
    /// <param name="id">Invoice ID.</param>
    /// <returns>PDF file.</returns>
    /// <response code="200">Returns the PDF file.</response>
    /// <response code="404">If the invoice is not found.</response>
    [HttpGet("{id}/download/pdf")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadInvoiceAsPdf(Guid id)
    {
        try
        {
            var fileBytes = await _invoiceService.DownloadInvoiceAsPdfAsync(id);
            var fileName = $"Invoice_{id.ToString().Substring(0, 8)}_{DateTime.Now:yyyyMMdd}.pdf";

            return File(fileBytes, "application/pdf", fileName);
        }
        catch (Exception ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
