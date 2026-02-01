using Microsoft.AspNetCore.Mvc;
using InvoiceMenecerApi.Services.Interfaces;
using InvoiceMenecerApi.DTOs.InvoiceDto;
using ASP_NET_08.Common;

namespace InvoiceMenecerApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class InvoiceRowController : ControllerBase
{
    private readonly IInvoiceRowService _invoiceRowService;

    public InvoiceRowController(IInvoiceRowService invoiceRowService)
    {
        _invoiceRowService = invoiceRowService;
    }

    /// <summary>
    /// Get all rows for a specific invoice
    /// </summary>
    /// <param name="invoiceId">Invoice ID</param>
    [HttpGet("invoice/{invoiceId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<InvoiceRowDto>>>> GetRowsByInvoiceId(Guid invoiceId)
    {
        var rows = await _invoiceRowService.GetRowsByInvoiceIdAsync(invoiceId);
        return Ok(ApiResponse<IEnumerable<InvoiceRowDto>>
            .SuccessResponse(rows));
    }

    /// <summary>
    /// Get a specific row by ID
    /// </summary>
    /// <param name="id">Row ID</param>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<InvoiceRowDto>>> GetRowById(Guid id)
    {
        var row = await _invoiceRowService.GetRowByIdAsync(id);

        if (row == null)
            return NotFound($"Invoice row with id {id} not found");

        return Ok(ApiResponse<InvoiceRowDto>
            .SuccessResponse(row));
    }

    /// <summary>
    /// Add a new row to an invoice
    /// </summary>
    /// <param name="invoiceId">Invoice ID</param>
    /// <param name="createRowDto">Row data</param>
    [HttpPost("invoice/{invoiceId}")]
    public async Task<ActionResult<ApiResponse<InvoiceRowDto>>> AddRowToInvoice(
        Guid invoiceId,
        [FromBody] CreateInvoiceRowDto createRowDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var row = await _invoiceRowService.AddRowToInvoiceAsync(invoiceId, createRowDto);

        if (row == null)
            return BadRequest("Cannot add row to sent invoices or invoice not found");

        return CreatedAtAction(
            nameof(GetRowById),
            new { id = row.Id },
            ApiResponse<InvoiceRowDto>
                .SuccessResponse(row, "Row added successfully"));
    }

    /// <summary>
    /// Update an existing row
    /// </summary>
    /// <param name="id">Row ID</param>
    /// <param name="updateRowDto">Updated row data</param>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<InvoiceRowDto>>> UpdateRow(
        Guid id,
        [FromBody] CreateInvoiceRowDto updateRowDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var row = await _invoiceRowService.UpdateRowAsync(id, updateRowDto);

        if (row == null)
            return BadRequest("Cannot update rows of sent invoices or row not found");

        return Ok(ApiResponse<InvoiceRowDto>
            .SuccessResponse(row, "Row updated successfully"));
    }

    /// <summary>
    /// Delete a row from an invoice
    /// </summary>
    /// <param name="id">Row ID</param>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<string>>> DeleteRow(Guid id)
    {
        var isDeleted = await _invoiceRowService.DeleteRowAsync(id);

        if (!isDeleted)
            return BadRequest("Cannot delete rows from sent invoices or row not found");

        return Ok(ApiResponse<string>
            .SuccessResponse($"Row with id {id} has been deleted successfully"));
    }
}