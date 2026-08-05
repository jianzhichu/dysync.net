using dy.net.model.dto;
using dy.net.service;
using dy.net.utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace dy.net.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly AdminUserService _userService;
        private readonly JwtTokenService _jwtTokenService;

        public AuthController(
            AdminUserService userService,
            IWebHostEnvironment webHostEnvironment,
            JwtTokenService jwtTokenService)
        {
            _userService = userService;
            this.webHostEnvironment = webHostEnvironment;
            _jwtTokenService = jwtTokenService;
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

            if (user == null || user.Password != Md5Util.Md5(loginUserInfo.Password))
            {
                return ApiResult.Fail("用户名或密码不正确");
            }

            var tokenString = _jwtTokenService.GenerateToken(user.UserName);

            // 与所有其他接口保持统一的 { code, message, data } 响应结构。
            return ApiResult.Success(new
            {
                token = tokenString,
                expires = _jwtTokenService.ExpireMilliseconds,
                userName = user.UserName
            });
        }
    }
}
