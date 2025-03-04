using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TOS.Web.Controllers.Helpers
{
    /// <summary>
    /// SQL Server データベースでエラーが発生した場合のエラーコードを定義します。
    /// </summary>
    public static class SqlErrorNumbers
    {
        /// <summary>
        /// 一意キー制約違反が発生した場合のエラーコードを定義します。
        /// </summary>
        public const int PrimaryKeyViolation = 2627;

        /// <summary>
        /// NOT NULL 制約違反が発生した場合のエラーコードを定義します。
        /// </summary>
        public const int NotNullAllow = 515;
    }
}