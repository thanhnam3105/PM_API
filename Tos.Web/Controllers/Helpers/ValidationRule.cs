/** 最終更新日 : 2018-08-24 **/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static TOS.Web.Controllers.Helpers.Contants;

namespace TOS.Web.Controllers.Helpers
{
    /// <summary>
    /// バリデーションの定義情報を格納します
    /// </summary>
    public class ValidationRule
    {
        /// <summary>
        /// 指定されたカラム定義に従って検証ルールを生成します
        /// </summary>
        /// <param name="columnDef">カラム定義</param>
        /// <param name="target">検証対象値</param>
        public ValidationRule(Dictionary<string, object> ruleTypes, object target)
        {
            foreach (var ruleType in ruleTypes)
            {
                if (ruleType.Key == ValidationRuleTypes.Required)
                {
                    rules.Add(() => target.Required());
                }
                else if (ruleType.Key == ValidationRuleTypes.MaxLength)
                {
                    string value = target.GetType() == typeof(System.DBNull) ? target.ToString() : (string)target;
                    int length = int.Parse(ruleType.Value.ToString());
                    rules.Add(() => value.MaxByteLength(length));
                }
                else if (ruleType.Key == ValidationRuleTypes.Range)
                {
                    string value = target.GetType() == typeof(System.DBNull) ? target.ToString() : (string)target;
                    TextFieldSetting.Range range = (TextFieldSetting.Range)ruleType.Value;
                    rules.Add(() => value.Range(range.Min, range.Max));
                }
                else if (ruleType.Key == ValidationRuleTypes.Number)
                {
                    string value = target.GetType() == typeof(System.DBNull) ? target.ToString() : (string)target;
                    bool allowNegativeNumber = bool.Parse(ruleType.Value.ToString());
                    rules.Add(() => value.Number(allowNegativeNumber));
                }
                else if (ruleType.Key == ValidationRuleTypes.Integer)
                {
                    string value = target.GetType() == typeof(System.DBNull) ? target.ToString() : (string)target;
                    bool allowNegativeNumber = bool.Parse(ruleType.Value.ToString());
                    rules.Add(() => value.Integer(allowNegativeNumber));
                }
                else if (ruleType.Key == ValidationRuleTypes.PointLength)
                {
                    string value = target.GetType() == typeof(System.DBNull) ? target.ToString() : (string)target;
                    TextFieldSetting.PointLength pointcheck = (TextFieldSetting.PointLength)ruleType.Value;
                    rules.Add(() => value.PointLength(pointcheck.BeforePoint, pointcheck.AfterPoint, pointcheck.AllowNegativeNumber));
                }
                else if (ruleType.Key == ValidationRuleTypes.Date)
                {
                    string value = target.GetType() == typeof(System.DBNull) ? target.ToString() : (string)target;
                    string format = ruleType.Value.ToString();
                    rules.Add(() => value.DateFormat(format));
                }
                else if (ruleType.Key == ValidationRuleTypes.Boolean)
                {
                    string value = target.GetType() == typeof(System.DBNull) ? target.ToString() : (string)target;
                    rules.Add(() => value.ToString().IsBit());
                }
                else if (ruleType.Key == ValidationRuleTypes.Zenkaku)
                {
                    string value = target.GetType() == typeof(System.DBNull) ? target.ToString() : (string)target;
                    rules.Add(() => value.ToString().IsZenkaku());
                }
                else if (ruleType.Key == ValidationRuleTypes.Hankaku)
                {
                    string value = target.GetType() == typeof(System.DBNull) ? target.ToString() : (string)target;
                    rules.Add(() => value.ToString().IsHankaku());
                }
                else if (ruleType.Key == ValidationRuleTypes.Alphabet)
                {
                    string value = target.GetType() == typeof(System.DBNull) ? target.ToString() : (string)target;
                    rules.Add(() => value.ToString().IsAlphabet());
                }
                else if (ruleType.Key == ValidationRuleTypes.Alphanum)
                {
                    string value = target.GetType() == typeof(System.DBNull) ? target.ToString() : (string)target;
                    rules.Add(() => value.ToString().IsAlphabetNumber());
                }
                else if (ruleType.Key == ValidationRuleTypes.Haneisukigo)
                {
                    string value = target.GetType() == typeof(System.DBNull) ? target.ToString() : (string)target;
                    rules.Add(() => value.ToString().IsHankakuEiSuKigo());
                }
                else if (ruleType.Key == ValidationRuleTypes.Hankana)
                {
                    string value = target.GetType() == typeof(System.DBNull) ? target.ToString() : (string)target;
                    rules.Add(() => value.ToString().IsHankakuKana());
                }
                //TODO: バリデーションの種別を増やす場合はここに処理を追加
            }
        }

        private List<Func<ValidationResult>> rules = new List<Func<ValidationResult>>();

        /// <summary>
        /// バリデーション定義を追加します
        /// </summary>
        /// <param name="rule">バリデーション定義</param>
        public void Add(Func<ValidationResult> rule)
        {
            this.rules.Add(rule);
        }

        /// <summary>
        /// バリデーション処理を実行します
        /// </summary>
        /// <returns>バリデーション結果</returns>
        public ValidationResult Validate()
        {
            ValidationResult result = new ValidationResult();

            foreach (var rule in rules)
            {
                var ruleResult = rule.Invoke();

                result.Errors.AddRange(ruleResult.Errors);
            }
            return result;
        }
    }

    /// <summary>
    /// 検証結果としてエラー情報を格納します
    /// </summary>
    public class ValidationResult
    {
        private List<string> errors = new List<string>();

        /// <summary>
        /// 検証結果としてエラーが発生しているかどうかを取得します
        /// </summary>
        public bool IsValid
        {
            get { return errors.Count == 0; }
        }
        /// <summary>
        /// エラー情報を取得します
        /// </summary>
        public List<string> Errors
        {
            get { return errors; }
        }
    }

}
