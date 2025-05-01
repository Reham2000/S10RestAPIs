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


    }
}
