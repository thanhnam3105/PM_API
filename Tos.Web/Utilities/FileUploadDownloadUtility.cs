/** 最終更新日 : 2016-10-17 **/
using System;
using System.Collections.Generic;
using System.Web;
using System.Net.Http;
using System.Net;
using System.Text;
using System.IO;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Threading;
using System.Web.Http;

namespace Tos.Web.Controllers.Helpers
{
    /// <summary>
    /// クライアント側にHttpResposeMessageを返すためのユーティリティクラスです。
    /// </summary>
    public static class FileUploadDownloadUtility
    {
        //private static readonly string RequestClientKeyName = Properties.Resources.RequestClientKeyName;
        //private static readonly string ResponseHtmlFormat = Properties.Resources.ResponseHtmlFormat;

        /// <summary>
        /// 指定されたストリームをファイルとしてダウンロードするためのレスポンスを生成します。
        /// </summary>
        /// <param name="responseStream">コンテンツストリーム</param>
        /// <param name="fileName">ファイル名</param>
        /// <returns>レスポンス</returns>
        public static HttpResponseMessage CreateFileResponse(Stream responseStream, string fileName)
        {
            return CreateFileResponse(HttpStatusCode.OK, responseStream, fileName);
        }

        /// <summary>
        /// 指定されたストリームをファイルとしてダウンロードするためのレスポンスを生成します。
        /// </summary>
        /// <param name="responseStream">コンテンツストリーム</param>
        /// <param name="fileName">ファイル名</param>
        /// <returns>レスポンス</returns>
        public static HttpResponseMessage CreateFileResponse(HttpStatusCode satusCode, Stream responseStream, string fileName, string count = null)
        {
            HttpResponseMessage result = new HttpResponseMessage();
            result.StatusCode = satusCode;
            responseStream.Position = 0;
            result.Content = new StreamContent(responseStream);
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
            result.Content.Headers.ContentDisposition.FileName = HttpUtility.UrlEncode(fileName);
            result.Content.Headers.Add("Access-Control-Expose-Headers", "Content-Disposition");

            NameValueHeaderValue headerValue = new NameValueHeaderValue("msg");
            headerValue.Value = count;
            result.Content.Headers.ContentDisposition.Parameters.Add(headerValue);

            result.Headers.CacheControl = new CacheControlHeaderValue { Private = true, MaxAge = TimeSpan.Zero };

            return result;
        }

        /// <summary>
        /// 文字列を数値実体参照文字列に変換します。
        /// </summary>
        /// <param name="value">変換する文字列</param>
        /// <returns>変換された数値実体参照文字列</returns>
        private static string ToNumericCharacterReference(string value)
        {
            StringBuilder result = new StringBuilder();
            foreach (int v in value)
            {
                result.Append("&#x" + v.ToString("x") + ";");
            }
            return result.ToString();
        }

        /// <summary>
        /// マルチパートでアップロードされたファイルをローカルの一時フォルダに格納します。
        /// </summary>
        /// <param name="request">アップロードでクライアントから送信された HttpRequestMessage </param>
        /// <param name="path">一時フォルダのパス</param>
        /// <returns>MultipartFormDataStreamProvider のインスタンス</returns>
        public static MultipartFormDataStreamProvider ReadAsMultiPart(HttpRequestMessage request, string path)
        {

            if (!request.Content.IsMimeMultipartContent("form-data"))
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            MultipartFormDataStreamProvider streamProvider = new MultipartFormDataStreamProvider(path);

            Task.Factory.StartNew(() => streamProvider = request.Content.ReadAsMultipartAsync(streamProvider).Result,
                                  CancellationToken.None,
                                  TaskCreationOptions.LongRunning,
                                  TaskScheduler.Default)
                                  .Wait();

            return streamProvider;
        }

        /// <summary>
        /// 一時フォルダから一致する名称を含むファイルを削除する
        /// </summary>
        /// <param name="dirDownload">一時フォルダのパス</param>
        /// <param name="name">検索する文字列（ファイル名）</param>
        /// <returns>MultipartFormDataStreamProvider のインスタンス</returns>
        public static void deleteTempFile(string dirDownload, string name)
        {
            // 既存ファイル削除（前回までの一時保存フォルダを削除）
            string[] files = Directory.GetFiles(dirDownload);
            foreach (string s in files)
            {
                // ファイル名取得
                String[] strAry = s.Split('\\');
                String fileName = strAry[strAry.Length - 1];

                // 検索する文字列が含まれるものを削除
                if (fileName.IndexOf(name) >= 0)
                {
                    FileInfo cFileInfo = new FileInfo(s);

                    // 読み取り専用属性がある場合は、読み取り専用属性を解除
                    if ((cFileInfo.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                    {
                        cFileInfo.Attributes = FileAttributes.Normal;
                    }

                    // ファイルを削除する
                    cFileInfo.Delete();
                }
            }
        }
    }
}
