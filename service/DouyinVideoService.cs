using dy.net.dto;
using dy.net.model;
using dy.net.repository;
using dy.net.utils;
using System.ComponentModel;
using System.Threading.Tasks;

namespace dy.net.service
{
    public class DouyinVideoService
    {

        private readonly DouyinVideoRepository _dyCollectVideoRepository;

        public DouyinVideoService(DouyinVideoRepository dyCollectVideoRepository)
        {
            _dyCollectVideoRepository = dyCollectVideoRepository;
        }


        public async Task<bool> batchInsert(List<DouyinVideo> videos)
        {

            // 边界处理：如果传入的列表为空，直接返回成功（或根据业务返回false）
            if (videos == null || !videos.Any())
                return true;

            // 1. 提取待插入的所有AwemeId（去重，减少数据库查询压力）
            var newAwemeIds = videos.Select(x => x.AwemeId)
                                    .Distinct()
                                    .ToList();

            // 2. 查询数据库中已存在的AwemeId（只查需要的字段，提高效率）
            var existingAwemeIds = await _dyCollectVideoRepository
                .Query(x => newAwemeIds.Contains(x.AwemeId)) // 使用Query方法构建查询
                .Select(x => x.AwemeId) // 只获取AwemeId，减少数据传输
                .ToListAsync();

            // 3. 过滤出数据库中不存在的视频（只保留新记录）
            var videosToInsert = videos
                .Where(video => !existingAwemeIds.Contains(video.AwemeId))
                .ToList();

            // 4. 如果没有需要插入的新记录，直接返回成功
            if (!videosToInsert.Any())
                return true;

            // 5. 批量插入过滤后的新记录
            var insertedCount = await _dyCollectVideoRepository.InsertRangeAsync(videosToInsert);

            // 返回是否插入成功（至少插入一条）
            return insertedCount > 0;
        }


        public async Task<VideoStaticsDto> GetStatics()
        {

            List<DouyinVideo> list = await this._dyCollectVideoRepository.GetAllAsync();
            if (!list.Any())
                return new VideoStaticsDto();
            var Categories = list.GroupBy(x => x.Tag1).Select(x => new VideoStaticsItemDto { Name = x.Key, Count = x.LongCount() }).OrderByDescending(p => p.Count).ToList();
            Categories.Where(x => string.IsNullOrWhiteSpace(x.Name)).ToList().ForEach(x => x.Name = "其他");
            var data = new VideoStaticsDto
            {
                AuthorCount = list.Select(x => x.AuthorId).Distinct().Count(),
                CategoryCount = list.Select(x => x.Tag1).Distinct().Count(),
                VideoCount = list.Count,
                Categories = Categories,
                FavoriteCount = list.Count(x => x.ViedoType == VideoTypeEnum.Favorite),
                CollectCount = list.Count(x => x.ViedoType == VideoTypeEnum.Collect),
                FollowCount = list.Count(x => x.ViedoType == VideoTypeEnum.UperPost),
                GraphicVideoCount = list.Count(x => x.ViedoType == VideoTypeEnum.ImageVideo),

                VideoSizeTotal = ByteToGbConverter.ConvertBytesToGb(list.Sum(x => x.FileSize)),
                VideoFavoriteSize = ByteToGbConverter.ConvertBytesToGb(list.Where(x => x.ViedoType == VideoTypeEnum.Favorite).Sum(x => x.FileSize)),
                VideoCollectSize = ByteToGbConverter.ConvertBytesToGb(list.Where(x => x.ViedoType == VideoTypeEnum.Collect).Sum(x => x.FileSize)),
                VideoFollowSize = ByteToGbConverter.ConvertBytesToGb(list.Where(x => x.ViedoType == VideoTypeEnum.UperPost).Sum(x => x.FileSize)),
                GraphicVideoSize = ByteToGbConverter.ConvertBytesToGb(list.Where(x => x.ViedoType == VideoTypeEnum.ImageVideo).Sum(x => x.FileSize)),

                //TotalDiskSize= ByteToGbConverter.GetHostTotalDiskSpaceGB(),
            };
            if (data.GraphicVideoSize == "0.00")
            {
                if(list.Where(x => x.ViedoType == VideoTypeEnum.ImageVideo).Sum(x => x.FileSize) > 0)
                {
                    data.GraphicVideoSize = "<0.01";//避免显示0.00误导用户
                }
            }
            data.Authors = list.GroupBy(x => x.Author).Select(x => new VideoStaticsItemDto { Name = x.Key, Count = x.LongCount(), Icon = x.FirstOrDefault().AuthorAvatarUrl }).OrderByDescending(d => d.Count).ToList();
            return data;
        }


        //分页查询

        public async Task<(List<DouyinVideo> list, int totalCount)> GetPagedAsync(int pageIndex, int pageSize, string tag = null, string author = null, string viedoType = null, List<string>? dates = null)
        {
            return await _dyCollectVideoRepository.GetPagedAsync(pageIndex, pageSize, tag, author, viedoType, dates);
        }


        /// <summary>
        /// 关注的博主的视频如果配置为视频标题作为文件名，生成文件名
        /// </summary>
        /// <param name="AuthorId"></param>
        /// <param name="ViedoNameSimplify"></param>
        /// <returns></returns>
        public async Task<(string, string)> GetUperLastViedoFileName(string AuthorId, string ViedoNameSimplify)
        {

            return await _dyCollectVideoRepository.GetUperLastViedoFileName(AuthorId, ViedoNameSimplify);
        }

        /// <summary>
        /// 根据ID获取视频信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<DouyinVideo> GetById(string id)
        {
            return await _dyCollectVideoRepository.GetByIdAsync(id);
        }

    }



}
