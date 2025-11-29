using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;
using System.Text;

namespace dy.net.utils
{
     public static class StringProcessor
    {
        /// <summary>
        /// 将字符串转换为字符数组 (ASCII)
        /// </summary>
        public static int[] ToOrdArray(string s)
        {
            return s.Select(c => (int)c).ToArray();
        }

        /// <summary>
        /// 将整数数组转回字符串
        /// </summary>
        public static string ToCharStr(int[] arr)
        {
            return new string(arr.Select(i => (char)i).ToArray());
        }

        /// <summary>
        /// JavaScript 无符号右移操作 (>>>)
        /// </summary>
        public static int JsShiftRight(int value, int n)
        {
            uint uValue = (uint)value;
            return (int)(uValue >> n);
        }

        /// <summary>
        /// 生成伪随机混淆字节字符串 (长度为 length * 4)
        /// </summary>
        private static Random _random = new Random();
        public static string GenerateRandomBytes(int length = 3)
        {
            StringBuilder result = new StringBuilder();

            for (int i = 0; i < length; i++)
            {
                int rd = _random.Next(10000);

                result.Append((char)(((rd & 255) & 170) | 1));
                result.Append((char)(((rd & 255) & 85) | 2));
                result.Append((char)((JsShiftRight(rd, 8) & 170) | 5));
                result.Append((char)((JsShiftRight(rd, 8) & 85) | 40));
            }

            return result.ToString();
        }
    }

    public class CryptoUtility
    {
        public string Salt { get; set; }
        public List<string> Base64Alphabet { get; set; }

        private readonly int[] _bigArray = {
            121, 243,  55, 234, 103,  36,  47, 228,  30, 231, 106,   6, 115,  95,  78, 101,
            250, 207, 198,  50, 139, 227, 220, 105,  97, 143,  34,  28, 194, 215,  18, 100,
            159, 160,  43,   8, 169, 217, 180, 120, 247,  45,  90,  11,  27, 197,  46,   3,
             84,  72,   5,  68,  62,  56, 221,  75, 144,  79,  73, 161, 178,  81,  64, 187,
            134, 117, 186, 118,  16, 241, 130,  71,  89, 147, 122, 129,  65,  40,  88, 150,
            110, 219, 199, 255, 181, 254,  48,   4, 195, 248, 208,  32, 116, 167,  69, 201,
             17, 124, 125, 104,  96,  83,  80, 127, 236, 108, 154, 126, 204,  15,  20, 135,
            112, 158,  13,   1, 188, 164, 210, 237, 222,  98, 212,  77, 253,  42, 170, 202,
             26,  22,  29, 182, 251,  10, 173, 152,  58, 138,  54, 141, 185,  33, 157,  31,
            252, 132, 233, 235, 102, 196, 191, 223, 240, 148,  39, 123,  92,  82, 128, 109,
             57,  24,  38, 113, 209, 245,   2, 119, 153, 229, 189, 214, 230, 174, 232,  63,
             52, 205,  86, 140,  66, 175, 111, 171, 246, 133, 238, 193,  99,  60,  74,  91,
            225,  51,  76,  37, 145, 211, 166, 151, 213, 206,   0, 200, 244, 176, 218,  44,
            184, 172,  49, 216,  93, 168,  53,  21, 183,  41,  67,  85, 224, 155, 226, 242,
             87, 177, 146,  70, 190,  12, 162,  19, 137, 114,  25, 165, 163, 192,  23,  59,
              9,  94, 179, 107,  35,   7, 142, 131, 239, 203, 149, 136,  61, 249,  14, 156
        };

        public CryptoUtility(string salt, List<string> base64Alphabet)
        {
            Salt = salt;
            Base64Alphabet = base64Alphabet;
        }

        /// <summary>
        /// 计算 SM3 哈希并返回 byte 数组（即整数列表）
        /// </summary>
        public static byte[] Sm3Hash(byte[] input)
        {
            var digest = new SM3Digest();
            digest.BlockUpdate(input, 0, input.Length);
            byte[] output = new byte[digest.GetDigestSize()];
            digest.DoFinal(output, 0);
            return output;
        }

