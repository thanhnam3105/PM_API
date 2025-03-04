/** 最終更新日 : 2018-08-24 **/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.VisualBasic.FileIO;
using System.Text.RegularExpressions;
using System.Data.Entity.Validation;
using System.Globalization;
using TOS.Web.Properties;
using static TOS.Web.Controllers.Helpers.Contants;

namespace TOS.Web.Controllers.Helpers
{

    /// <summary>
    /// CSV / TSV などのファイルのカラム設定を保持します。
    /// </summary>
    public class TextFieldSetting
    {
        /// <summary>
        /// 範囲を表すオブジェクトを定義します。
        /// </summary>
        public class Range 
        {
            /// <summary>
            /// 最小値を取得または設定します。
            /// </summary>
            public double? Min { get; set; }

            /// <summary>
            /// 最大値を取得または設定します。
            /// </summary>
            public double? Max { get; set; }
        }

        /// <summary>
        /// 小数値フォーマットを表すオブジェクトを定義します。
        /// </summary>
        public class PointLength
        {
            /// <summary>
            /// 整数部桁数を取得または設定します。
            /// </summary>
            public int BeforePoint { get; set; }

            /// <summary>
            /// 小数部桁数を取得または設定します。
            /// </summary>
            public int AfterPoint { get; set; }

            /// <summary>
            /// 負の数の許可を取得または設定します。
            /// </summary>
            public bool AllowNegativeNumber { get; set; }
        }

        /// <summary>
        /// カラムの対象となるプロパティ名を取得または設定します。
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        /// カラムの表示名を取得または設定します。
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// 変換時のフォーマットを取得または設定します。
        /// </summary>
        public string Format { get; set; }

        /// <summary>
        /// 正規表現検証のエラーメッセージを取得または設定します。
        /// </summary>
        public string RegexErrorMessage { get; set; }

        /// <summary>
        /// 正規表現検証で利用する正規表現文字列を取得または設定します。
        /// </summary>
        public string RegexValidation { get; set; }

        /// <summary>
        /// 必須のフィールドかどうかを取得または設定します。
        /// </summary>
        public bool Required { get; set; }

        /// <summary>
        /// 値の範囲を取得または設定します。
        /// </summary>
        public Range ValueRange { get; set; }

        /// <summary>
        /// プロパティのバイト長を取得または設定します。
        /// </summary>
        public int Length { get; set; }

        /// <summary>
        /// 値のくくり文字を設定します。
        /// </summary>
        public string WrapChar { get; set; }

        /// <summary>
        /// バリデーションルールを設定します。
        /// </summary>
        public Dictionary<string, object> ValidateRules { get; set; }

        public Dictionary<string, object> Rules
        {
            get
            {
                Dictionary<string, object> rules;
                if (this.ValidateRules != null)
                {
                    rules = this.ValidateRules;
                }
                else
                {
                    rules = new Dictionary<string, object>();
                }

                if (this.Required) 
                {
                    rules[ValidationRuleTypes.Required] = true;
                }
                if (this.Length > 0) 
                {
                    rules[ValidationRuleTypes.MaxLength] = this.Length;
                }
                if (this.ValueRange != null)
                {
                    rules[ValidationRuleTypes.Range] = this.ValueRange;
                }

                return rules;
            }
        }
    }

    /// <summary>
    /// レコード読み取り時のエラー情報を格納します。
    /// </summary>
    public class TextFieldError
    {
        /// <summary>
        /// レコードの行番号
        /// </summary>
        public long RecordNumber { get; set; }

        /// <summary>
        /// レコードの列名
        /// </summary>
        public string ColumnName { get; set; }

        /// <summary>
        /// エラーが発生した値
        /// </summary>
        public string ErrorValue { get; set; }

        /// <summary>
        /// エラーメッセージ
        /// </summary>
        public string ErrorMessage { get; set; }
    }

    /// <summary>
    /// 特定のオブジェクトのテキストファイルの出力、読み込みを行います。
    /// </summary>
    /// <typeparam name="T">テキストファイルの出力、読み込みの対象となるクラスを指定します。</typeparam>
    public class TextFieldFile<T> : IDisposable where T : new()
    {

        private Stream stream;
        private TextWriter writer;
        private TextFieldParser parser;
        private Encoding encoding = Encoding.UTF8;
        private bool disposed = false;
        private TextFieldSetting[] settings;
        private long recordCount = 0;

