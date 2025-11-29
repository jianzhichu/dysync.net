using System.Security.Cryptography;
using System.Text;

namespace dy.net.utils
{
    public class XBogus
    {
        private readonly int?[] _array;
        private readonly string _character;
        private readonly byte[] _uaKey = { 0x00, 0x01, 0x0c };
        private readonly string _userAgent;

        public string Params { get; private set; }
        public string Xb { get; private set; }

        public XBogus(string userAgent = "")
        {
            // 初始化 Array 数组（对应 Python 的 self.Array）
            _array = new int?[128];
            // 数字 0-9 对应 ASCII 48-57
            for (int i = 48; i <= 57; i++)
                _array[i] = i - 48;
            // 字母 A-F 对应 ASCII 65-70，映射为 10-15
            for (int i = 65; i <= 70; i++)
                _array[i] = i - 55;
            // 字母 a-f 对应 ASCII 97-102，映射为 10-15
            for (int i = 97; i <= 102; i++)
                _array[i] = i - 87;

            // 字符映射表
            _character = "Dkdpgh4ZKsQB80/Mfvw36XI1R25-WUAlEi7NLboqYTOPuzmFjJnryx9HVGcaStCe=";

            // 用户代理，默认值与 Python 一致
            _userAgent = string.IsNullOrEmpty(userAgent)
                ? "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36 Edg/122.0.0.0"
                : userAgent;
        }

        /// <summary>
        /// 将字符串通过 MD5 哈希转换为整数数组
        /// </summary>
        private int[] Md5StrToArray(string md5Str)
        {
            if (!string.IsNullOrEmpty(md5Str) && md5Str.Length > 32)
                return md5Str.Select(c => (int)c).ToArray();

            var result = new List<int>();
            for (int i = 0; i < md5Str.Length; i += 2)
            {
                if (i + 1 >= md5Str.Length)
                    break;

                int? high = _array[md5Str[i]];
                int? low = _array[md5Str[i + 1]];
                if (high == null || low == null)
                    result.Add(0);
                else
                    result.Add(((int)high << 4) | (int)low);
            }
            return result.ToArray();
        }

        /// <summary>
        /// 多轮 MD5 哈希加密 URL 参数
        /// </summary>
        private int[] Md5Encrypt(string urlParams)
        {
            string firstMd5 = Md5(urlParams);
            int[] firstArray = Md5StrToArray(firstMd5);
            string secondMd5 = Md5(firstArray);
            return Md5StrToArray(secondMd5);
        }