        /// <summary>
        /// 对输入数据计算 SM3 哈希，并返回整数数组
        /// </summary>
        public int[] Sm3ToArray(object input)
        {
            byte[] bytes;

            if (input is string str)
            {
                bytes = Encoding.UTF8.GetBytes(str);
            }
            else if (input is int[] arr)
            {
                bytes = arr.Select(b => (byte)b).ToArray();
            }
            else
            {
                throw new ArgumentException("Input must be string or int[]");
            }

            byte[] hash = Sm3Hash(bytes);
            return hash.Select(b => (int)b).ToArray();
        }

        /// <summary>
        /// 添加盐值
        /// </summary>
        public string AddSalt(string param)
        {
            return param + Salt;
        }

        /// <summary>
        /// 处理参数（可选加盐）
        /// </summary>
        public object ProcessParam(object param, bool addSalt)
        {
            if (param is string s && addSalt)
            {
                return AddSalt(s);
            }
            return param;
        }

        /// <summary>
        /// 获取参数哈希数组（双重哈希）
        /// </summary>
        public int[] ParamsToArray(object param, bool addSalt = true)
        {
            var processed = ProcessParam(param, addSalt);
            var firstHash = Sm3ToArray(processed);
            return Sm3ToArray(firstHash);
        }

        /// <summary>
        /// RC4 加密
        /// </summary>
        public static byte[] Rc4Encrypt(byte[] key, string plaintext)
        {
            byte[] data = Encoding.UTF8.GetBytes(plaintext);
            var rc4 = new RC4Engine();
            rc4.Init(true, new KeyParameter(key));

            byte[] output = new byte[data.Length];
            rc4.ProcessBytes(data, 0, data.Length, output, 0);
            return output;
        }

        /// <summary>
        /// 自定义 Base64 编码
        /// </summary>
        public string Base64Encode(string input, int selectedAlphabet = 0)
        {
            string alphabet = Base64Alphabet[selectedAlphabet];
            var binary = new StringBuilder();

            foreach (char c in input)
            {
                binary.Append(Convert.ToString(c, 2).PadLeft(8, '0'));
            }

            while (binary.Length % 6 != 0)
            {
                binary.Append('0');
            }

            var chunks = new List<int>();
            for (int i = 0; i < binary.Length; i += 6)
            {
                string chunk = binary.ToString(i, Math.Min(6, binary.Length - i));
                chunks.Add(Convert.ToInt32(chunk, 2));
            }

            var output = new StringBuilder();
            foreach (int index in chunks)
            {
                output.Append(alphabet[index]);
            }

            // Padding
            int padding = (6 - (binary.Length % 6)) % 6;
            output.Append('=', padding / 2);

            return output.ToString();
        }

        /// <summary>
        /// ABogus 自定义编码逻辑（类似Base64但不同分组）
        /// </summary>
        public string AbogusEncode(string input, int selectedAlphabet)
        {
            var abogus = new List<char>();
            string alphabet = Base64Alphabet[selectedAlphabet];

            for (int i = 0; i < input.Length; i += 3)
            {
                int n = 0;
                if (i + 2 < input.Length)
                {
                    n = (input[i] << 16) | (input[i + 1] << 8) | input[i + 2];
                }
                else if (i + 1 < input.Length)
                {
                    n = (input[i] << 16) | (input[i + 1] << 8);
                }
                else
                {
                    n = input[i] << 16;
                }

                int[] masks = { 0xFC0000, 0x03F000, 0x0FC0, 0x3F };
                int[] shifts = { 18, 12, 6, 0 };

                for (int j = 0; j < 4; j++)
                {
                    if ((j == 2 && i + 1 >= input.Length) || (j == 3 && i + 2 >= input.Length))
                        break;
                    int val = (n & masks[j]) >> shifts[j];
                    abogus.Add(alphabet[val]);
                }
            }

            while (abogus.Count % 4 != 0)
            {
                abogus.Add('=');
            }

            return new string(abogus.ToArray());
        }

