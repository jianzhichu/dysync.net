namespace dy.net.model.dto
{


    public class UpdatePwdRequest
    {
        public string UserId { get; set; }
        public string OldPassword { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }

        public string UserName { get; set; }

    }

    public class LoginRequest
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }


    public class DataBaseConfigInfoRequest
    {
        public SqlSugar.DbType DbType { get; set; }

        public string ConnString { get; set; }
    }
}
