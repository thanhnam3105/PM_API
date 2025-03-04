using System;

namespace TOS.Web.Utilities
{
    /// <summary>
    /// ログ出力を定義します。
    /// </summary>
    public sealed class Logger
    {
        private static Logger appLogger = new Logger("application");
        private static Logger auditLogger = new Logger("audit");
        private NLog.Logger _logger;

        /// <summary>
        /// アプリケーションログを出力するためのインスタンスを取得します。
        /// </summary>
        public static Logger App
        {
            get { return appLogger; }
        }

        /// <summary>
        /// 監査ログを出力するためのインスタンスを取得します。
        /// </summary>
        public static Logger Audit
        {
            get { return auditLogger; }
        }

        //TODO: ログの種類を増やす場合は、上記の内容をもとにここに追加します。
        //例
        //private static Logger perfLogger = new Logger("performance");
        //public static Logger Performance
        //{
        //    get { return perfLogger; }
        //}


        private Logger(string name)
        {
            this.Name = name;
            this._logger = NLog.LogManager.GetLogger(name);
        }

        private void WriteEventLogWithLogLevel(NLog.LogLevel level, string message, Exception exception)
        {
            NLog.LogEventInfo logEventInfo = new NLog.LogEventInfo(level, this.Name, null, message, null, exception);
            _logger.Log(this.GetType(), logEventInfo);
        }

        /// <summary>
        /// ログ出力定義の名称を取得します。
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Info レベルでログを出力します。
        /// </summary>
        /// <param name="message">出力するメッセージ</param>
        /// <param name="exception">出力する例外インスタンス</param>
        public void Info(string message, Exception exception = null)
        {
            WriteEventLogWithLogLevel(NLog.LogLevel.Info, message, exception);
        }

        /// <summary>
        /// Debug レベルでログを出力します。
        /// </summary>
        /// <param name="message">出力するメッセージ</param>
        /// <param name="exception">出力する例外インスタンス</param>
        public void Debug(string message, Exception exception = null)
        {
            WriteEventLogWithLogLevel(NLog.LogLevel.Debug, message, exception);
        }

        /// <summary>
        /// Trace レベルでログを出力します。
        /// </summary>
        /// <param name="message">出力するメッセージ</param>
        /// <param name="exception">出力する例外インスタンス</param>
        public void Trace(string message, Exception exception = null)
        {
            WriteEventLogWithLogLevel(NLog.LogLevel.Trace, message, exception);
        }

        /// <summary>
        /// Warn レベルでログを出力します。
        /// </summary>
        /// <param name="message">出力するメッセージ</param>
        /// <param name="exception">出力する例外インスタンス</param>
        public void Warn(string message, Exception exception = null)
        {
            WriteEventLogWithLogLevel(NLog.LogLevel.Warn, message, exception);
        }

        /// <summary>
        /// Error レベルでログを出力します。
        /// </summary>
        /// <param name="message">出力するメッセージ</param>
        /// <param name="exception">出力する例外インスタンス</param>
        public void Error(string message, Exception exception = null)
        {
            WriteEventLogWithLogLevel(NLog.LogLevel.Error, message, exception);
        }

        /// <summary>
        /// Fatal レベルでログを出力します。
        /// </summary>
        /// <param name="message">出力するメッセージ</param>
        /// <param name="exception">出力する例外インスタンス</param>
        public void Fatal(string message, Exception exception = null)
        {
            WriteEventLogWithLogLevel(NLog.LogLevel.Fatal, message, exception);
        }

    }
}