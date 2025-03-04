using C5;
using DocumentFormat.OpenXml.Bibliography;
using System;
using System.Collections.Generic;

namespace TOS.Web.Controllers.Helpers
{
    public static class Contants
    {
        /// <summary>
        /// Status
        /// </summary>
        public static class APIStatus
        {
            public static readonly string Successed = "0";
            public static readonly string Failed = "1";
        }

        /// <summary>
        /// Data Sync Type
        /// </summary>
        public static class DataSyncType
        {
            public static readonly string Attendance = "1";
            public static readonly string RedmineTime = "2";
            public static readonly string RedmineIdentifier = "3";
            public static readonly string RedmineBugRequest = "4";
            public static readonly string RedmineMemberShip = "5";
            public static readonly string RedmineUsers = "6";
        }

        /// <summary>
        /// Password default
        /// </summary>
        public static class Password
        {
            public static readonly string Init = "Init#000**";
            public static readonly string InitUserCode = "Init#";
        }

        /// <summary>
        /// Code of flg_leave
        /// </summary>
        public static class FlgLeave
        {
            public static readonly short Leaved = 1;
            public static readonly short NotLeave = 0;
        }

        /// <summary>
        /// Flag common
        /// </summary>
        public static class Flag
        {
            public static readonly short InUse = 0;
            public static readonly short NotInUse = 1;
            public static readonly short NotDelete = 0;
            public static readonly short Delete = 1;
        }

        /// <summary>
        /// Choose value time when login
        /// </summary>
        public static class LoginRemember
        {
            /// <summary> Default : 1</summary>
            public static readonly int Default = 1;
            /// <summary> Remember : 30</summary>
            public static readonly int Remember = 30;
        }

        /// <summary>
        /// バリデーション種別を定義します
        /// </summary>
        public static class ValidationRuleTypes
        {
            public const string Required = "required";
            public const string MaxLength = "maxlength";
            public const string Range = "range";
            public const string Number = "number";
            public const string Integer = "integer";
            public const string PointLength = "pointlength";
            public const string Date = "date";
            public const string Boolean = "bit";
            public const string Zenkaku = "zenkaku";
            public const string Hankaku = "hankaku";
            public const string Alphabet = "alphabet";
            public const string Alphanum = "alphanum";
            public const string Haneisukigo = "haneisukigo";
            public const string Hankana = "hankana";
        }

        /// <summary>
        /// Message common
        /// </summary>
        public static class Message
        {
            public static readonly string errorToken = "Token Error";
        }
        /// <summary>
        /// カラムタイプの種類を定義します
        /// </summary>
        public static class ColumnTypes
        {
            public static readonly string BoolTypes = "Boolean";
            public static readonly string DateTimeTypes = "DateTime";
            public static readonly string DateTimeOffsetTypes = "DateTimeOffset";
        }

        /// <summary>
        /// CSV更新区分の種類を定義します
        /// </summary>
        public static class CsvUpdateColumn
        {
            public static readonly string NotUpdate = "0";
            public static readonly string CreateUpdate = "1";
            public static readonly string Delete = "2";
        }

        /// <summary>
        /// CSV更新区分の種類を定義します
        /// </summary>
        public static class TimeSet
        {
            public static readonly string Hour = "01";
            public static readonly string Minute = "02";
        }

        /// <summary>
        /// ブール型の数値変換を定義します
        /// </summary>
        public static class BoolStrings
        {
            public static readonly Dictionary<string, Boolean> BoolMembers = new Dictionary<string, Boolean>()
            {
                {"0", false},
                {"1", true}
            };
        }

        /// <summary>
        /// Category common
        /// </summary>
        public static class Category
        {
            public static readonly int Authority = 1;
            public static readonly int ProjectStatus = 2;
            public static readonly int TypeOfBug = 3;
            public static readonly int BugStatus = 4;
            public static readonly int WorkingDaysPerMonth = 5;
            public static readonly int HoursPerDay = 6;
            public static readonly int FiscalYearStartMonth = 7;
            public static readonly int ProjectType = 8;
            public static readonly int SyncDataType = 9;
            public static readonly int SyncDataStatus = 10;
            public static readonly int Country = 11;
            public static readonly int Currency = 12;
            public static readonly int TypeCustomer = 13;
        }

        /// <summary>
        /// Used
        /// </summary>
        public static class FlgUse
        {
            public static readonly bool InUse = true;
            public static readonly bool NotInUse = false;
        }

        /// <summary>
        /// Used
        /// </summary>
        public static class ProjectStatus
        {
            public static readonly short New = 0;
            public static readonly short OnGoing = 1;
            public static readonly short Done = 2;
        }
    }
}
