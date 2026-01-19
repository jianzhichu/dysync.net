using ClockSnowFlake;
using System.Security.Cryptography;
using System.Text;

namespace dy.net.utils
{
    public static class Md5Util
    {

        public static string JWT_TOKEN_KEY = "dysync.net-key-" + IdGener.GetGuid();
        public static string Md5(this string inputString)
        {

            // 将输入字符串转换为字节数组
            byte[] inputBytes = Encoding.UTF8.GetBytes(inputString);

            // 创建一个 MD5 对象
            using MD5 md5 = MD5.Create();
            // 计算哈希值
            byte[] hashBytes = md5.ComputeHash(inputBytes);

            // 将哈希值转换为十六进制字符串
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("x2"));
            }
            string hashString = sb.ToString();
            return hashString;
        }
    }
}
