using System.Net;

namespace TOS.Web.Utilities
{
    /// <summary>
    /// Parent api result
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ApiResult<T>
    {
        public bool IsSuccessed { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public string Message { get; set; }
        public T Items { get; set; }
    }

    /// <summary>
    /// Success api result
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ApiSuccessResult<T> : ApiResult<T>
    {
        public ApiSuccessResult()
        {
            IsSuccessed = true;
            StatusCode = HttpStatusCode.OK;
        }

        public ApiSuccessResult(T items)
        {
            IsSuccessed = true;
            StatusCode = HttpStatusCode.OK;
            Items = items;
        }
    }

    /// <summary>
    /// Error api result
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ApiErrorResult<T> : ApiResult<T>
    {
        public string[] ValidationErrors { get; set; }

        public ApiErrorResult()
        {
            IsSuccessed = false;
            StatusCode = HttpStatusCode.BadRequest;
        }

        public ApiErrorResult(string message)
        {
            IsSuccessed = false;
            StatusCode = HttpStatusCode.BadRequest;
            Message = message;
        }

        public ApiErrorResult(HttpStatusCode statusCode, string message)
        {
            IsSuccessed = false;
            StatusCode = statusCode;
            Message = message;
        }

        public ApiErrorResult(string[] validationErrors)
        {
            IsSuccessed = false;
            StatusCode = HttpStatusCode.BadRequest;
            ValidationErrors = validationErrors;
        }

        public ApiErrorResult(HttpStatusCode statusCode, string[] validationErrors)
        {
            IsSuccessed = false;
            StatusCode = statusCode;
            ValidationErrors = validationErrors;
        }
    }
}