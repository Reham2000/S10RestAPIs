using Core.Interfaces;
using Domain.DTos;
using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

namespace Api.Controllers
{
    [Route("api/V1.0/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ITokenService _tokenService;
        private readonly IUnitOfWork _unitOfWork;
        public AuthController(IAuthService authService, ITokenService tokenService,IUnitOfWork unitOfWork)
        {
            _authService = authService;
            _tokenService = tokenService;
            _unitOfWork = unitOfWork;
        }

        // POST: api/V1.0/Auth/Register
        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegisterDTo model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var user = await _authService.RegisterAsync(model);
                    if (user is null)
                        return BadRequest(new {
                            StatusCode = StatusCodes.Status400BadRequest,
                            message = "User already exists" 
                        });
                    return Ok(new { 
                            StatusCode = StatusCodes.Status200OK,
                        message = "User created successfully",
                        Data = new
                        {
                            UserId = user.Id,
                            UserName = user.UserName,
                            Email = user.Email
                        }
                    });
                }
                return BadRequest(new {
                    StatusCode = StatusCodes.Status400BadRequest,
                    message = "Invalid model",
                    Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new {
                    StatusCode = StatusCodes.Status400BadRequest,
                    message = ex.Message });
            }
        }

        // login
        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginDTo model)
        {
            try
            {
                if (ModelState.IsValid)
                {

                    var user = await _authService.LoginAsync(model);
                    if(user is null)
                        return Unauthorized(new
                        {
                            StatusCode = StatusCodes.Status401Unauthorized,
                            message = "Invalid UserName Or Password!"
                        });
                    // generate token
                    var token = _tokenService.GenerateJwtToken(user);
                    await _authService.StoreJwtToken(user, token);
                    return Ok(new
                    {
                        StatusCode = StatusCodes.Status200OK,
                        message = "User Logged in Successfully!",
                        Data = new
                        {
                            UserId = user.Id,
                            user.UserName,
                            user.Email,
                            Token = token
                        }
                    });



                }
                return BadRequest(new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    message = "Invalid model",
                    Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                });

            }
            catch (Exception ex)
            {
                return BadRequest(
                    new
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        message = ex.Message,
                    });
            }
        }
        [Authorize(Policy = "AllPolicy")]
        [HttpPost("Logout")]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var jti = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
                if (string.IsNullOrWhiteSpace(jti))
                {
                    return Unauthorized(new
                    {
                        StatusCode = StatusCodes.Status401Unauthorized,
                        message = "Invalid Token : Missing JTI"
                    });
                }
                await _unitOfWork.revokedTokens.RevokTokenAsync(jti);
                await _unitOfWork.CompleteAsync();

                return Ok(new
                {
                    StatusCode = StatusCodes.Status200OK,
                    message = "Logged out successfully!"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    message = "an error occurred while logging out...",
                    Details = ex.Message,
                });
            }
        }
        [Authorize(Policy = "AllPolicy")]
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken(TokenRequest request)
        {
            var response = await _tokenService.RefreshToken(request.Token,GetIpAddress());
            if(response == null)
                return Unauthorized(new
                {
                    StatusCaode = StatusCodes.Status401Unauthorized,
                    message = "Invaild token"
                });
            return Ok(new
            {
                StatusCaode = StatusCodes.Status401Unauthorized,
                message = "Refresh token generated successfully!",
                response = response

            });

        }

        [Authorize(Policy = "AllPolicy")]
        [HttpPost("revoce-token")]
        public async Task<IActionResult> RevoceTohen(TokenRequest request)
        {
            var result = await _tokenService.RevokeToken(request.Token,GetIpAddress());
            if(result)
                return Ok(new
                {
                    StatusCaode = StatusCodes.Status200OK,
                    message = "token has been revoced successsfully!",
                    data = result
                });
            return NotFound(new
            {
                StatusCaode = StatusCodes.Status404NotFound,
                message = "Invaild token : token Not found!"
            });
        }













        private string GetIpAddress()
        {
            return Request.Headers.ContainsKey("X-Forwarded-For")
                ? Request.Headers["X-Forwarded-For"]
                : HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
        }

    }
}
