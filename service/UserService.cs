using dy.net.dto;
using dy.net.model;
using dy.net.repository;

namespace dy.net.service
{
    public class UserService
    {

        private readonly UserRepository _userRepository;

        public UserService(UserRepository userRepository)
        {
            _userRepository = userRepository;
        }


        public async Task<(int code, string erro)> UpdatePwd(UpdatePwdRequest loginUser)
        {
            return await _userRepository.UpdatePwd(loginUser);
        }

        public async Task<LoginUserInfo> GetUser(string userName=null)
        {
            return await _userRepository.GetUser(userName);
        }

        public async Task<bool> UpdateAvatar(string avatar)
        {
            return await _userRepository.UpdateAvatar(avatar);
        }

        public (int code, string erro) InitUser(LoginUserInfo userInfo)
        {
            return  _userRepository.InitUser(userInfo);
        }
    }
}
