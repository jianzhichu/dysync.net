using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

namespace dy.net.model.dto
{
    public class DouyinApiResponse<T>
    {
        /// <summary>
        /// 响应状态码（自定义业务码，非HTTP状态码）
        /// </summary>
        [JsonPropertyName("code")]
        public int Code { get; set; }

        /// <summary>
        /// 响应数据
        /// </summary>
        [JsonPropertyName("data")]
        public T Data { get; set; }

        /// <summary>
        /// 响应消息（成功/失败描述）
        /// </summary>
        [JsonPropertyName("msg")]
        public string Msg { get; set; }
        [JsonPropertyName("message")]
        public string Message { get; set; }
        [JsonPropertyName("error")]
        public string Error { get; set; }

        /// <summary>
        /// 响应时间戳（UTC时间）
        /// </summary>
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }


    /// <summary>
    /// 响应状态码常量
    /// </summary>
    public static class ResponseCode
    {
        public const int Success = 0;        // 操作成功
        public const int ServerError = -1;    // 服务器错误
    }


    /// <summary>
    /// 直接返回 IActionResult 的API响应工具类
    /// 无需控制器手动拼接 Ok()/BadRequest()，一步返回标准化响应
    /// </summary>
    public static class ApiResult
    {
        #region 成功响应
        /// <summary>
        /// 成功响应（带数据）
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="data">响应数据</param>
        /// <param name="message">成功消息（默认：操作成功）</param>
        /// <returns>IActionResult（HTTP 200）</returns>
        public static IActionResult Success<T>(T data, string message = "操作成功")
        {
            var response = new DouyinApiResponse<T>
            {
                Code = ResponseCode.Success,
                Data = data,
                Message = message,
                Error = message,
                Msg = message,
            };
            return new OkObjectResult(response); // HTTP 200
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="code"></param>
        /// <param name="t"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static IActionResult SuccOrFail<T>(int code,T t,string message = "")
        {
            if (code == ResponseCode.Success)
            {
                return Success(t, message);
            }
            else
            {
                return Fail(message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="result"></param>
        /// <param name="t"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static IActionResult SuccOrFail<T>(bool result, T t, string message = "")
        {
            if (result)
            {
                return Success(t, message);
            }
            else
            {
                return Fail(message);
            }
        }
        /// <summary>
        /// 成功响应（无数据）
        /// </summary>
        /// <param name="message">成功消息（默认：操作成功）</param>
        /// <returns>IActionResult（HTTP 200）</returns>
        public static IActionResult Success(string message = "操作成功")
        {
            return Success<object>(null, message);
        }
        #endregion

        #region 失败响应
        /// <summary>
        /// 失败响应
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="message">错误消息</param>
        /// <param name="data">附加数据</param>
        /// <returns>IActionResult</returns>
        public static IActionResult Fail<T>(string message="请求失败", int businessCode = ResponseCode.ServerError,  T data = default)
        {
            var response = new DouyinApiResponse<T>
            {
                Code = businessCode,
                Data = data,
                Message = message,
                Error = message,
                Msg = message,
            };
            return new OkObjectResult(response);
        }

        /// <summary>
        /// 失败响应（无附加数据）
        /// </summary>
        /// <param name="message">错误消息</param>
        /// <param name="businessCode">自定义业务码</param>
        /// <returns>IActionResult</returns>
        public static IActionResult Fail(string message, int businessCode = ResponseCode.ServerError)
        {
            return Fail<object>(message, businessCode);
        }
        /// <summary>
        /// 失败响应（无附加数据）
        /// </summary>
        /// <param name="message">错误消息</param>
        /// <returns>IActionResult</returns>
        public static IActionResult Fail(string message="请求失败")
        {
            return Fail<object>(message, ResponseCode.ServerError);
        }
        #endregion
    }

}
