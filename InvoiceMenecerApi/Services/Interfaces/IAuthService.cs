using InvoiceMenecerApi.DTOs;

namespace InvoiceMenecerApi.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto registerRequest);
    Task<AuthResponseDto> LoginAsync(LoginRequestDto loginRequest);
    Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequest refreshTokenRequest);
    Task RevokeRefreshTokenAsync(RefreshTokenRequest refreshTokenRequest);
    Task<AuthResponseDto> EditProfile(string userId, EditProfileRequest editProfileRequest);
    Task<AuthResponseDto> UpdatePasswordAsync(string userId, UpdatePasswordRequest updatePasswordRequest);
    Task<bool> DeleteAccountAsync(string userId, DeletePasswordRequest deletePasswordRequest);

}