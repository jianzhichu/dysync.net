using ClockSnowFlake;
using dy.net.model.dto;
using dy.net.service;
using dy.net.utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace dy.net.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly AdminUserService _userService;

        public AuthController(
            AdminUserService userService,
            IWebHostEnvironment webHostEnvironment)
        {
            _userService = userService;
            this.webHostEnvironment = webHostEnvironment;
        }

        /// <summary>
        /// 修改密码
        /// </summary>
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> UpdatePwd(UpdatePwdRequest user)
        {
            var (code, erro) = await _userService.UpdatePwd(user);
            return ApiResult.Success("", erro);
        }

        /// <summary>
        /// 获取头像
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetUserAvatar()
        {
            var user = await _userService.GetUser();
            return ApiResult.Success(new
            {
                user?.Avatar,
                user?.Id,
                user?.UserName
            });
        }

        /// <summary>
        /// 登录获取 Token
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Login(LoginRequest loginUserInfo)
        {
            if (loginUserInfo == null)
            {
                return ApiResult.Fail("参数不能为空");
            }

            var user = await _userService.GetUser(loginUserInfo.UserName);

            if (user == null)
            {
                return ApiResult.Fail("用户名或密码不正确");
            }

            if (user.Password != Md5Util.Md5(loginUserInfo.Password))
            {
                return ApiResult.Fail("用户名或密码不正确");
            }

            var tokenString = GenerateJwtToken(user.UserName);

            return Ok(new
            {
                code = 0,
                erro = "",
                token = tokenString,

                // 单位：毫秒。与前端 setAuthorization 的 number 参数保持一致。
                // 7 天 = 604800000 毫秒。
                expires = 7L * 24 * 60 * 60 * 1000,

                data = user.UserName
            });
        }

        private static string GenerateJwtToken(string username)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(
                    JwtRegisteredClaimNames.Jti,
                    Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(
                Encoding.ASCII.GetBytes(Md5Util.JWT_TOKEN_KEY));

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            // JWT 有效期与前端 Cookie 都统一为 7 天。
            var expires = DateTime.UtcNow.AddDays(7);

            var token = new JwtSecurityToken(
                issuer: IdGener.GetLong().ToString(),
                audience: IdGener.GetLong().ToString(),
                claims: claims,
                expires: expires,
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