        /// <summary>
        /// 字节数组变换加密（RC4-like 流密码）
        /// </summary>
        public string TransformBytes(int[] bytesList)
        {
            string bytesStr = StringProcessor.ToCharStr(bytesList);
            var result = new List<char>();

            int indexB = _bigArray[1];
            int initialValue = 0;
            int valueE = 0;

            for (int i = 0; i < bytesStr.Length; i++)
            {
                char ch = bytesStr[i];
                int charValue = ch;

                if (i == 0)
                {
                    initialValue = _bigArray[indexB];
                    int sumInitial = indexB + initialValue;

                    _bigArray[1] = initialValue;
                    _bigArray[indexB] = indexB;
                }
                else
                {
                    int sumInitial = initialValue + valueE;
                    sumInitial %= _bigArray.Length;
                    valueE = _bigArray[(i + 2) % _bigArray.Length];
                    sumInitial = (indexB + valueE) % _bigArray.Length;
                    initialValue = _bigArray[sumInitial];
                }

                int sumInitialFinal = (indexB + (i == 0 ? initialValue : valueE)) % _bigArray.Length;
                int valueF = _bigArray[sumInitialFinal];
                int encryptedChar = charValue ^ valueF;
                result.Add((char)encryptedChar);

                // 更新状态
                valueE = _bigArray[(i + 2) % _bigArray.Length];
                sumInitialFinal = (indexB + valueE) % _bigArray.Length;
                int temp = _bigArray[sumInitialFinal];
                _bigArray[sumInitialFinal] = _bigArray[(i + 2) % _bigArray.Length];
                _bigArray[(i + 2) % _bigArray.Length] = temp;
                indexB = sumInitialFinal;
            }

            return new string(result.ToArray());
        }
    }

    public class BrowserFingerprintGenerator
    {
        private static Random _random = new Random();

        public static string GenerateFingerprint(string browserType = "Edge")
        {
            return browserType switch
            {
                "Chrome" => _GenerateFingerprint("Win32"),
                "Firefox" => _GenerateFingerprint("Win32"),
                "Safari" => _GenerateFingerprint("MacIntel"),
                "Edge" => _GenerateFingerprint("Win32"),
                _ => _GenerateFingerprint("Win32")
            };
        }

        private static string _GenerateFingerprint(string platform)
        {
            int innerWidth = _random.Next(1024, 1921);
            int innerHeight = _random.Next(768, 1081);
            int outerWidth = innerWidth + _random.Next(24, 33);
            int outerHeight = innerHeight + _random.Next(75, 91);
            int screenX = 0;
            int screenY = _random.Next(2) == 0 ? 0 : 30;
            int sizeWidth = _random.Next(1024, 1921);
            int sizeHeight = _random.Next(768, 1081);
            int availWidth = _random.Next(1280, 1921);
            int availHeight = _random.Next(800, 1081);

            return $"{innerWidth}|{innerHeight}|{outerWidth}|{outerHeight}|" +
                   $"{screenX}|{screenY}|0|0|{sizeWidth}|{sizeHeight}|" +
                   $"{availWidth}|{availHeight}|{innerWidth}|{innerHeight}|24|24|{platform}";
        }
    }

    public class ABogus2
    {
        private int aid = 6383;
        private int pageId = 0;
        private string salt = "cus";
        private bool boe = false;
        private double ddrt = 8.5;
        private double ic = 8.5;
        private List<string> paths = new() {
            "^/webcast/", "^/aweme/v1/", "^/aweme/v2/", "/v1/message/send", "^/live/", "^/captcha/", "^/ecom/"
        };

        private byte[] uaKey = { 0x00, 0x01, 0x0E };

        private string character = "Dkdpgh2ZmsQB80/MfvV36XI1R45-WUAlEixNLwoqYTOPuzKFjJnry79HbGcaStCe";
        private string character2 = "ckdp1h4ZKsUB80/Mfvw36XIgR25+WQAlEi7NLboqYTOPuzmFjJnryx9HVGDaStCe";

        private List<string> characterList;
        private CryptoUtility cryptoUtility;

        private string userAgent;
        private string browserFp;

        private int[] sortIndex = {
            18, 20, 52, 26, 30, 34, 58, 38, 40, 53, 42, 21, 27, 54, 55, 31, 35, 57, 39, 41, 43, 22, 28,
            32, 60, 36, 23, 29, 33, 37, 44, 45, 59, 46, 47, 48, 49, 50, 24, 25, 65, 66, 70, 71
        };

        private int[] sortIndex2 = {
            18, 20, 26, 30, 34, 38, 40, 42, 21, 27, 31, 35, 39, 41, 43, 22, 28, 32, 36, 23, 29, 33, 37,
            44, 45, 46, 47, 48, 49, 50, 24, 25, 52, 53, 54, 55, 57, 58, 59, 60, 65, 66, 70, 71
        };

        public List<int> Options { get; set; } = new() { 0, 1, 14 }; // POST 默认

