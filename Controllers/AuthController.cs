using ClockSnowFlake;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using dy.net.service;
using dy.net.utils;
using dy.net.dto;

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
            return Ok(new { code, erro });
        }


        /// <summary>
        /// 获取头像
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetUserAvatar()
        {
            var user = await _userService.GetUser();
            return Ok(new { code = 0, error = "", data = new { user?.Avatar, user?.Id, user?.UserName } });
        }

        /// <summary>
        /// 修改用户头像
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> UpdateUserAvatar(IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                long maxFileSize = 5 * 1024 * 1024; // 限制文件大小为5MB
                if (file.Length > maxFileSize)
                {
                    return Ok(new { code = -1, erro = "文件最大只能上传5M" });
                }
                var fileName = $"{IdGener.GetGuid()}_{file.FileName}";
                var filePath = webHostEnvironment.IsProduction() ?
                    Path.Combine(Md5Util.UPLOAD_PATH_PRO, fileName) : Path.Combine(Md5Util.UPLOAD_PATH_DEV, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);

                    try
                    {
                        // 问了节约空间，删除文件夹下的所有文件
                        string[] files = Directory.GetFiles(webHostEnvironment.IsProduction() ? Md5Util.UPLOAD_PATH_PRO : Md5Util.UPLOAD_PATH_DEV);
                        foreach (string mfile in files)
                        {
                            if (!mfile.Contains(fileName))
                                System.IO.File.Delete(mfile);
                        }
                    }
                    catch (Exception ex)
                    {
                        Serilog.Log.Error($"delete file error ,{ex.Message}");
                    }
                    var update = await _userService.UpdateAvatar(fileName);

                    return Ok(new { code = update ? 0 : -1, erro = update ? "" : "上传失败", data = update ? fileName : "" });
                }
            }
            else
            {
                return Ok(new { code = -1, erro = "空文件" });
            }
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
                return Ok(new { code = -1, erro = "参数不能为空" });
            }
            else
            {
                var user = await _userService.GetUser(loginUserInfo.UserName);

                if (user == null)
                {
                    return Ok(new { code = -1, erro = "用户名或密码不正确" });
                }
                else
                {
                    if (user.Password == Md5Util.Md5(loginUserInfo.Password))
                    {

                        var tokenString = GenerateJwtToken(user.UserName);

                        return Ok(new { code = 0, erro = "", token = tokenString, expires = 24 * 60 * 60 * 1000 });
                    }
                    else
                    {
                        return Ok(new { code = -1, erro = "用户名或密码不正确" });
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

            var expires = DateTime.Now.AddDays(1);

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
