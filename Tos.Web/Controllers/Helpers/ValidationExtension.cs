/** 最終更新日 : 2018-08-24 **/
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace TOS.Web.Controllers.Helpers
{
    /// <summary>
    /// バリデーションの拡張を定義します
    /// </summary>
    public static class ValidationExtension
    {
        /// <summary>
        /// 必須項目チェックを実施します
        /// </summary>
        /// <param name="value">検証値</param>
        /// <returns>検証結果</returns>
        public static ValidationResult Required(this object value)
        {
            ValidationResult result = new ValidationResult();

            if (value == null || string.IsNullOrEmpty(value.ToString()))
            {
                //result.Errors.Add(Properties.Resources.RequiredTextField);
            }

            return result;
        }

        /// <summary>
        /// バイト長チェックを実施します
        /// </summary>
        /// <param name="value">検証値</param>
        /// <param name="length">最大値</param>
        /// <returns>検証結果</returns>
        public static ValidationResult MaxByteLength(this string value, int length)
        {
            ValidationResult result = new ValidationResult();

            if (string.IsNullOrEmpty(value))
            {
                return result;
            }

            //if (Encoding.GetEncoding(Properties.Resources.Encoding).GetByteCount(value) > length)
            //{
            //    result.Errors.Add(string.Format(Properties.Resources.OutOfLengthTextField, length, length / 2));
            //}

            return result;
        }

        /// <summary>
        /// 値範囲チェックを実施します
        /// </summary>
        /// <param name="value">検証値</param>
        /// <param name="min">最小値</param>
        /// <param name="max">最大値</param>
        /// <returns>検証結果</returns>
        public static ValidationResult Range(this string value, double? min, double? max)
        {
            ValidationResult result = new ValidationResult();

            double doubleValue;
            bool isNumeric = double.TryParse(value, System.Globalization.NumberStyles.Any, null, out doubleValue);

            if (!isNumeric)
            {
                return result;
            }

            if (max < doubleValue || doubleValue < min)
            {
                //result.Errors.Add(string.Format(Properties.Resources.OutOfRangeTextField, min, max));
            }

            return result;
        }

        /// <summary>
        /// 数値チェックを実施します
        /// </summary>
        /// <param name="value">検証値</param>
        /// <param name="allowNegativeNumber">負の数の許可</param>
        /// <returns>検証結果</returns>
        public static ValidationResult Number(this string value, bool allowNegativeNumber)
        {
            ValidationResult result = new ValidationResult();

            if (string.IsNullOrEmpty(value))
            {
                return result;
            }

            if (!value.IsNumeric(allowNegativeNumber))
            {
                //result.Errors.Add(Properties.Resources.InvalidNumberFormatError);
            }
            return result;
        }

        /// <summary>
        /// 整数チェックを実施します
        /// </summary>
        /// <param name="value">検証値</param>
        /// <param name="allowNegativeNumber">負の数の許可</param>
        /// <returns>検証結果</returns>
        public static ValidationResult Integer(this string value, bool allowNegativeNumber)
        {
            ValidationResult result = new ValidationResult();

            if (string.IsNullOrEmpty(value))
            {
                return result;
            }

            if (!value.IsNumeric(allowNegativeNumber))
            {
                if (allowNegativeNumber)
                {
                    //result.Errors.Add(Properties.Resources.InvalidNumberFormatError);
                    return result;
                }
                else
                {
                    //result.Errors.Add(Properties.Resources.InvalidDigitsFormatError);
                    return result;
                }
            }

            long number;
            if (!long.TryParse(value, out number))
            {
                //result.Errors.Add(Properties.Resources.InvalidIntegerFormatError);
                return result;
            }

            return result;
        }

        /// <summary>
        /// 小数フォーマットチェックを実施します
        /// </summary>
        /// <param name="value">検証値</param>
        /// <param name="beforePoint">整数部桁数</param>
        /// <param name="afterPoint">小数部桁数</param>
        /// <param name="allowNegativeNumber">負の数の許可</param>
        /// <returns>検証結果</returns>
        public static ValidationResult PointLength(this string value, int beforePoint, int afterPoint, bool allowNegativeNumber)
        {
            ValidationResult result = new ValidationResult();

            if (string.IsNullOrEmpty(value))
            {
                return result;
            }

            if (!value.IsNumeric(allowNegativeNumber))
            {
                //result.Errors.Add(Properties.Resources.InvalidIntegerFormatError);
                return result;
            }

            string[] array = Decimal.Parse(value).ToString().Replace("-", "").Split('.');
            if (array[0].Length > beforePoint)
            {
                //result.Errors.Add(string.Format(Properties.Resources.InvalidPointLengthFormatError, beforePoint, afterPoint));
                return result;
            }
            if (array.Length > 1 && array[1].Length > afterPoint)
            {
                //result.Errors.Add(string.Format(Properties.Resources.InvalidPointLengthFormatError, beforePoint, afterPoint));
                return result;
            }

            return result;
        }

        /// <summary>
        /// 日付チェックを実施します
        /// </summary>
        /// <param name="value">検証値</param>
        /// <param name="format">変換フォーマット</param>
        /// <returns>検証結果</returns>
        public static ValidationResult DateFormat(this string value, string format)
        {
            ValidationResult result = new ValidationResult();

            if (string.IsNullOrEmpty(value))
            {
                return result;
            }

            if (!IsDateFormatyyyyMMdd(value, format))
            {
                //result.Errors.Add(string.Format(Properties.Resources.InvalidDateFormatError, format));
            }
            return result;
        }
 
        /// <summary>
        /// ビット値チェックを実施します
        /// </summary>
        /// <param name="value">検証値</param>
        /// <returns>検証結果</returns>
        public static ValidationResult IsBit(this string value)
        {
            ValidationResult result = new ValidationResult();

            if (string.IsNullOrEmpty(value))
            {
                return result;
            }

            string[] bitStrings = { "0", "1", "TRUE", "FALSE" };
            if (!bitStrings.Contains(value.ToUpper().Trim()))
            {
                //result.Errors.Add(Properties.Resources.InvalidBitFormatError);
            }

            return result;
        }

        /// <summary>
        /// 全角チェックを実施します
        /// </summary>
        /// <param name="value">検証値</param>
        /// <returns>検証結果</returns>
        public static ValidationResult IsZenkaku(this string value)
        {
            ValidationResult result = new ValidationResult();

            if (string.IsNullOrEmpty(value))
            {
                return result;
            }
            if (!Regex.IsMatch(value, "^[^\u0020-\u007E\uFF61-\uFF9F]+$"))
            {
                //result.Errors.Add(Properties.Resources.InvalidZenkakuError);
            }

            return result;
        }

        /// <summary>
        /// 半角チェックを実施します
        /// </summary>
        /// <param name="value">検証値</param>
        /// <returns>検証結果</returns>
        public static ValidationResult IsHankaku(this string value)
        {
            ValidationResult result = new ValidationResult();

            if (string.IsNullOrEmpty(value))
            {
                return result;
            }
            if (!Regex.IsMatch(value, "^[\u0020-\u007E\uFF61-\uFF9F]+$"))
            {
                //result.Errors.Add(Properties.Resources.InvalidHankakuError);
            }

            return result;
        }

        /// <summary>
        /// 半角アルファベットチェックを実施します
        /// </summary>
        /// <param name="value">検証値</param>
        /// <returns>検証結果</returns>
        public static ValidationResult IsAlphabet(this string value)
        {
            ValidationResult result = new ValidationResult();

            if (string.IsNullOrEmpty(value))
            {
                return result;
            }
            if (!Regex.IsMatch(value, "^[\u0020\u0041-\u005A\u0061-\u007A]+$"))
            {
                //result.Errors.Add(Properties.Resources.InvalidAlphabetError);
            }

            return result;
        }

        /// <summary>
        /// 半角アルファベット・数値チェックを実施します
        /// </summary>
        /// <param name="value">検証値</param>
        /// <returns>検証結果</returns>
        public static ValidationResult IsAlphabetNumber(this string value)
        {
            ValidationResult result = new ValidationResult();

            if (string.IsNullOrEmpty(value))
            {
                return result;
            }
            if (!Regex.IsMatch(value, "^[\u0020\u0041-\u005A\u0030-\u0039\u0061-\u007A]+$"))
            {
                //result.Errors.Add(Properties.Resources.InvalidAlphabetNumberError);
            }

            return result;
        }

        /// <summary>
        /// 半角アルファベット・数字・記号チェックを実施します
        /// </summary>
        /// <param name="value">検証値</param>
        /// <returns>検証結果</returns>
        public static ValidationResult IsHankakuEiSuKigo(this string value)
        {
            ValidationResult result = new ValidationResult();

            if (string.IsNullOrEmpty(value))
            {
                return result;
            }
            if (!Regex.IsMatch(value, "^[\u0020-\u007E]+$"))
            {
                //result.Errors.Add(Properties.Resources.InvalidHankakuEiSuKigoError);
            }

            return result;
        }

        /// <summary>
        /// 半角カナチェックを実施します
        /// </summary>
        /// <param name="value">検証値</param>
        /// <returns>検証結果</returns>
        public static ValidationResult IsHankakuKana(this string value)
        {
            ValidationResult result = new ValidationResult();

            if (string.IsNullOrEmpty(value))
            {
                return result;
            }
            if (!Regex.IsMatch(value, "^[\u0020\uFF61-\uFF9F]+$"))
            {
                //result.Errors.Add(Properties.Resources.InvalidHankakuKanaError);
            }

            return result;
        }

        private static bool IsDateFormatyyyyMMdd(this string str, string format)
        {
            //0または0埋めの場合は日付と見なす
            if (!Regex.IsMatch(str, "[^0]"))
            {
                return true;
            }

            DateTime dmy;
            if (DateTime.TryParseExact(str, format, null, DateTimeStyles.None, out dmy))
            {
                return true;
            }
            return false;
        }
        
        public static bool IsNumeric(this string value, bool allowNegativeNumber)
        {
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            if (Regex.IsMatch(value, "[a-zA-Z]+"))
            {
                return false;
            }
            Decimal number;
            if (Decimal.TryParse(value, out number))
            {
                if (!allowNegativeNumber)
                {
                    return number >= 0;
                }
                return true;
            }
            return false;
        }

        public static bool IsNumeric(this string value)
        {
            return IsNumeric(value, true);
        }

    }
}
