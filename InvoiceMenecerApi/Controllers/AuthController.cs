using InvoiceMenecerApi.Common;
using InvoiceMenecerApi.DTOs;
using InvoiceMenecerApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InvoiceMenecerApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Register(
        [FromBody] RegisterRequestDto registerRequest)
    {
        var result = await _authService.RegisterAsync(registerRequest);
        return Ok(ApiResponse<AuthResponseDto>.SuccessResponse(result, "User registered successfully"));
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Login(
        [FromBody] LoginRequestDto loginRequest)
    {
        var result = await _authService.LoginAsync(loginRequest);
        return Ok(ApiResponse<AuthResponseDto>.SuccessResponse(result, "Login successfully"));

    }

    [HttpPut("edit-profile")]
    public async Task<ActionResult<AuthResponseDto>> EditProfile([FromBody] EditProfileRequest editProfileRequest)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _authService.EditProfile(UserId, editProfileRequest);
        return Ok(result);
    }

    [HttpPut("update-password")]
    public async Task<ActionResult<AuthResponseDto>> UpdatePassword([FromBody] UpdatePasswordRequest updatePasswordRequest)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _authService.UpdatePasswordAsync(UserId, updatePasswordRequest);
        return Ok(result);
    }

    [HttpDelete("delete-account")]
    public async Task<IActionResult> DeleteAccount([FromBody] DeletePasswordRequest deletePasswordRequest)
    {
        var result = await _authService.DeleteAccountAsync(UserId, deletePasswordRequest);

        if (!result)
            return BadRequest("Failed to delete account");

        return NoContent();
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Refresh(
        [FromBody] RefreshTokenRequest refreshTokenRequest)
    {
        var result = await _authService.RefreshTokenAsync(refreshTokenRequest);
        return Ok(ApiResponse<AuthResponseDto>.SuccessResponse(result, "Token refresh successfully"));
    }

    [HttpPost("revoke")]
    public async Task<ActionResult> Revoke(
        [FromBody] RefreshTokenRequest refreshTokenRequest)
    {
        await _authService.RevokeRefreshTokenAsync(refreshTokenRequest);
        return Ok(ApiResponse<object>.SuccessResponse("Token revoke successfully"));
    }


}
