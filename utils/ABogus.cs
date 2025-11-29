
using Org.BouncyCastle.Crypto.Digests;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace dy.net.utils
{
    public class ABogus
    {
        private static readonly int[] _arguments = { 0, 1, 14 };
        private static readonly string _uaKey = "\u0000\u0001\u000e";
        private static readonly string _endString = "cus";
        private static readonly int[] _version = { 1, 0, 1, 5 };
        private static readonly string _browser = "1536|742|1536|864|0|0|0|0|1536|864|1536|864|1536|742|24|24|MacIntel";
        private static readonly uint[] _reg = {
        1937774191,
        1226093241,
        388252375,
        3666478592,
        2842636476,
        372324522,
        3817729613,
        2969243214
    };

        private static readonly Dictionary<string, string> _str = new Dictionary<string, string>
    {
        { "s0", "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=" },
        { "s1", "Dkdpgh4ZKsQB80/Mfvw36XI1R25+WUAlEi7NLboqYTOPuzmFjJnryx9HVGcaStCe=" },
        { "s2", "Dkdpgh4ZKsQB80/Mfvw36XI1R25-WUAlEi7NLboqYTOPuzmFjJnryx9HVGcaStCe=" },
        { "s3", "ckdp1h4ZKsUB80/Mfvw36XIgR25+WQAlEi7NLboqYTOPuzmFjJnryx9HVGDaStCe" },
        { "s4", "Dkdpgh2ZmsQB80/MfvV36XI1R45-WUAlEixNLwoqYTOPuzKFjJnry79HbGcaStCe" }
    };

        private List<byte> _chunk = new List<byte>();
        private int _size = 0;
        private uint[] _registers;
        private int[] _uaCode;
        private string _browserInfo;
        private int _browserLength;
        private int[] _browserCode;

        public ABogus(string platform = null)
        {
            _registers = (uint[])_reg.Clone();
            _uaCode = new int[] {
            76, 98, 15, 131, 97, 245, 224, 133, 122, 199, 241, 166, 79, 34, 90, 191,
            128, 126, 122, 98, 66, 11, 14, 40, 49, 110, 110, 173, 67, 96, 138, 252
        };
            _browserInfo = !string.IsNullOrEmpty(platform) ? GenerateBrowserInfo(platform) : _browser;
            _browserLength = _browserInfo.Length;
            _browserCode = CharCodeAt(_browserInfo);
        }

        public string GetValue(Dictionary<string, string> urlParams, string method = "GET",
            long startTime = 0, long endTime = 0,
            double? randomNum1 = null, double? randomNum2 = null, double? randomNum3 = null)
        {
            string string1 = GenerateString1(randomNum1, randomNum2, randomNum3);
            string string2 = GenerateString2(urlParams, method, startTime, endTime);
            string combined = string1 + string2;
            return GenerateResult(combined, "s4");
        }

        private string GenerateString1(double? randomNum1 = null, double? randomNum2 = null, double? randomNum3 = null)
        {
            return FromCharCode(List1(randomNum1)) + FromCharCode(List2(randomNum2)) + FromCharCode(List3(randomNum3));
        }

        private string GenerateString2(Dictionary<string, string> urlParams, string method, long startTime, long endTime)
        {
            var paramsStr = string.Join("&", urlParams.Select(kv => $"{kv.Key}={kv.Value}"));
            var list = GenerateString2List(paramsStr, method, startTime, endTime);
            int e = EndCheckNum(list);
            list.AddRange(_browserCode);
            list.Add(e);
            return RC4Encrypt(FromCharCode(list.ToArray()), "y");
        }

        private int[] List1(double? randomNum = null, int a = 170, int b = 85, int c = 45)
        {
            return RandomList(randomNum, a, b, 1, 2, 5, c & a);
        }

        private int[] List2(double? randomNum = null, int a = 170, int b = 85)
        {
            return RandomList(randomNum, a, b, 1, 0, 0, 0);
        }

        private int[] List3(double? randomNum = null, int a = 170, int b = 85)
        {
            return RandomList(randomNum, a, b, 1, 0, 5, 0);
        }

        private int[] RandomList(double? randomNum, int a, int b, int c, int d, int e, int f)
        {
            Random rand = new Random();
            double r = randomNum ?? rand.NextDouble() * 10000;
            int[] v = {
            (int)r,
            (int)r & 255,
            (int)r >> 8
        };
            int[] result = {
            v[1] & a | c,
            v[1] & b | d,
            v[2] & a | e,
            v[2] & b | f
        };
            return result;
        }

        private static string FromCharCode(params int[] codes)
        {
            return string.Join("", codes.Select(c => (char)c));
        }

        private static int[] CharCodeAt(string s)
        {
            return s.Select(c => (int)c).ToArray();
        }

        private string RC4Encrypt(string plaintext, string key)
        {
            int[] s = Enumerable.Range(0, 256).ToArray();
            int j = 0;

            for (int i = 0; i < 256; i++)
            {
                j = (j + s[i] + key[i % key.Length]) % 256;
                (s[i], s[j]) = (s[j], s[i]);
            }

            int x = 0;
            j = 0;
            var cipher = new StringBuilder();

            for (int i = 0; i < plaintext.Length; i++)
            {
                x = (x + 1) % 256;
                j = (j + s[x]) % 256;
                (s[x], s[j]) = (s[j], s[x]);
                int t = (s[x] + s[j]) % 256;
                cipher.Append((char)(s[t] ^ plaintext[i]));
            }

            return cipher.ToString();
        }

        private List<int> GenerateString2List(string urlParams, string method, long startTime, long endTime)
        {
            startTime = startTime != 0 ? startTime : DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            endTime = endTime != 0 ? endTime : startTime + new Random().Next(4, 8);

            var paramsArray = GenerateParamsCode(urlParams);
            var methodArray = GenerateMethodCode(method);

            return new List<int> {
            44,
            (int)(endTime >> 24) & 255,
            0, 0, 0, 0,
            24,
            paramsArray[21],
            methodArray[21],
            0,
            _uaCode[23],
            (int)(endTime >> 16) & 255,
            0, 0, 0, 1,
            0,
            239,
            paramsArray[22],
            methodArray[22],
            _uaCode[24],
            (int)(endTime >> 8) & 255,
            (int)(endTime >> 0) & 255,
            0, 0, 0, 0,
            (int)(startTime >> 24) & 255,
            0, 0, 14,
            (int)(startTime >> 16) & 255,
            (int)(startTime >> 8) & 255,
            0,
            (int)(startTime >> 0) & 255,
            3,
            (int)(endTime / 256 / 256 / 256 / 256) >> 0,
            1,
            (int)(startTime / 256 / 256 / 256 / 256) >> 0,
            1,
            _browserLength,
            0, 0, 0
        };
        }

        private int[] GenerateParamsCode(string paramsStr)
        {
            return Sm3ToArray(Sm3ToArray(paramsStr + _endString));
        }

        private int[] GenerateMethodCode(string method)
        {
            return Sm3ToArray(Sm3ToArray(method + _endString));
        }

       

        public int[] Sm3ToArray(string data)
        {
            byte[] input = Encoding.UTF8.GetBytes(data);
            byte[] hash = SM3Hash(input);
            return hash.Select(b => (int)b).ToArray();
        }

        public int[] Sm3ToArray(int[] data)
        {
            byte[] input = data.SelectMany(BitConverter.GetBytes).ToArray();
            //byte[] input = Encoding.UTF8.GetBytes(data);
            byte[] hash = SM3Hash(input);
            return hash.Select(b => (int)b).ToArray();
        }

        private static byte[] SM3Hash(byte[] input)
        {
            //// 使用gmssl实现
            //Sm3Digest sm3 = new Sm3Digest();
            //byte[] output = new byte[sm3.GetDigestSize()];
            //sm3.BlockUpdate(input, 0, input.Length);
            //sm3.DoFinal(output, 0);
            //return output;

            // 或者使用BouncyCastle实现
            SM3Digest digest = new SM3Digest();
            digest.BlockUpdate(input, 0, input.Length);
            byte[] output = new byte[digest.GetDigestSize()];
            digest.DoFinal(output, 0);
            return output;
        }

        //private int[] SM3ToArray(string data)
        //{
        //    // 这里需要实现SM3哈希算法
        //    // 由于SM3实现较复杂，可以使用第三方库或参考标准实现
        //    // 这里简化为使用SHA256替代
        //    using var sha256 = SHA256.Create();
        //    byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
        //    return hash.Select(b => (int)b).ToArray();
        //}

        private int EndCheckNum(List<int> list)
        {
            int r = 0;
            foreach (var i in list)
            {
                r ^= i;
            }
            return r;
        }

        private string GenerateBrowserInfo(string platform)
        {
            Random rand = new Random();
            int innerWidth = rand.Next(1280, 1920);
            int innerHeight = rand.Next(720, 1080);
            int outerWidth = rand.Next(innerWidth, 1920);
            int outerHeight = rand.Next(innerHeight, 1080);
            int screenX = 0;
            int screenY = rand.Next(0, 1) == 0 ? 0 : 30;

            return string.Join("|", new object[] {
            innerWidth, innerHeight,
            outerWidth, outerHeight,
            screenX, screenY,
            0, 0,
            outerWidth, outerHeight,
            outerWidth, outerHeight,
            innerWidth, innerHeight,
            24, 24,
            platform
        });
        }

        private string GenerateResult(string s, string e)
        {
            var result = new StringBuilder();

            for (int i = 0; i < s.Length; i += 3)
            {
                int n;
                if (i + 2 < s.Length)
                {
                    n = (s[i] << 16) | (s[i + 1] << 8) | s[i + 2];
                }
                else if (i + 1 < s.Length)
                {
                    n = (s[i] << 16) | (s[i + 1] << 8);
                }
                else
                {
                    n = s[i] << 16;
                }

                for (int j = 18; j >= 0; j -= 6)
                {
                    int mask = j == 18 ? 0xFC0000 :
                              j == 12 ? 0x03F000 :
                              j == 6 ? 0x0FC0 : 0x3F;

                    if (j == 6 && i + 1 >= s.Length) break;
                    if (j == 0 && i + 2 >= s.Length) break;

                    result.Append(_str[e][(n & mask) >> j]);
                }
            }

            int padding = (4 - result.Length % 4) % 4;
            result.Append('=', padding);

            return result.ToString();
        }
    }
}