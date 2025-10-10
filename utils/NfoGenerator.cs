using dy.net.dto;
using System.Xml.Linq;

namespace dy.net.utils
{
    /// <summary>
    /// NFO文件生成器，负责将VideoInfo对象转换为XML格式的NFO文件
    /// </summary>
    public class NfoGenerator
    {
        /// <summary>
        /// 生成视频NFO文件
        /// </summary>
        /// <param name="videoInfo">视频信息对象，包含所有需要写入NFO的元数据</param>
        /// <param name="outputPath">输出文件的完整路径，包括文件名</param>
        public void GenerateNfoFile(VideoNFOInfo videoInfo, string outputPath)
        {
            try
            {
                // 创建根元素，电影用"movie"，电视剧集用"episodedetails"，电视节目用"tvshow"
                XElement root = new XElement("movie");

                // 添加基本信息
                if (!string.IsNullOrWhiteSpace(videoInfo.Title))
                    root.Add(new XElement("title", videoInfo.Title));

                if (!string.IsNullOrWhiteSpace(videoInfo.OriginalTitle))
                    root.Add(new XElement("originaltitle", videoInfo.OriginalTitle));

                if (!string.IsNullOrWhiteSpace(videoInfo.SortTitle))
                    root.Add(new XElement("sorttitle", videoInfo.SortTitle));

                if (videoInfo.Year > 0)
                    root.Add(new XElement("year", videoInfo.Year));

                if (!string.IsNullOrWhiteSpace(videoInfo.Plot))
                    root.Add(new XElement("plot", videoInfo.Plot));

                if (!string.IsNullOrWhiteSpace(videoInfo.Outline))
                    root.Add(new XElement("outline", videoInfo.Outline));

                if (!string.IsNullOrWhiteSpace(videoInfo.Tagline))
                    root.Add(new XElement("tagline", videoInfo.Tagline));

                // 添加人员信息
                if (!string.IsNullOrWhiteSpace(videoInfo.Director))
                    root.Add(new XElement("director", videoInfo.Director));

                // 添加演员
                foreach (var actor in videoInfo.Actors)
                {
                    root.Add(new XElement("actor",
                        new XElement("name", actor)
                    ));
                }

                // 添加编剧
                foreach (var writer in videoInfo.Writers)
                {
                    root.Add(new XElement("writer", writer));
                }

                // 添加媒体信息
                if (!string.IsNullOrWhiteSpace(videoInfo.Genre))
                    root.Add(new XElement("genre", videoInfo.Genre));

                if (videoInfo.Rating > 0)
                    root.Add(new XElement("rating", videoInfo.Rating));

                if (videoInfo.Votes > 0)
                    root.Add(new XElement("votes", videoInfo.Votes));

                if (!string.IsNullOrWhiteSpace(videoInfo.Studio))
                    root.Add(new XElement("studio", videoInfo.Studio));

                if (videoInfo.Premiered.HasValue)
                    root.Add(new XElement("premiered", videoInfo.Premiered.Value.ToString("yyyy-MM-dd")));

                if (!string.IsNullOrWhiteSpace(videoInfo.Runtime))
                    root.Add(new XElement("runtime", videoInfo.Runtime));

                // 添加文件信息
                if (!string.IsNullOrWhiteSpace(videoInfo.FileName))
                    root.Add(new XElement("filenameandpath", videoInfo.FileName));

                if (videoInfo.FileSize > 0)
                    root.Add(new XElement("filesize", videoInfo.FileSize));

                // 添加新增字段
                if (!string.IsNullOrWhiteSpace(videoInfo.Country))
                    root.Add(new XElement("country", videoInfo.Country));

                if (!string.IsNullOrWhiteSpace(videoInfo.Language))
                    root.Add(new XElement("language", videoInfo.Language));

                if (!string.IsNullOrWhiteSpace(videoInfo.VideoCodec))
                    root.Add(new XElement("codec", videoInfo.VideoCodec));

                if (!string.IsNullOrWhiteSpace(videoInfo.AudioCodec))
                    root.Add(new XElement("audiocodec", videoInfo.AudioCodec));

                if (!string.IsNullOrWhiteSpace(videoInfo.Resolution))
                    root.Add(new XElement("resolution", videoInfo.Resolution));

                // 创建文档并保存
                XDocument doc = new XDocument(
                    new XDeclaration("1.0", "UTF-8", "yes"),
                    root
                );

                // 确保目录存在
                var directory = Path.GetDirectoryName(outputPath);
                if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                doc.Save(outputPath);
                Console.WriteLine($"NFO文件已生成: {outputPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"生成NFO文件时出错: {ex.Message}");
            }
        }
    }

}
