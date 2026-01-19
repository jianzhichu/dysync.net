using ClockSnowFlake;
using dy.net.model.dto;
using dy.net.model.entity;
using dy.net.utils;
using SqlSugar;

namespace dy.net.repository
{
    public class AdminUserRepository : BaseRepository<AdminUserInfo>
    {
        public AdminUserRepository(ISqlSugarClient db) : base(db)
        {
        }


        /// <summary>
        /// 修改密码
        /// </summary>
        /// <param name="loginUser"></param>
        /// <returns></returns>
        public async Task<(int code, string erro)> UpdatePwd(UpdatePwdRequest loginUser)
        {
            if (string.IsNullOrWhiteSpace(loginUser.UserId))
            {
                return (-1, "用户不存在");
            }
            else
            {
                var user = await this.GetByIdAsync(loginUser.UserId);
                if (user != null)
                {
                    if (loginUser?.OldPassword?.Md5() != user.Password)
                    {
                        return (-1, "原密码错误");
                    }
                    else
                    {
                        var newpassword = loginUser.Password.Md5();
                        user.Password = newpassword;
                        user.UserName = loginUser.UserName;
                        var res = await this.UpdateAsync(user); 
                        return (res?0:-1,res  ?"更新成功" : "更新失败");
                    }
                }
                else
                {
                    return (-1, "用户不存在");
                }
            }
        }


        public async Task<AdminUserInfo> GetUser(string userName=null)
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                return await this.GetFirstAsync(x=>!string.IsNullOrWhiteSpace(x.Id));
            }
            else
            {
                return await this.GetFirstAsync(x => x.UserName == userName);
            }
        }


        /// <summary>
        /// 修改头像
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="avatar"></param>
        /// <returns></returns>
        public async Task<bool> UpdateAvatar(string avatar)
        {
            var user = await this.GetUser();
            if (user is null)
            {
                return false;
            }
            else
            {
                user.Avatar = avatar;
                var update = await UpdateAsync(user);
                return update ;
            }
        }

        /// <summary>
        /// 初始化系统管理员
        /// </summary>
        /// <param name="userInfo"></param>
        /// <returns></returns>
        public  (int code, string erro) InitUser(AdminUserInfo userInfo)
        {
            var isInit =  this.GetFirst(x=>!string.IsNullOrWhiteSpace(x.Id));
            if (isInit!=null)
            {
                return (-1, "系统用户已存在");
            }
            else
            {
                userInfo.Id = IdGener.GetLong().ToString();
                userInfo.CreateTime = DateTime.Now;
                userInfo.Password = Md5Util.Md5(userInfo.Password);

                var res =  this.Insert(userInfo);
                return (res  ? 0 : -1, res ? "初始用户成功" : "初始化用户失败");
            }
        }
    }
}