        /// <summary>
        /// 计算 MD5 哈希值
        /// </summary>
        private string Md5(object input)
        {
            int[] dataArray;
            switch (input)
            {
                case string str:
                    dataArray = Md5StrToArray(str);
                    break;
                case int[] arr:
                    dataArray = arr;
                    break;
                default:
                    throw new ArgumentException("Invalid input type. Expected string or int array.");
            }

            using (var md5 = MD5.Create())
            {
                byte[] bytes = dataArray.Select(i => (byte)(i & 0xFF)).ToArray();
                byte[] hashBytes = md5.ComputeHash(bytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            }
        }

        /// <summary>
        /// 第一次编码转换
        /// </summary>
        private string EncodingConversion(
            int a, int b, int c, int e, int d, int t, int f, int r, int n, int o,
            int i, int _, int x, int u, int s, int l, int v, int h, int p)
        {
            var bytes = new byte[]
            {
                (byte)a, (byte)i, (byte)b, (byte)_ , (byte)c, (byte)x,
                (byte)e, (byte)u, (byte)d, (byte)s, (byte)t, (byte)l,
                (byte)f, (byte)v, (byte)r, (byte)h, (byte)n, (byte)p, (byte)o
            };
            return Encoding.GetEncoding("ISO-8859-1").GetString(bytes);
        }

        /// <summary>
        /// 第二次编码转换
        /// </summary>
        private string EncodingConversion2(int a, int b, string c)
        {
            return ((char)a).ToString() + ((char)b).ToString() + c;
        }

        /// <summary>
        /// RC4 加密算法
        /// </summary>
        private byte[] Rc4Encrypt(byte[] key, byte[] data)
        {
            int[] S = Enumerable.Range(0, 256).ToArray();
            int j = 0;

            // 初始化 S 盒
            for (int i = 0; i < 256; i++)
            {
                j = (j + S[i] + key[i % key.Length]) % 256;
                (S[i], S[j]) = (S[j], S[i]);
            }

            // 生成密文
            var encrypted = new byte[data.Length];
            int i2 = 0, j2 = 0;
            for (int k = 0; k < data.Length; k++)
            {
                i2 = (i2 + 1) % 256;
                j2 = (j2 + S[i2]) % 256;
                (S[i2], S[j2]) = (S[j2], S[i2]);
                int t = (S[i2] + S[j2]) % 256;
                encrypted[k] = (byte)(data[k] ^ S[t]);
            }

            return encrypted;
        }

        /// <summary>
        /// 位运算计算
        /// </summary>
        private string Calculation(int a1, int a2, int a3)
        {
            int x1 = (a1 & 0xFF) << 16;
            int x2 = (a2 & 0xFF) << 8;
            int x3 = x1 | x2 | (a3 & 0xFF);

            char c1 = _character[(x3 & 0x0FC0000) >> 18]; // 16515072 = 0x0FC0000
            char c2 = _character[(x3 & 0x003F000) >> 12]; // 258048 = 0x003F000
            char c3 = _character[(x3 & 0x0000FC0) >> 6];  // 4032 = 0x0000FC0
            char c4 = _character[x3 & 0x3F];

            return $"{c1}{c2}{c3}{c4}";
        }

        /// <summary>
        /// 获取 X-Bogus 值
        /// </summary>
        public (string Params, string Xb, string UserAgent) GetXBogus(string urlParams)
        {
            // 计算 array1
            byte[] uaBytes = Encoding.GetEncoding("ISO-8859-1").GetBytes(_userAgent);
            byte[] rc4Ua = Rc4Encrypt(_uaKey, uaBytes);
            string base64Ua = Convert.ToBase64String(rc4Ua);
            string md5Ua = Md5(base64Ua);
            int[] array1 = Md5StrToArray(md5Ua);

            // 计算 array2（固定 MD5：d41d8cd98f00b204e9800998ecf8427e 是空字符串的 MD5）
            int[] array2 = Md5StrToArray(Md5(Md5StrToArray("d41d8cd98f00b204e9800998ecf8427e")));

            // 计算 URL 参数的 MD5 数组
            int[] urlParamsArray = Md5Encrypt(urlParams);

            // 时间戳和固定值
            long timer = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            int ct = 536919696;

            // 构建 new_array
            var newArray = new List<double>
            {
                64, 0.00390625, 1, 12,
                urlParamsArray.Length > 14 ? urlParamsArray[14] : 0,
                urlParamsArray.Length > 15 ? urlParamsArray[15] : 0,
                array2.Length > 14 ? array2[14] : 0,
                array2.Length > 15 ? array2[15] : 0,
                array1.Length > 14 ? array1[14] : 0,
                array1.Length > 15 ? array1[15] : 0,
                (timer >> 24) & 0xFF,
                (timer >> 16) & 0xFF,
                (timer >> 8) & 0xFF,
                timer & 0xFF,
                (ct >> 24) & 0xFF,
                (ct >> 16) & 0xFF,
                (ct >> 8) & 0xFF,
                ct & 0xFF
            };

            // 计算异或结果
            int xorResult = (int)newArray[0];
            for (int i = 1; i < newArray.Count; i++)
            {
                int b = (int)newArray[i];
                xorResult ^= b;
            }
            newArray.Add(xorResult);

            // 拆分 array3 和 array4
            var array3 = new List<int>();
            var array4 = new List<int>();
            for (int i = 0; i < newArray.Count; i++)
            {
                array3.Add((int)newArray[i]);
                if (i + 1 < newArray.Count)
                    array4.Add((int)newArray[i + 1]);
                i++;
            }

            // 合并数组
            int[] mergeArray = array3.Concat(array4).ToArray();

            // 生成乱码
            string encoding1 = EncodingConversion(
                mergeArray[0], mergeArray[1], mergeArray[2], mergeArray[3], mergeArray[4],
                mergeArray[5], mergeArray[6], mergeArray[7], mergeArray[8], mergeArray[9],
                mergeArray[10], mergeArray[11], mergeArray[12], mergeArray[13], mergeArray[14],
                mergeArray[15], mergeArray[16], mergeArray[17], mergeArray[18]
            );

            byte[] encoding1Bytes = Encoding.GetEncoding("ISO-8859-1").GetBytes(encoding1);
            byte[] rc4Key = Encoding.GetEncoding("ISO-8859-1").GetBytes("ÿ");
            byte[] rc4Encrypted = Rc4Encrypt(rc4Key, encoding1Bytes);
            string rc4Str = Encoding.GetEncoding("ISO-8859-1").GetString(rc4Encrypted);

            string garbledCode = EncodingConversion2(2, 255, rc4Str);

            // 计算 X-Bogus
            StringBuilder xbBuilder = new StringBuilder();
            for (int i = 0; i < garbledCode.Length; i += 3)
            {
                if (i + 2 >= garbledCode.Length)
                    break;

                int a = garbledCode[i];
                int b = garbledCode[i + 1];
                int c = garbledCode[i + 2];
                xbBuilder.Append(Calculation(a, b, c));
            }

            // 结果赋值
            Xb = xbBuilder.ToString();
            Params = $"{urlParams}&X-Bogus={Xb}";

            return (Params, Xb, _userAgent);
        }
    }

