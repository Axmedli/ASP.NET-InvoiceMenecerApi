using InvoiceMenecer.Models;
using InvoiceMenecerApi.DTOs.CustomerDto;

namespace InvoiceMenecerApi.Services.Interfaces;

public interface ICustomerService
{
    Task<CustomerResponseDto> CreateCustomerAsync(CreateCustomerDto createCustomerDto);
    Task<CustomerResponseDto> UpdateCustomerAsync(Guid customerId, UpdateCustomerDto updateCustomerDto);
    Task<bool> HardDeleteCustomerAsync(Guid customerId);
    Task<bool> ArchiveCustomerAsync(Guid customerId);
    Task<CustomerResponseDto> GetCustomerByIdAsync(Guid customerId);
    Task<IEnumerable<CustomerResponseDto>> GetAllCustomersAsync();
}
