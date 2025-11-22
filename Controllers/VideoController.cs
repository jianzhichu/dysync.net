using dy.net.dto;
using dy.net.service;
using dy.net.utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Drawing.Printing;

namespace dy.net.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VideoController : ControllerBase
    {
        private readonly DouyinVideoService dyCollectVideoService;

        public VideoController(DouyinVideoService dyCollectVideoService)
        {
            this.dyCollectVideoService = dyCollectVideoService;
        }
        /// <summary>
        /// 分页查询收藏视频
        /// </summary>
        /// <param name="dto"></param>
        [HttpPost("paged")]
        public async Task<IActionResult> GetPagedAsync(DouyinVideoPageRequestDto dto)
        {
            var (list, totalCount) = await dyCollectVideoService.GetPagedAsync(dto.PageIndex, dto.PageSize, dto.Tag, dto.Author,dto.ViedoType,dto.Dates);
            return Ok(new
            {
                code = 0,
                data = new
                {
                    data = list,
                    total=totalCount,
                    pageIndex=dto.PageIndex,
                    pageSize=dto.PageSize
                }
            });
        }

        /// <summary>
        /// 查询统计数据
        /// </summary>
        /// <returns></returns>
        [HttpGet("statics")]
        public async Task<IActionResult> GetStaticsAsync()
        {
            var data = await dyCollectVideoService.GetStatics();
            return Ok(new
            {
                code = 0,
                data
            });
        }
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Index()
        {
            List<MyClass> myClasses = new List<MyClass>();
            foreach (var drive in DriveInfo.GetDrives())
            {
                myClasses.Add(new MyClass { name=drive.Name, x1= drive.TotalSize, x2= drive.TotalFreeSpace });
                Serilog.Log.Debug($"Drive: {drive.Name}, Total Size: {drive.TotalSize}, Free Space: {drive.TotalFreeSpace}");
            }
            //var disk=DiskInfoHelper.GetDockerHostTotalDiskSpaceGB();
            return Ok(JsonConvert.SerializeObject(myClasses)+"\r\n"+JsonConvert.SerializeObject(myClasses.Sum(x=>x.x1)+"\r\n"+ JsonConvert.SerializeObject(myClasses.Sum(x => x.x2))));
        }


        public class MyClass
        {
            public string name { get; set; }

            public long x1 { get; set; }

            public long x2 { get; set; }
        }
    }
}