        /// <summary>
        /// TextFieldFile クラスのインスタンスを初期化します。
        /// </summary>
        /// <param name="stream">読み込み / 書き込みを行うストリーム</param>
        /// <param name="encoding">読み込み、書き込み時の文字エンコーディング</param>
        /// <param name="settings">カラム設定</param>
        public TextFieldFile(Stream stream, Encoding encoding, params TextFieldSetting[] settings)
        {
            RequiredArgument(stream, "stream");
            RequiredArgument(encoding, "encoding");

            this.stream = stream;
            this.encoding = encoding;
            this.parser = new TextFieldParser(this.stream, this.encoding);
            this.writer = new StreamWriter(this.stream, encoding);
            this.settings = settings;
        }

        /// <summary>
        /// TextFieldFile クラスのインスタンスを初期化します。
        /// </summary>
        /// <param name="path">読み込み / 書き込みを行うファイルのパス</param>
        /// <param name="encoding">読み込み、書き込み時の文字エンコーディング</param>
        /// <param name="settings">カラム設定</param>
        public TextFieldFile(string path, Encoding encoding, params TextFieldSetting[] settings)
            : this(new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read), encoding, settings)
        {
            RequiredArgument(path, path);
        }

        private void RequiredArgument(object target, string argumentName)
        {
            if (target == null || string.IsNullOrEmpty(target.ToString()))
            {
                throw new ArgumentNullException(argumentName);
            }
        }

        /// <summary>
        /// カラムの区切り文字を取得、または設定します。
        /// </summary>
        public string[] Delimiters { set; get; }

        /// <summary>
        /// 1レコード目をヘッダーとして識別するかどうかを取得、または設定します。
        /// </summary>
        public bool IsFirstRowHeader { set; get; }

        /// <summary>
        /// 1カラム目を更新区分として利用するかどうかを取得、または設定します。
        /// </summary>
        public bool IsUseUpdateColumn { get; set; }

        /// <summary>
        /// ファイルの読み込み時にファイルの終端に到達したかどうかを取得します。
        /// </summary>
        public bool EndOfData
        {
            get { return parser.EndOfData; }
        }

        /// <summary>
        /// 読み込んだレコード数を取得します。
        /// </summary>
        public long RecordCount
        {
            get { return this.recordCount; }
        }

        /// <summary>
        /// レコードの読み込み時にエラーが発生しているかどうかを取得します。
        /// </summary>
        public bool HasError
        {
            get { return (this.errors.Count > 0); }
        }

        private List<TextFieldError> errors = new List<TextFieldError>();
//        private string currentRecord;

        /// <summary>
        /// テキストファイルから対象クラスのインスタンスを 1レコード分読み込みます
        /// </summary>
        /// <returns>読み込んだ値が設定された対象クラスのインスタンス</returns>
//        public T ReadFields()
//        {
//            T result = new T();
//            Type resultType = result.GetType();

//            parser.TextFieldType = FieldType.Delimited;
//            parser.SetDelimiters(this.Delimiters);
//            parser.HasFieldsEnclosedInQuotes = true;
//            parser.TrimWhiteSpace = false;

//            if (this.recordCount == 0 && this.IsFirstRowHeader)
//            {
//                parser.ReadLine();
//            }
////            this.currentRecord = null;

//            string[] columns = parser.ReadFields();
//            if (columns == null)
//            {
//                return default(T);
//            }

//            this.recordCount++;

//            if (columns.Length != settings.Length)
//            {
//                AddError(string.Empty, string.Join(Delimiters[0], columns), Properties.Resource.InvalidColumnSizeTextFieldFile);
//                return default(T);
//            }

//            bool hasError = false;
//            if (this.IsUseUpdateColumn)
//            {
//                string updateColumn = columns[0];
//                if (updateColumn == CsvUpdateColumn.NotUpdate)
//                {
//                    return default(T);
//                }
//            }

//            for (int i = 0; i < columns.Length; i++)
//            {
//                string column = columns[i];
//                TextFieldSetting setting = settings[i];

//                //制御文字コードの除去
//                column = Regex.Replace(column, "[\u0000-\u0009\u000b\u000c\u000e-\u001f\u007f]", string.Empty);

//                if (ValidateValue(setting, column))
//                {
//                    try
//                    {
//                        PropertyInfo prop = resultType.GetProperty(setting.PropertyName);
//                        SetValue(prop, result, column, setting.Format);
//                    }
//                    catch (Exception ex)
//                    {
//                        AddError(setting.DisplayName ?? setting.PropertyName, column, ex.Message);
//                        hasError = true;
//                    }
//                }
//                else
//                {
//                    hasError = true;
//                }
//            }

//            if (hasError) {
//                return default(T);
//            }

//            return result;
//        }