    // 测试代码
    //public class XBogusTest
    //{
    //    public static void Main()
    //    {
    //        string ua = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/123.0.0.0 Safari/537.36";
    //        var xb = new XBogus(ua);

    //        string dyUrlParams = "device_platform=webapp&aid=6383&channel=channel_pc_web&sec_user_id=MS4wLjABAAAAW9FWcqS7RdQAWPd2AA5fL_ilmqsIFUCQ_Iym6Yh9_cUa6ZRqVLjVQSUjlHrfXY1Y&max_cursor=0&locate_query=false&show_live_replay_strategy=1&need_time_list=1&time_list_query=0&whale_cut_token=&cut_version=1&count=18&publish_video_strategy_type=2&pc_client_type=1&version_code=170400&version_name=17.4.0&cookie_enabled=true&screen_width=1920&screen_height=1080&browser_language=zh-CN&browser_platform=Win32&browser_name=Edge&browser_version=122.0.0.0&browser_online=true&engine_name=Blink&engine_version=122.0.0.0&os_name=Windows&os_version=10&cpu_core_num=12&device_memory=8&platform=PC&downlink=10&effective_type=4g&round_trip_time=50&webid=7335414539335222835&msToken=p9Y7fUBuq9DKvAuN27Peml6JbaMqG2ZcXfFiyDv1jcHrCN00uidYqUgSuLsKl1onC-E_n82m-aKKYE0QGEmxIWZx9iueQ6WLbvzPfqnMk4GBAlQIHcDzxb38FLXXQxAm";
    //        string tkUrlParams = "WebIdLastTime=1713796127&abTestVersion=%5Bobject%20Object%5D&aid=1988&appType=t&app_language=zh-Hans&app_name=tiktok_web&browser_name=Mozilla&browser_online=true&browser_platform=Win32&browser_version=5.0%20%28Windows%20NT%2010.0%3B%20Win64%3B%20x64%29%20AppleWebKit%2F537.36%20%28KHTML%2C%20like%20Gecko%29%20Chrome%2F123.0.0.0%20Safari%2F537.36&channel=tiktok_web&device_id=7360698239018452498&odinId=7360698115047851026&region=TW&tz_name=Asia%2FHong_Kong&uniqueId=rei_toy625";

    //        var dyResult = xb.GetXBogus(dyUrlParams);
    //        Console.WriteLine($"Douyin - URL: {dyResult.Params}, X-Bogus: {dyResult.Xb}, UA: {dyResult.UserAgent}");

    //        var tkResult = xb.GetXBogus(tkUrlParams);
    //        Console.WriteLine($"TikTok - URL: {tkResult.Params}, X-Bogus: {tkResult.Xb}, UA: {tkResult.UserAgent}");
    //    }
    //}
}
