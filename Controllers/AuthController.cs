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
        public AuthController(AdminUserService userService, IWebHostEnvironment webHostEnvironment)
        {
            _userService = userService;
            this.webHostEnvironment = webHostEnvironment;
        }


        /// <summary>
        /// 修改密码
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
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
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetUserAvatar()
        {
            var user = await _userService.GetUser();
            return ApiResult.Success(new { user?.Avatar, user?.Id, user?.UserName });
        }




        /// <summary>
        /// 登录获取token
        /// </summary>
        /// <param name="loginUserInfo"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Login(LoginRequest loginUserInfo)
        {
            if (loginUserInfo == null)
            {
                return ApiResult.Fail("参数不能为空");
            }
            else
            {
                var user = await _userService.GetUser(loginUserInfo.UserName);

                if (user == null)
                {
                    return ApiResult.Fail("用户名或密码不正确");
                }
                else
                {
                    if (user.Password == Md5Util.Md5(loginUserInfo.Password))
                    {

                        var tokenString = GenerateJwtToken(user.UserName);
                        return Ok(new { code = 0, erro = "", token = tokenString, expires = 24 * 60 * 60 * 1000, data = user.UserName });
                    }
                    else
                    {
                        return ApiResult.Fail("用户名或密码不正确");
                    }
                }
            }
        }


        private static string GenerateJwtToken(string username)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
             };
            var k = Md5Util.JWT_TOKEN_KEY;
            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(k));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expires = DateTime.Now.AddDays(7);

            var token = new JwtSecurityToken(
                issuer: IdGener.GetLong().ToString(),
                audience: IdGener.GetLong().ToString(),
                claims: claims,
                expires: expires,
                signingCredentials: credentials
            );

            var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);
            return jwtToken;
        }
    }
}
