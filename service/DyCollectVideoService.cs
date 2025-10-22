using dy.net.dto;
using dy.net.model;
using dy.net.repository;
using dy.net.utils;
using System.ComponentModel;
using System.Threading.Tasks;

namespace dy.net.service
{
    public class DyCollectVideoService
    {

        private readonly DyCollectVideoRepository _dyCollectVideoRepository;

        public DyCollectVideoService(DyCollectVideoRepository dyCollectVideoRepository)
        {
            _dyCollectVideoRepository = dyCollectVideoRepository;
        }


        public async Task<bool> batchInsert(List<DyCollectVideo> videos)
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

            List<DyCollectVideo> list = await this._dyCollectVideoRepository.GetAllAsync();
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
                FavoriteCount = list.Count(x => x.ViedoType == "1"),
                CollectCount = list.Count(x => x.ViedoType == "2"),
                FollowCount = list.Count(x => x.ViedoType == "3"),

                VideoSizeTotal = ByteToGbConverter.ConvertBytesToGb(list.Sum(x => x.FileSize)),
                VideoFavoriteSize = ByteToGbConverter.ConvertBytesToGb(list.Where(x => x.ViedoType == "1").Sum(x => x.FileSize)),
                VideoCollectSize = ByteToGbConverter.ConvertBytesToGb(list.Where(x => x.ViedoType == "2").Sum(x => x.FileSize)),
                VideoFollowSize = ByteToGbConverter.ConvertBytesToGb(list.Where(x => x.ViedoType == "3").Sum(x => x.FileSize)),

                TotalDiskSize= ByteToGbConverter.GetHostTotalDiskSpaceGB(),
            };
            data.Authors = list.GroupBy(x => x.Author).Select(x => new VideoStaticsItemDto { Name = x.Key, Count = x.LongCount(), Icon = x.FirstOrDefault().AuthorAvatarUrl }).OrderByDescending(d => d.Count).ToList();
            return data;
        }


        //分页查询

        public async Task<(List<DyCollectVideo> list, int totalCount)> GetPagedAsync(int pageIndex, int pageSize, string tag = null, string author = null, string viedoType = null, List<string>? dates = null)
        {
            return await _dyCollectVideoRepository.GetPagedAsync(pageIndex, pageSize, tag, author, viedoType, dates);
        }

    }



}