        public ABogus2(string fp = "", string userAgent = "", List<int> options = null)
        {
            if (options != null) Options = options;

            this.userAgent = !string.IsNullOrEmpty(userAgent)
                ? userAgent
                : "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/130.0.0.0 Safari/537.36 Edg/130.0.0.0";

            this.browserFp = !string.IsNullOrEmpty(fp)
                ? fp
                : BrowserFingerprintGenerator.GenerateFingerprint("Edge");

            characterList = new List<string> { character, character2 };
            cryptoUtility = new CryptoUtility(salt, characterList);
        }

        public string EncodeData(string data, int alphabetIndex = 0)
        {
            return cryptoUtility.AbogusEncode(data, alphabetIndex);
        }

        public (string paramsWithAbogus, string abogus, string userAgent, string body) GenerateAbogus(string paramsStr, string body = "")
        {
            var abDir = new Dictionary<int, object>
            {
                { 8, 3 },
                { 15, new {
                    aid = this.aid,
                    pageId = this.pageId,
                    boe = this.boe,
                    ddrt = this.ddrt,
                    paths = this.paths,
                    track = new { mode = 0, delay = 300, paths = new List<object>() },
                    dump = true,
                    rpU = ""
                }},
                { 18, 44 },
                { 19, new[] { 1, 0, 1, 0, 1 } },
                { 66, 0 },
                { 69, 0 },
                { 70, 0 },
                { 71, 0 }
            };

            long startEncryption = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            int[] array1 = cryptoUtility.ParamsToArray(paramsStr); // 双重哈希
            int[] array2 = body != "" ? cryptoUtility.ParamsToArray(body) : new int[0];
            string encodedUa = cryptoUtility.Base64Encode(
                      StringProcessor.ToCharStr(
                          Array.ConvertAll(CryptoUtility.Rc4Encrypt(uaKey, userAgent), b => (int)b)
                      ),
                      1
                  );

            int[] array3 = cryptoUtility.Sm3ToArray(encodedUa); // 不加盐

            long endEncryption = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            // 插入时间戳高位
            abDir[20] = (byte)((startEncryption >> 24) & 0xFF);
            abDir[21] = (byte)((startEncryption >> 16) & 0xFF);
            abDir[22] = (byte)((startEncryption >> 8) & 0xFF);
            abDir[23] = (byte)(startEncryption & 0xFF);
            abDir[24] = (int)((startEncryption >> 32) & 0xFF);
            abDir[25] = (int)((startEncryption >> 40) & 0xFF);

            // 请求选项
            abDir[26] = (byte)((Options[0] >> 24) & 0xFF);
            abDir[27] = (byte)((Options[0] >> 16) & 0xFF);
            abDir[28] = (byte)((Options[0] >> 8) & 0xFF);
            abDir[29] = (byte)(Options[0] & 0xFF);

            abDir[30] = (byte)((Options[1] >> 8) & 0xFF);
            abDir[31] = (byte)(Options[1] & 0xFF);
            abDir[32] = (byte)((Options[1] >> 24) & 0xFF);
            abDir[33] = (byte)((Options[1] >> 16) & 0xFF);

            abDir[34] = (byte)((Options[2] >> 24) & 0xFF);
            abDir[35] = (byte)((Options[2] >> 16) & 0xFF);
            abDir[36] = (byte)((Options[2] >> 8) & 0xFF);
            abDir[37] = (byte)(Options[2] & 0xFF);

            abDir[38] = array1[21];
            abDir[39] = array1[22];
            abDir[40] = array2.Length > 21 ? array2[21] : 0;
            abDir[41] = array2.Length > 22 ? array2[22] : 0;
            abDir[42] = array3.Length > 23 ? array3[23] : 0;
            abDir[43] = array3.Length > 24 ? array3[24] : 0;

            abDir[44] = (byte)((endEncryption >> 24) & 0xFF);
            abDir[45] = (byte)((endEncryption >> 16) & 0xFF);
            abDir[46] = (byte)((endEncryption >> 8) & 0xFF);
            abDir[47] = (byte)(endEncryption & 0xFF);
            abDir[48] = abDir[8];
            abDir[49] = (int)((endEncryption >> 32) & 0xFF);
            abDir[50] = (int)((endEncryption >> 40) & 0xFF);

            abDir[51] = (byte)((pageId >> 24) & 0xFF);
            abDir[52] = (byte)((pageId >> 16) & 0xFF);
            abDir[53] = (byte)((pageId >> 8) & 0xFF);
            abDir[54] = (byte)(pageId & 0xFF);
            abDir[55] = pageId;
            abDir[56] = aid;
            abDir[57] = (byte)(aid & 0xFF);
            abDir[58] = (byte)((aid >> 8) & 0xFF);
            abDir[59] = (byte)((aid >> 16) & 0xFF);
            abDir[60] = (byte)((aid >> 24) & 0xFF);

            abDir[64] = browserFp.Length;
            abDir[65] = browserFp.Length;

            // 排序取值
            var sortedValues = sortIndex
              .Select(k => Convert.ToInt32(abDir.GetValueOrDefault(k, 0)))
              .ToList();

            var fpArray = StringProcessor.ToOrdArray(browserFp).ToList();

            int abXor = 0;
            abXor = sortIndex2
                .Select(k => Convert.ToInt32(abDir.GetValueOrDefault(k, 0)))
                .Aggregate(0, (x, y) => x ^ y);


            sortedValues.AddRange(fpArray);
            sortedValues.Add(abXor);

            string randomBytes = StringProcessor.GenerateRandomBytes();
            string transformed = cryptoUtility.TransformBytes(sortedValues.ToArray());

            string abogusBytesStr = randomBytes + transformed;
            string abogus = cryptoUtility.AbogusEncode(abogusBytesStr, 0);

            string finalParams = $"{paramsStr}&a_bogus={abogus}";

            return (finalParams, abogus, userAgent, body);
        }
    }

