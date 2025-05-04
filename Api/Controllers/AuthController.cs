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
        //private readonly IAuthService _services.authService;
        //private readonly ITokenService _tokenService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IServiceUnitOfWork _services;
        public AuthController(/*IAuthService authService, ITokenService tokenService,*/
            IUnitOfWork unitOfWork,IServiceUnitOfWork serviceUnitOfWork)
        {
            //_services.authService = authService;
            //_tokenService = tokenService;
            _unitOfWork = unitOfWork;
            _services = serviceUnitOfWork;

        }

        // POST: api/V1.0/Auth/Register
        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegisterDTo model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var result = await _services.authService.RegisterAsync(model);
                    if (!result.IsSuccessed)
                        return BadRequest(new {
                            StatusCode = StatusCodes.Status400BadRequest,
                            message = result.Errors 
                        });
                    return Ok(new { 
                            StatusCode = StatusCodes.Status200OK,
                        message = "User created successfully",
                        Data = new
                        {
                            UserId = result.Model.Id,
                            UserName = result.Model.UserName,
                            Email = result.Model.Email
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
        [Authorize(Policy ="AdminPolicy")]
        // POST: api/V1.0/Auth/Register
        [HttpPost("Add")]
        public async Task<IActionResult> Add(UserDTo model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var result = await _services.authService.AddAsync(model);
                    if (!result.IsSuccessed)
                        return BadRequest(new
                        {
                            StatusCode = StatusCodes.Status400BadRequest,
                            message = "Registration Faild",
                            Errors = result.Errors
                        });
                    return Ok(new
                    {
                        StatusCode = StatusCodes.Status200OK,
                        message = "User created successfully",
                        Data = new
                        {
                            UserId = result.Model.Id,
                            UserName = result.Model.UserName,
                            Email = result.Model.Email
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
                return BadRequest(new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    message = ex.Message
                });
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

                    var result = await _services.authService.LoginAsync(model);
                    if(!result.IsSuccessed)
                        return Unauthorized(new
                        {
                            StatusCode = StatusCodes.Status401Unauthorized,
                            message = result.Errors
                        });
                    // generate token
                    var token = await _services.tokenService.GenerateJwtToken(result.Model);
                    await _services.authService.StoreJwtToken(result.Model, token);
                    return Ok(new
                    {
                        StatusCode = StatusCodes.Status200OK,
                        message = "User Logged in Successfully!",
                        Data = new
                        {
                            UserId = result.Model.Id,
                            result.Model.UserName,
                            result.Model.Email,
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
            var response = await _services.tokenService.RefreshToken(request.Token,GetIpAddress());
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
            var result = await _services.tokenService.RevokeToken(request.Token,GetIpAddress());
            if(result)
                return Ok(new
                {
                    StatusCaode = StatusCodes.Status200OK,
                    message = "token has been revoced successsfully!",
                    data = result ? "Ok!" : "Error!"
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
