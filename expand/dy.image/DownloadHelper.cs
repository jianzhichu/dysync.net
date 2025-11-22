using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dy.image
{

    public class DownloadHelper
    {
        private readonly HttpClient _httpClient;

        public DownloadHelper(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.Timeout = TimeSpan.FromSeconds(60); // 下载超时30秒
            _httpClient.DefaultRequestHeaders.Add("Accept-Language", "zh-CN,zh;q=0.8,zh-TW;q=0.7,zh-HK;q=0.5,en-US;q=0.3,en;q=0.2");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/119.0.0.0 Safari/537.36");
            _httpClient.DefaultRequestHeaders.Add("Referer", "https://www.douyin.com");
        }

        /// <summary>下载网络文件到指定路径</summary>
        public async Task DownloadFileAsync(string url, string savePath)
        {
            if (string.IsNullOrEmpty(url)) throw new ArgumentNullException(nameof(url));
            if (string.IsNullOrEmpty(savePath)) throw new ArgumentNullException(nameof(savePath));

            // 创建目录（如果不存在）
            var directory = Path.GetDirectoryName(savePath)!;
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

            // 下载文件
            using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode(); // 非2xx状态码抛出异常

            using var stream = await response.Content.ReadAsStreamAsync();
            using var fileStream = new FileStream(savePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);
            await stream.CopyToAsync(fileStream);
        }
    }
}