        /// <summary>
        /// カラムに対して指定されたバリデーションを実施します
        /// </summary>
        /// <returns>バリデーションエラーの場合はfalse、エラーなしはtrue</returns>
        private bool ValidateValue(TextFieldSetting setting, string value)
        {
            string columnName = setting.DisplayName ?? setting.PropertyName;

            //定型バリデーションチェック
            if (setting.Rules.Count > 0) 
            {
                ValidationRule validation = new ValidationRule(setting.Rules, value);
                ValidationResult result = validation.Validate();
                if (!result.IsValid) 
                {
                    foreach (var errorMessage in result.Errors)
                    {
                        AddError(columnName, value, errorMessage);
                    }
                    return false;
                }
            }

            //正規表現チェック
            if (!string.IsNullOrEmpty(setting.RegexValidation) && !Regex.IsMatch(value, setting.RegexValidation))
            {
                AddError(columnName, value, setting.RegexErrorMessage);
                return false;
            }

            return true;
        }

        /// <summary>
        /// エラー情報を追加します。
        /// </summary>
        /// <param name="column">カラム名</param>
        /// <param name="value">値</param>
        /// <param name="message">エラーメッセージ</param>
        /// <param name="rowNumber">行番号（任意）</param>
        public void AddError(string column, string value, string message, long? rowNumber = null)
        {
            long recordNumber;
            if (rowNumber == null)
            {
                recordNumber = IsFirstRowHeader ? recordCount + 1 : recordCount;
            }
            else
            {
                recordNumber = IsFirstRowHeader ? rowNumber.Value + 1 : rowNumber.Value;
            }

            TextFieldError error = new TextFieldError()
            {
                ColumnName = column,
                ErrorValue = value,
                RecordNumber = recordNumber,
                ErrorMessage = message
            };

            errors.Add(error);
        }

        private static readonly TextFieldSetting[] ErrorFileSettings = new TextFieldSetting[]
        {
            new TextFieldSetting() { PropertyName = "RecordNumber", DisplayName = Properties.Resource.RecordNumber },
            new TextFieldSetting() { PropertyName = "ColumnName", DisplayName = Properties.Resource.ColumnName },
            new TextFieldSetting() { PropertyName = "ErrorMessage", DisplayName = Properties.Resource.ErrorMessage, WrapChar= "\"" },
            new TextFieldSetting() { PropertyName = "ErrorValue", DisplayName = Properties.Resource.ErrorValue, WrapChar= "\"" }
        };

        /// <summary>
        /// エラー情報をストリームに書き込んで取得します。
        /// </summary>
        /// <returns></returns>
        public MemoryStream GetErrorStream()
        {
            MemoryStream stream = new MemoryStream();
            TextWriter writer = new StreamWriter(stream, this.encoding);
            //writer.WriteLine(Properties.Resources.CSVUploadError);
            writer.Flush();
            //TextFieldFile<TextFieldError> tfile = new TextFieldFile<TextFieldError>(stream, Encoding.GetEncoding(Properties.Resources.Encoding), ErrorFileSettings);
            //tfile.Delimiters = new string[] { "," };
            //tfile.IsFirstRowHeader = true;
            //tfile.WriteFields(this.errors);

            return stream;
        }

        /// <summary>
        /// 引数で指定された対象クラスのオブジェクトのシーケンスを書き込みます。
        /// </summary>
        /// <param name="targets">対象クラスのオブジェクトのシーケンス</param>
        public void WriteFields(IEnumerable<T> targets)
        {
            Type targetType = typeof(T);
            string delimiter = string.Join(string.Empty, this.Delimiters);

            if (this.IsFirstRowHeader)
            {
                string[] headers = this.settings.Select(s => s.DisplayName ?? s.PropertyName).ToArray();
                this.writer.WriteLine(string.Join(delimiter, headers));
            }

            foreach (var target in targets)
            {
                List<string> fields = new List<string>();
                foreach (var setting in settings)
                {
                    PropertyInfo prop = targetType.GetProperty(setting.PropertyName);
                    object val = prop.GetValue(target, null);

                    string formatStr = FormatValue(val, prop, setting);  

                    //WrapCharが指定されている場合、項目値内にwrapcharが存在するときは重ねるように変換する
                    string wrap = setting.WrapChar == null ? string.Empty : setting.WrapChar;
                    string valStr = setting.WrapChar == null ? formatStr : formatStr.Replace(wrap, wrap + wrap);
                    
                    fields.Add(string.Format("{0}{1}{2}", wrap, valStr, wrap));
                }

                this.writer.WriteLine(string.Join(delimiter, fields));
            }

            this.writer.Flush();
        }

