/** 最終更新日 : 2017-09-07 **/
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Security.Claims;

namespace Tos.Web.Utilities
{
    /// <summary>
    /// エンティティの変更セットを定義します。
    /// </summary>
    /// <typeparam name="T">変更セットの対象となるエンティティを定義します。</typeparam>
    public class ChangeSet<T>
    {
        /// <summary>
        /// 変更セットを定義する <see cref="ChangeSet"/> クラスのインスタンスを初期化します。
        /// </summary>
        public ChangeSet()
        {
            this.Created = new List<T>();
            this.Updated = new List<T>();
            this.Deleted = new List<T>();
        }

        /// <summary>
        /// 追加されたエンティティのリストを取得、または設定します。
        /// </summary>
        public List<T> Created { get; set; }

        /// <summary>
        /// 変更されたエンティティのリストを取得、または設定します。
        /// </summary>
        public List<T> Updated { get; set; }

        /// <summary>
        /// 削除されたエンティティのリストを取得、または設定します。
        /// </summary>
        public List<T> Deleted { get; set; }

    }

    public static class ChangeSetExtensions {

        /// <summary>
        /// ChangeSet<T> の値をもとに DbContext に値を追加します。
        /// </summary>
        /// <typeparam name="T">変更セットが保持するエンティティ型</typeparam>
        /// <param name="self">対象とするチェンジセット</param>
        /// <param name="context">値を設定する DbContext</param>
        /// <returns>CreatedまたはUpdatedとマークされていたエンティティのリスト</returns>
        public static IEnumerable<T> AttachTo<T>(this ChangeSet<T> self, DbContext context) where T : class {

            var set = context.Set<T>();
            var results = new List<T>();
            if (self.Created != null) {
                foreach (var created in self.Created) {
                    set.Add(created);
                    results.Add(created);
                }
            }

            if (self.Updated != null) {
                foreach (var updated in self.Updated) {
                    set.Attach(updated);
                    context.Entry<T>(updated).State = EntityState.Modified;
                    results.Add(updated);
                }
            }

            if (self.Deleted != null) {
                foreach (var deleted in self.Deleted) {
                    set.Attach(deleted);
                    set.Remove(deleted);
                }
            }
            return results;

        }


        /// <summary>
        /// ChangeSet<T> からエンティティ一覧を取得します
        /// </summary>
        /// <typeparam name="T">エンティティの型</typeparam>
        /// <param name="self">ChangeSet</param>
        /// <param name="includeDeleted">結果セットに削除対象のエンティティを含むかを設定します
        ///     true    : 削除対象のエンティティを含む
        ///     false   : 削除対象のエンティティを含まない
        ///     規定値  ： false
        /// </param>
        /// <returns>エンティティ一覧</returns>
        public static IEnumerable<T> Flatten<T>(this ChangeSet<T> self, bool includeDeleted = false)
        {
            var result = new List<T>();

            result.AddRange(self.Created);
            result.AddRange(self.Updated);

            if (includeDeleted)
            {
                result.AddRange(self.Deleted);
            }

            return result;
        }

        /// <summary>
        /// テーブルの共通項目を設定します
        /// </summary>
        /// <typeparam name="T">エンティティの型</typeparam>
        /// <param name="changeSet">changeSet</param>
        /// <param name="identity">ユーザー情報</param>
        public static void SetDataSaveInfo<T>(this ChangeSet<T> changeSet, ClaimsIdentity identity)
        {
            //TODO: cd_create/cd_updateが文字型の場合、下記ロジックを採用する
            string userName = identity.Name;
            //TODO: cd_create/cd_updateが数値型の場合、下記ロジックを採用する
            //decimal userName = 0;
            //Decimal.TryParse(UserInfo.GetUserNameFromIdentity(identity), out userName);
            DateTime date = DateTime.Now;

            foreach (dynamic value in changeSet.Created)
            {
                // TODO: プロジェクトで利用するテーブルの共通項目に応じて変更します
                value.cd_create = userName;
                value.dt_create = date;

                value.cd_update = userName;
                value.dt_update = date;
            }
            foreach (dynamic value in changeSet.Updated)
            {
                // TODO: プロジェクトで利用するテーブルの共通項目に応じて変更します
                value.cd_update = userName;
                value.dt_update = date;
            }
        }
    }
}
