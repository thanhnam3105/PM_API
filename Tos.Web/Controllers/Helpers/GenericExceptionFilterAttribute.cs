/** 最終更新日 : 2016-10-17 **/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Filters;

namespace Tos.Web.Controllers.Helpers
{
    /// <summary>
    /// 汎用のExceptionFilterAttributeを定義します。
    /// </summary>
    /// <remarks>
    /// コンストラクタで指定された例外クラスのタイプで例外処理を実行します。
    /// </remarks>
    public abstract class GenericExceptionFilterAttribute : ExceptionFilterAttribute
    {
        public GenericExceptionFilterAttribute(Type targetType)
        {
            if (targetType != null && typeof(Exception).IsAssignableFrom(targetType))
            {
                throw new ArgumentOutOfRangeException();
            }
            if (targetType == null)
            {
                targetType = typeof(Exception);
            }
            this.TargetType = targetType;
        }

        public GenericExceptionFilterAttribute()
        {
            this.TargetType = typeof(Exception);
        }

        public Type TargetType { get; private set; }

        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            if (actionExecutedContext.Exception != null &&
                 this.TargetType.IsAssignableFrom(actionExecutedContext.Exception.GetType()))
            {
                this.HandleException(actionExecutedContext);
            }
        }

        protected abstract void HandleException(HttpActionExecutedContext actionExecutedContext);
    }
}
