using dy.net.dto;
using System.Xml.Linq;

namespace dy.net.utils
{
    /// <summary>
    /// NFO文件生成器
    /// </summary>
    public class NfoFileGenerator
    {
        /// <summary>
        /// 根据视频信息生成NFO文件
        /// </summary>
        /// <param name="videoInfo">视频信息对象</param>
        /// <param name="filePath">保存NFO文件的路径</param>
        public static void GenerateNfoFile(VideoNfo videoInfo, string filePath)
        {
            if (videoInfo == null)
                throw new ArgumentNullException(nameof(videoInfo));

            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("文件路径不能为空", nameof(filePath));

            // 创建根元素
            XElement root = new XElement("movie"); // 对于电影使用"movie"，电视剧可以使用"tvshow"

            // 添加视频信息
            if (!string.IsNullOrEmpty(videoInfo.Title))
                root.Add(new XElement("title", videoInfo.Title));

            if (!string.IsNullOrEmpty(videoInfo.Author))
                root.Add(new XElement("author", videoInfo.Author));

            if (!string.IsNullOrEmpty(videoInfo.Thumbnail))
                root.Add(new XElement("thumb", new XAttribute("aspect", "poster"), videoInfo.Thumbnail));

            if (!string.IsNullOrEmpty(videoInfo.Poster))
                root.Add(new XElement("fanart",
                    new XElement("thumb", videoInfo.Poster)
                ));

            // 创建XDocument并保存
            XDocument doc = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                root
            );

            doc.Save(filePath);
        }
    }
}
