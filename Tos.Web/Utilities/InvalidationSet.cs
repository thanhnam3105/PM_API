/** 最終更新日 : 2016-10-17 **/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Tos.Web.Utilities
{
    /// <summary>
    /// 一連のバリデーションエラーとなったデータおよびメッセージを定義します。
    /// </summary>
    /// <typeparam name="T">対象となるエンティティ</typeparam>
    public class InvalidationSet<T> : List<Invalidation<T>>
    {
    }

    /// <summary>
    /// バリデーションエラーとなったデータおよびメッセージを定義します。
    /// </summary>
    /// <typeparam name="T">対象となるエンティティ</typeparam>
    public class Invalidation<T>
    {

        /// <summary>
        /// インスタンスを初期化します。
        /// </summary>
        /// <param name="message">バリデーションエラーメッセージ</param>
        /// <param name="data">バリデーションエラーとなったデータ</param>
        /// /// <param name="invalidationName">バリデーションエラー名</param>
        public Invalidation(string message, T data, string invalidationName)
        {
            this.Message = message;
            this.Data = data;
            this.InvalidationName = invalidationName;
        }

        /// <summary>
        /// バリデーションエラーメッセージを取得または設定します。
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// バリデーションエラーとなったデータを取得または設定します。
        /// </summary>
        public T Data { get; set; }

        /// <summary>
        /// バリデーションエラー名を取得または設定します。
        /// </summary>
        public string InvalidationName { get; set; }

    }
}
