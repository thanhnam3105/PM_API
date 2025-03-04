/** 最終更新日 : 2016-10-17 **/
using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using System.Globalization;

namespace Tos.Web.Utilities
{

    /// <summary>
    /// オブジェクトのデータを別のオブジェクトにコピーします。
    /// </summary>
    public class DataCopier {


        private DataCopier() {
        }

        /// <summary>
        /// 指定されたオブジェクトと一致するプロパティに同じ値が設定されたオブジェクトを返します。
        /// </summary>
        /// <typeparam name="TDest">値が設定されたオブジェクトを指定する型パラメータ</typeparam>
        /// <param name="source">設定もととなるオブジェクト</param>
        /// <returns>値が設定されたオブジェクト</returns>
        /// <remarks><see cref="TypeCode" /> に設定された DBNull , Empty , Object 以外の型のみ値を設定します。</remarks>
        public static TDest ReFill<TDest>(object source) where TDest : new() {
            return ReFill<TDest>(source, new DataCopierSetting());
        }

        /// <summary>
        /// 指定されたオブジェクトに引数で指定された設定に基づいて同じ値が設定されたオブジェクトを返します。
        /// </summary>
        /// <typeparam name="TDest">値が設定されたオブジェクトを指定する型パラメータ</typeparam>
        /// <param name="source">設定もととなるオブジェクト</param>
        /// <param name="setting">値の設定方法を指定する設定</param>
        /// <returns>値が設定されたオブジェクト</returns>
        public static TDest ReFill<TDest>(object source, DataCopierSetting setting) where TDest : new() {
            TDest dest = Activator.CreateInstance<TDest>();
            ReFill(source, dest, setting);
            return dest;
        }

        /// <summary>
        /// 指定されたオブジェクトと一致する指定されたオブジェクトのプロパティに同じ値を設定します。
        /// </summary>
        /// <param name="source">設定もととなるオブジェクト</param>
        /// <param name="destination">設定されるオブジェクト</param>
        /// <remarks><see cref="TypeCode" /> に設定された DBNull , Empty , Object 以外の型のみ値を設定します。</remarks>
        public static void ReFill(object source, object destination) {
            ReFill(source, destination, new DataCopierSetting());
        }

        /// <summary>
        /// 指定されたオブジェクトと一致する指定されたオブジェクトのプロパティに同じ値を設定します。
        /// </summary>
        /// <param name="source">設定もととなるオブジェクト</param>
        /// <param name="destination">設定されるオブジェクト</param>
        /// <param name="setting">値の設定方法を指定する設定</param>
        public static void ReFill(object source, object destination, DataCopierSetting setting) {
            if (source == null) {
                throw new ArgumentNullException("source");
            }
            if (destination == null) {
                return;
            }
            PropertyInfo[] sourceProperties = source.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            Type destType = destination.GetType();


            foreach (PropertyInfo sourceProp in sourceProperties) {
                try {
                    if (!sourceProp.CanRead) {
                        continue;
                    }
                    Type sourcePropType = GetTargetType(sourceProp);

                    if (setting.SimpleTypeOnly) {
                        if (!IsSimpleType(sourcePropType)) {
                            continue;
                        }
                    }

                    String destPropName = GetDestPropName(sourceProp.Name, setting);
                    PropertyInfo destProp = destType.GetProperty(destPropName, BindingFlags.Public | BindingFlags.Instance);
                    if (destProp == null || !destProp.CanWrite) {
                        continue;
                    }
                    object sourceValue = sourceProp.GetGetMethod().Invoke(source, null);

                    SetValueToDest(source, destination, sourcePropType, sourceValue, destProp, setting);

                } catch {
                    if (setting.ThrowOnError) {
                        throw;
                    }
                }
            }
        }

        private static bool IsSimpleType(Type target) {
            TypeCode code = Type.GetTypeCode(target);
            return !(code == TypeCode.DBNull || code == TypeCode.Empty || (code == TypeCode.Object && target.FullName != "System.DateTimeOffset" ) );
        }

        private static Type GetTargetType(PropertyInfo prop) {
            Type target = Nullable.GetUnderlyingType(prop.PropertyType);
            if (target == null) {
                return prop.PropertyType;
            }
            return target;

        }

        private static bool IsNullabe(Type type) {
            return Nullable.GetUnderlyingType(type) != null;
        }


        private static String GetDestPropName(string name, DataCopierSetting setting) {
            if (setting != null && setting.NameMap != null) {
                if (setting.NameMap.ContainsKey(name)) {
                    return setting.NameMap[name];
                }
            }
            return name;
        }

        private static void SetValueToDest(object source, object destination, Type sourcePropType, object sourceValue, PropertyInfo destProp, DataCopierSetting setting) {
            Type destPropType = GetTargetType(destProp);
            if (!destPropType.IsAssignableFrom(sourcePropType) && !setting.Convert) {
                return;
            }

            if (sourcePropType == typeof(string) && sourceValue != null && sourceValue.ToString() == "" && setting.EmptyToNull) {
                sourceValue = null;
            }

            if (setting.Convert) {
                if (sourceValue != null) {
                    sourceValue = Convert.ChangeType(sourceValue, destPropType);
                }
            }

            //nullが設定できない値の場合は例外
            if (sourceValue == null && !IsNullabe(destProp.PropertyType) && destProp.PropertyType.IsValueType) {
                if (setting.ThrowOnError) {
                    throw new InvalidOperationException();
                }
                return;
            }
            destProp.GetSetMethod().Invoke(destination, new object[] { sourceValue });
        }

    }

    /// <summary>
    /// <see cref="DataCopier"></see>で値を設定する際の、設定方法を定義します。
    /// </summary>
    /// <remarks></remarks>
    public class DataCopierSetting {

        /// <summary>
        /// インスタンスを生成します。
        /// </summary>
        /// <remarks></remarks>

        public DataCopierSetting() {
            this.SimpleTypeOnly = true;
            this.NameMap = new Dictionary<string,string>();
            this.Convert = true;
            this.EmptyToNull = true;
            this.ThrowOnError = false;
        }

        /// <summary>
        /// 単純な型のみ設定するかどうかを取得または設定します。
        /// </summary>
        /// <value>単純な型のみ設定するかどうか</value>
        /// <returns>単純な型のみ設定する場合は true 、そうでない場合は false</returns>
        /// <remarks>単純な型とは <see cref="Type" /> の <see cref="TypeCode" /> が
        /// <see cref="TypeCode.DBNull" /> , <see cref="TypeCode.Empty" /> および <see cref="TypeCode.Object" />
        /// 以外の型を指します。
        /// </remarks>
        public bool SimpleTypeOnly { get; set; }

        /// <summary>
        /// 値を交換するプロパティ名のマッピングを取得します。
        /// </summary>
        /// <returns>値を交換するプロパティ名のマッピング</returns>
        /// <remarks></remarks>
        public Dictionary<string, string> NameMap { get; private set; }

        /// <summary>
        /// 値交換時に型変換を行うかどうかを取得または設定します。
        /// </summary>
        /// <value>値交換時に型変換を行うかどうか</value>
        /// <returns>値交換時に型変換を行う場合は true , そうでない場合は false</returns>
        public bool Convert { get; set; }

        /// <summary>
        /// 値交換時に空文字列をNULLに変換するかどうかを取得または設定します。
        /// </summary>
        public bool EmptyToNull { get; set; }

        /// <summary>
        /// 例外を発行するかどうかを取得または設定します。
        /// </summary>
        public bool ThrowOnError { get; set; }
    }

}

