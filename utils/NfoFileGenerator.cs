using dy.net.model.dto;
using dy.net.model.entity;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace dy.net.utils
{
    /// <summary>
    /// NFO文件生成器
    /// </summary>
    public class NfoFileGenerator
    {


        /// <summary>
        /// 生成NFO文件
        /// NFO文件包含视频的元数据信息，如标题、作者、封面等
        /// </summary>
        /// <param name="video">视频信息</param>
        /// <returns>一个表示异步操作的任务</returns>
        public static  void GenerateVideoNfoFile(DouyinVideo video)
        {
            try
            {

                string videoDirectory = Path.GetDirectoryName(video.VideoSavePath); // 视频所在目录
                string videoFileNameWithoutExt = Path.GetFileNameWithoutExtension(video.VideoSavePath); // 无扩展名的文件名
                string nfoFullPath = Path.Combine(videoDirectory, $"{videoFileNameWithoutExt}.nfo"); // NFO文件完整路径
                //string postFullPath = Path.Combine(videoDirectory, "poster.jpg"); // NFO文件完整路径

                if (!string.IsNullOrWhiteSpace(video.AuthorAvatar))
                {
                    if (!video.OnlyImgOrOnlyMp3)
                    {
                        //说明是视频
                        //复制作者头像到当前目录 并改名为跟nfo里面的作者相同的名字
                        if (File.Exists(video.AuthorAvatar))
                        {
                            var fileExt = Path.GetExtension(video.AuthorAvatar);
                            var actorsDir = Path.Combine(videoDirectory, ".actors");
                            if(!Directory.Exists(actorsDir))
                            {
                                Directory.CreateDirectory(actorsDir);
                            }
                            var nfoActorFullPath = Path.Combine(actorsDir, $"{video.Author}{fileExt}");

                            if (!File.Exists(nfoActorFullPath))
                            {
                                File.Copy(video.AuthorAvatar, nfoActorFullPath, overwrite: true);
                                //File.Delete(video.AuthorAvatar);
                            }
                        }
                    }
                }


              var nfoInfo=  new DouyinVideoNfo
                {
                    Actors = new List<Actor>
                    {
                        new() {
                            Name = video.Author,
                            Role = "主演",
                        }
                    },
                    Author = video.Author,
                    Poster = "poster.jpg",
                    Title = video.VideoTitle,
                    Thumbnail = "poster.jpg",// 使用poster作为缩略图
                    ReleaseDate = video.CreateTime,
                    Genres = new List<string> { video.Tag1, video.Tag2, video.Tag3 }.Where(t => !string.IsNullOrWhiteSpace(t)).ToList()
                };
                if (video.ViedoType == VideoTypeEnum.dy_mix || video.ViedoType == VideoTypeEnum.dy_series)
                {
                    string directory = Path.GetDirectoryName(nfoFullPath);
                    nfoFullPath = Path.Combine(directory, "tvshow.nfo");
                    if (!File.Exists(nfoFullPath))
                        GenerateNfoFile(nfoInfo, nfoFullPath, "tvshow");
                }
                else
                {
                    GenerateNfoFile(nfoInfo, nfoFullPath);
                }
               
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "{f}nfo文件生成异常", video.VideoTitle);
            }
        }

        private static void GenerateNfoFile(DouyinVideoNfo videoInfo, string filePath,string xmlRoot= "movie")
        {
            try
            {
                if (videoInfo == null)
                    throw new ArgumentNullException(nameof(videoInfo));

                if (string.IsNullOrWhiteSpace(filePath))
                    throw new ArgumentException("文件路径不能为空", nameof(filePath));

                // 创建根元素
                XElement root = new XElement(xmlRoot);
                //root.Add(new XElement("outline")); 
                root.Add(new XElement("lockdata", true));
                //root.Add(new XElement("director", videoInfo.Author));
                //root.Add(new XElement("plot", $"<![CDATA[{videoInfo.Title}]]>"));

                // 添加视频信息（先清理无效字符）
                if (!string.IsNullOrWhiteSpace(videoInfo.Title))
                    root.Add(new XElement("title", CleanInvalidXmlChars(videoInfo.Title)));

                //if (!string.IsNullOrWhiteSpace(videoInfo.Author))
                //    root.Add(new XElement("author", CleanInvalidXmlChars(videoInfo.Author)));

                if (videoInfo.ReleaseDate.HasValue)
                {
                    root.Add(new XElement("releasedate", videoInfo.ReleaseDate.Value.ToString("yyyy-MM-dd")));
                    root.Add(new XElement("premiered", videoInfo.ReleaseDate.Value.ToString("yyyy-MM-dd")));
                }

                // 分类标签（清理每个标签）
                if (videoInfo.Genres != null && videoInfo.Genres.Any())
                {
                    foreach (var genre in videoInfo.Genres)
                    {
                        if (!string.IsNullOrWhiteSpace(genre))
                            root.Add(new XElement("genre", CleanInvalidXmlChars(genre)));
                    }
                }

                // --- 新增：处理演员信息 ---
                if (videoInfo.Actors != null && videoInfo.Actors.Any())
                {
                    foreach (var actor in videoInfo.Actors)
                    {
                        // 至少需要演员姓名
                        if (!string.IsNullOrWhiteSpace(actor.Name))
                        {
                            var actorElement = new XElement("actor");
                            actorElement.Add(new XElement("name", CleanInvalidXmlChars(actor.Name)));
                            if (!string.IsNullOrWhiteSpace(actor.Role))
                                actorElement.Add(new XElement("role", CleanInvalidXmlChars(actor.Role)));

                                actorElement.Add(new XElement("tmdbid", ""));

                            root.Add(actorElement);
                        }
                    }
                }
                // --- 演员信息处理结束 ---

                if (!string.IsNullOrWhiteSpace(videoInfo.Thumbnail))
                    root.Add(new XElement("thumb", new XAttribute("aspect", "poster"), CleanInvalidXmlChars(videoInfo.Thumbnail)));

                if (!string.IsNullOrWhiteSpace(videoInfo.Poster))
                    root.Add(new XElement("fanart",
                        new XElement("thumb", CleanInvalidXmlChars(videoInfo.Poster))
                    ));

                // 创建XDocument并保存
                XDocument doc = new XDocument(
                    new XDeclaration("1.0", "utf-8", "yes"),
                    root
                );

                // 确保目录存在
                string directory = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                doc.Save(filePath);
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, $"生成 {videoInfo?.Title ?? "未知视频"} 的 NFO 文件时出错。");
            }
        }

        /// <summary>
        /// 清理字符串中无效的XML字符（包括无效的UTF-16代理对）
        /// </summary>
        private static string CleanInvalidXmlChars(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            StringBuilder cleaned = new StringBuilder();
            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];
                // XML 1.0 允许的字符范围：#x9 | #xA | #xD | [#x20-#xD7FF] | [#xE000-#xFFFD] | [#x10000-#x10FFFF]
                if (IsValidXmlChar(c))
                {
                    // 检查是否是高代理项，如果是则需要配对低代理项
                    if (char.IsHighSurrogate(c))
                    {
                        // 确保不越界，且下一个字符是低代理项
                        if (i + 1 < input.Length && char.IsLowSurrogate(input[i + 1]))
                        {
                            cleaned.Append(c);       // 高代理项
                            cleaned.Append(input[i + 1]); // 低代理项
                            i++; // 跳过已处理的低代理项
                        }
                        // 否则，高代理项无效，跳过
                    }
                    else
                    {
                        cleaned.Append(c);
                    }
                }
                // 无效字符直接跳过
            }
            return cleaned.ToString();
        }

        /// <summary>
        /// 判断字符是否为XML允许的有效字符
        /// </summary>
        private static bool IsValidXmlChar(char c)
        {
            return (c == 0x9) || (c == 0xA) || (c == 0xD)
                   || (c >= 0x20 && c <= 0xD7FF)
                   || (c >= 0xE000 && c <= 0xFFFD)
                   || (c >= 0x10000 && c <= 0x10FFFF);
        }
    }
}