        /// <summary>
        /// 指定されたフォーマット文字でフォーマットされた文字列を返却します
        /// </summary>
        private string FormatValue(object val, PropertyInfo prop, TextFieldSetting setting)
        {
            string format = setting.Format == null ? string.Empty : setting.Format;
            string formatStr = val == null ? string.Empty : val.ToString();
            Type propType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

            //if (ColumnTypes.BoolTypes.Contains(propType.Name) && format == Properties.Resources.NumberToBoolean)
            if (ColumnTypes.BoolTypes.Contains(propType.Name) && format == "bit")
            {
                bool parseResult;
                if (Boolean.TryParse(formatStr, out parseResult))
                {
                    bool bl = (bool)val;
                    formatStr = Convert.ToInt32(bl).ToString();
                }
            }
            else if (!string.IsNullOrEmpty(format))
            {
                formatStr = string.Format("{0:" + format + "}", val);
            }

            return formatStr;
        }


        /// <summary>
        /// アップロードデータを指定されたフォーマット変換をして取り込む
        /// </summary>
        private void SetValue(PropertyInfo prop, object target, string value, string format)
        {
            Type propType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

            if (format != null)
            {
                if (ColumnTypes.DateTimeOffsetTypes.Equals(propType.Name))
                {
                    DateTimeOffset parseResult;
                    if (DateTimeOffset.TryParseExact(value, format, CultureInfo.CurrentCulture, DateTimeStyles.None, out parseResult))
                    {
                        prop.SetValue(target, parseResult, null);
                        return;
                    }

                }
                else if (ColumnTypes.DateTimeTypes.Equals(propType.Name))
                {
                    DateTime parseResult;
                    if (DateTime.TryParseExact(value, format, CultureInfo.CurrentCulture, DateTimeStyles.None, out parseResult))
                    {
                        prop.SetValue(target, parseResult, null);
                        return;
                    }

                }
                //else if (ColumnTypes.BoolTypes.Contains(propType.Name) && format == Properties.Resources.NumberToBoolean)
                else if (ColumnTypes.BoolTypes.Contains(propType.Name) && format == "bit")
                {
                    if (!string.IsNullOrEmpty(value) && value.IsNumeric())
                    {
                        try
                        {
                            Boolean parseResult = BoolStrings.BoolMembers[value];
                            prop.SetValue(target, parseResult, null);
                            return;
                        } catch (Exception) { }
                    }
                }
            }

            var val = (string.IsNullOrEmpty(value)) ? null : Convert.ChangeType(value, propType);
            prop.SetValue(target, val, null);
        }

        /// <summary>
        /// TextFieldFile オブジェクトのすべてのリソースを破棄します。
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            if (disposing)
            {
                parser.Dispose();
            }

            disposed = true;
        }

        ~TextFieldFile()
        {
            Dispose(false);
        }

    }

    /// <summary>
    /// ファイル書出し時に使用するユーティリティクラスです。
    /// </summary>
    public static class TextFiedlFileUtility {

        /// <summary>
        /// Exceptionからログ表示用エラーメッセージを抽出する
        /// ex: 例外メッセージ
        /// </summary>
        public static List<TextFieldError> GetExceptionMessage(Exception ex)
        {
            List<TextFieldError> errorList = new List<TextFieldError>();

            if (ex == null)
            {
                return errorList;
            }
            else if (ex is DbEntityValidationException)
            {
                foreach (var errors in ((DbEntityValidationException)ex).EntityValidationErrors)
                {
                    foreach (var error in errors.ValidationErrors)
                    {
                        TextFieldError err = new TextFieldError()
                        {
                            ColumnName = error.PropertyName,
                            ErrorValue = string.Empty,
                            ErrorMessage = error.ErrorMessage
                        };
                        errorList.Add(err);
                    }
                }
            }
            else if (ex.InnerException != null)
            {
                return GetExceptionMessage(ex.InnerException);
            }
            else
            {
                TextFieldError err = new TextFieldError()
                {
                    ColumnName = string.Empty,
                    ErrorValue = string.Empty,
                    ErrorMessage = ex.Message
                };
                errorList.Add(err);
            }

            return errorList;
        }

    }

}