    // 测试代码
    //public class ABogusTest
    //{
    //    //public static void Main()
    //    //{
    //    //    string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36 Edg/131.0.0.0";
    //    //    string edgeFp = BrowserFingerprintGenerator.GenerateFingerprint("Edge");
    //    //    var abogus = new ABogus2(fp: edgeFp, userAgent: userAgent);

    //    //    // GET请求测试
    //    //    string getParams = "device_platform=webapp&aid=6383&channel=channel_pc_web&sec_user_id=MS4wLjABAAAArDVBosPJF3eIWVEFp0szuJ-e1V_-rK0ieJeWwpE77E8&max_cursor=0&locate_query=false&show_live_replay_strategy=1&need_time_list=1&time_list_query=0&whale_cut_token=&cut_version=1&count=18&publish_video_strategy_type=2&from_user_page=1&update_version_code=170400&pc_client_type=1&pc_libra_divert=Windows&support_h265=1&support_dash=0&version_code=290100&version_name=29.1.0&cookie_enabled=true&screen_width=1920&screen_height=1080&browser_language=zh-CN&browser_platform=Win32&browser_name=Edge&browser_version=131.0.0.0&browser_online=true&engine_name=Blink&engine_version=131.0.0.0&os_name=Windows&os_version=10&cpu_core_num=12&device_memory=8&platform=PC&downlink=10&effective_type=4g&round_trip_time=50";
    //    //    var getResult = abogus.GenerateABogus(getParams);
    //    //    Console.WriteLine($"GET 完整URL: https://www.douyin.com/aweme/v1/web/aweme/detail/?{getResult.Params}");
    //    //    Console.WriteLine($"GET ABogus: {getResult.ABogus}");

    //    //    // POST请求测试
    //    //    string postParams = "device_platform=webapp&aid=6383&channel=channel_pc_web&pc_client_type=1&pc_libra_divert=Windows&update_version_code=170400&support_h265=1&support_dash=0&version_code=170400&version_name=17.4.0&cookie_enabled=true&screen_width=1920&screen_height=1080&browser_language=zh-CN&browser_platform=Win32&browser_name=Edge&browser_version=131.0.0.0&browser_online=true&engine_name=Blink&engine_version=131.0.0.0&os_name=Windows&os_version=10&cpu_core_num=12&device_memory=8&platform=PC&downlink=10&effective_type=4g&round_trip_time=50";
    //    //    string postBody = "aweme_type=0&item_id=7467485482314763572&play_delta=1&source=0";
    //    //    var postResult = abogus.GenerateABogus(postParams, postBody);
    //    //    Console.WriteLine($"POST 完整URL: https://www.douyin.com/aweme/v2/web/aweme/stats/?{postResult.Params}");
    //    //    Console.WriteLine($"POST ABogus: {postResult.ABogus}");
    //    //    Console.WriteLine($"POST Body: {postResult.Body}");
    //    //}
    //}
}

