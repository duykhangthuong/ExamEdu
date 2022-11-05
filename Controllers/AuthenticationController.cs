using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using BackEnd.DTO.AuthDTO;
using BackEnd.Helper.Authentication;
using BackEnd.Helper.RefreshToken;
using examedu.Services.Account;
using ExamEdu.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BackEnd.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthenticationController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IAccountService _accountService;
        private readonly IJwtGenerator _jwtGenerator;
        private readonly IRefreshToken _refreshToken;
        public AuthenticationController(IAccountService accountService, IMapper mapper, IJwtGenerator jwtGenerator, IRefreshToken refreshToken)
        {
            _accountService = accountService;
            _mapper = mapper;
            _jwtGenerator = jwtGenerator;
            _refreshToken = refreshToken;
        }

        /// <summary>
        /// Login function for all roles
        /// </summary>
        /// <param name="loginInput">Email and password of user</param>
        /// <returns>User data with access token in body and refresh token in cookies</returns>
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginInput loginInput)
        {
            try
            {
                //Find account of user in db
                (var account, string password) = await _accountService.GetAccountByEmail(loginInput.Email);
                if (account == null)
                {
                    return NotFound(new ResponseDTO(404, "Wrong credentials or account has been deactivated"));
                }

                //Verify password with BCrypt
                bool verified = false;
                verified = BCrypt.Net.BCrypt.Verify(loginInput.Password, password);
                if (verified == false)
                {
                    return NotFound(new ResponseDTO(404, "Wrong credentials"));
                }

                //Create claims including email and role for adding to payload in access token
                string roleName = await _accountService.GetRoleName(account.RoleID);
                var claims = new List<Claim>{
                    new Claim(ClaimTypes.Email,account.Email)
                };
                claims.Add(new Claim("role", roleName));

                //Generate access and refresh token
                string accessToken = _jwtGenerator.GenerateAccessToken(claims);
                string refreshToken = _refreshToken.GenerateRefreshToken(account.Email);
                if (accessToken == null || refreshToken == null)
                {
                    return NotFound(new ResponseDTO(404, "Token generate fail"));
                }

                //Add refresh token to cookies with httponly
                Response.Cookies.Append("X-Refresh-Token", refreshToken, new CookieOptions() { HttpOnly = true, SameSite = SameSiteMode.None, Secure = true });

                //Add access token to account response
                var authResponse = _mapper.Map<AuthResponse>(account);
                authResponse.Role = roleName;
                authResponse.AccessToken = accessToken;
                return Ok(authResponse);
            }
            catch
            {
                return Conflict(new ResponseDTO(409, "Something wrong. Please try again."));
            }
        }

        /// <summary>
        /// Revoke refresh token of user
        /// </summary>
        /// <returns>200: Logout success/400: No cookies found, Fail to remove refresh token</returns>
        [HttpPost("logout")]
        public ActionResult Logout()
        {
            if (!Request.Cookies.TryGetValue("X-Refresh-Token", out var refreshToken))
            {
                return BadRequest(new ResponseDTO(400, "No cookies found!"));
            }
            var tokenEmail = _refreshToken.GetEmailByRefreshToken(refreshToken);
            try
            {
                _refreshToken.RemoveRefreshTokenByEmail(tokenEmail);
            }
            catch
            {
                return BadRequest(new ResponseDTO(400, "Fail to remove refresh token."));
            }
            return Ok(new ResponseDTO(200, "Logout success!"));
        }

        [HttpPut("forgot-password")]
        public async Task<ActionResult> ForgotPassword([FromBody]string email)
        {
            //Find account of user in db
            (var account, string password) = await _accountService.GetAccountByEmail(email);
            if (account == null)
            {
                return NotFound(new ResponseDTO(404, "Email does not exist or has been deactivated"));
            }

            int rs= await _accountService.UpdatePasswordAndSendEmail(email);
            if(rs==1){
                return Ok(new ResponseDTO(200, "An email contains new password has been sent to your email address. Please check your email to get the new password."));
            }else{
                return Conflict(new ResponseDTO(409, "Reset password fail. Please try again."));
            }
        }
    }
}