using dy.net.dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;

namespace dy.net.extension
{
    public class DouyinExceptionFilter : IExceptionFilter
    {


        /// <summary>
        /// 异常拦截核心方法
        /// </summary>
        public void OnException(ExceptionContext context)
        {
            // 1. 记录异常日志（区分业务异常和系统异常）
            LogException(context);

            // 2. 构建标准化响应结果
            var response = BuildExceptionResponse(context.Exception);

            // 3. 设置响应结果（终止异常传播）
            context.Result = new ObjectResult(response);
            context.ExceptionHandled = true;
        }

        /// <summary>
        /// 记录异常日志
        /// </summary>
        private void LogException(ExceptionContext ex)
        {
            Serilog.Log.Error($"系统异常：{ex.Exception.Message},{ex.HttpContext.Request.Path}");
        }

        /// <summary>
        /// 根据异常类型构建响应结果
        /// </summary>
        private static DouyinApiResponse<object> BuildExceptionResponse(Exception ex)
        {
            return new DouyinApiResponse<object>
            {
                Code = ResponseCode.ServerError,
                Message = "服务器内部错误，请稍后重试",
                Data = null,
                Timestamp = DateTime.UtcNow
            };
        }
    }


    /// <summary>
    /// 全局异常过滤器扩展（简化注册）
    /// </summary>
    public static class GlobalExceptionFilterExtensions
    {
        public static IMvcBuilder AddGlobalExceptionFilter(this IMvcBuilder mvcBuilder)
        {
            mvcBuilder.AddMvcOptions(options =>
            {
                options.Filters.Add<DouyinExceptionFilter>();
            });
            return mvcBuilder;
        }
    }
}
