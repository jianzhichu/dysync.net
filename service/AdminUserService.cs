using dy.net.model.dto;
using dy.net.model.entity;
using dy.net.repository;

namespace dy.net.service
{
    public class AdminUserService
    {

        private readonly AdminUserRepository _userRepository;

        public AdminUserService(AdminUserRepository userRepository)
        {
            _userRepository = userRepository;
        }


        public async Task<(int code, string erro)> UpdatePwd(UpdatePwdRequest loginUser)
        {
            return await _userRepository.UpdatePwd(loginUser);
        }

        public async Task<AdminUserInfo> GetUser(string userName=null)
        {
            return await _userRepository.GetUser(userName);
        }

        public async Task<bool> UpdateAvatar(string avatar)
        {
            return await _userRepository.UpdateAvatar(avatar);
        }

        public (int code, string erro) InitUser(string UserName,string Password)
        {
            AdminUserInfo userInfo = new AdminUserInfo
            {
                UserName = UserName,
                Password = Password,
                CreateTime = DateTime.Now
            };
            return  _userRepository.InitUser(userInfo);
        }
    }
}
