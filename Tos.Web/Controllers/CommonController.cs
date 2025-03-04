using ClosedXML.Excel;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web;
using System.Web.Http;
using System.Windows.Documents;
using Tos.Web.Controllers.Helpers;
using Tos.Web.Utilities;
using TOS.Web.Controllers.Helpers;
using TOS.Web.Models;
using static TOS.Web.Controllers.Helpers.Contants;

namespace TOS.Web.Controllers
{
    public class CommonController : ApiController
    {
        public string getScreenTarget(HttpRequestMessage Request)
        {
            return Request.Headers.GetValues("Screen").FirstOrDefault();
        }

        public static ProjectManagementEntities contextDBO;
        public CommonController()
        {
            contextDBO = new ProjectManagementEntities();
        }

        /// <summary>
        /// Set value of base class for extend class
        /// </summary>
        /// <param name="className"></param>
        /// <param name="itemValues"></param>
        public void SetBaseValueForExtendClass(dynamic className, dynamic itemValues)
        {
            var baseProperties = className.GetType().BaseType.GetProperties();
            foreach (var prop in baseProperties)
            {
                var value = itemValues.GetType().GetProperty(prop.Name).GetValue(itemValues, null);
                className.GetType().GetProperty(prop.Name).SetValue(className, value);
            }
        }

        /// <summary>
        /// Convert code farm to Connect String
        /// </summary>
        /// <param name="farmCode"></param>
        public ProjectManagementEntities getSQLContext()
        {
            ProjectManagementEntities context = new ProjectManagementEntities();
            return context;
        }

        /// <summary>
        /// Get parameter allow null value for npgsql
        /// </summary>
        /// <param name="name"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public static SqlParameter GetParam(string name, object val, bool BlankIsNull = true)
        {
            if (val == null || (BlankIsNull && val.ToString() == string.Empty))
            {
                return new SqlParameter(name, DBNull.Value);
            }
            return new SqlParameter(name, val);
        }

        /*
        * Get data in object with name
        */
        public string GetValObjDy(object obj, short indexCommon)
        {
            string propertyName = "nm_common_";
            propertyName = propertyName + indexCommon;
            foreach (var prop in obj.GetType().GetProperties())
            {
                if (prop.Name == propertyName)
                    return prop.GetValue(obj) != null ? prop.GetValue(obj).ToString() : null;
            }
            return null;
        }

        /*
        * Get data with cd_common
        */
        public List<CommonModelResponse> GetCommonData(int cd_category, bool? flg_use = null, int? cd_common = null )
        {
            ProjectManagementEntities _context = new ProjectManagementEntities();
            var result = _context.ma_common
                            .Where(x => x.cd_category == cd_category
                                && (cd_common == null || x.cd_common == cd_common)
                                && (flg_use == null || x.flg_use == flg_use))
                            .OrderBy(x => x.cd_sort)
                            .AsEnumerable()
                            .Select(cm => new CommonModelResponse
                            {
                                value = cm.cd_common,
                                name = cm.nm_common,
                            }).ToList();

            return result;
        }

        /*
        * Get data witd control disable
        */
        public List<CommonModelResponse> GetCommonDataWithDisabled(int cd_category, int? cd_common = null)
        {
            ProjectManagementEntities _context = new ProjectManagementEntities();
            var result = _context.ma_common
                            .Where(x => x.cd_category == cd_category && (cd_common == null || x.cd_common == cd_common))
                            .OrderBy(x => x.cd_sort)
                            .AsEnumerable()
                            .Select(cm => new CommonModelResponse
                            {
                                value = cm.cd_common,
                                name = cm.nm_common,
                                isDisabled = cm.flg_use != FlgUse.InUse
                            }).ToList();

            return result;
        }

        /*
        * Get data with list
        */
        public List<CommonModelResponse> GetData(string cd_category, List<string> listCommon)
        {
            ProjectManagementEntities context = new ProjectManagementEntities();
            var result = context.ma_common
                            .Where(x => x.cd_category.Equals(cd_category)
                                && (listCommon.Count == 0 || listCommon.Contains(x.cd_common.ToString())))
                            .OrderBy(x => x.cd_sort)
                            .AsEnumerable()
                            .Select(cm => new CommonModelResponse
                            {
                                value = cm.cd_common,
                                name = cm.nm_common
                            }).ToList();

            return result;
        }

        /*
        * Get data another
        */
        public List<CommonModelResponse> GetDataAnotherCode(string cd_category, string cd_common)
        {
            ProjectManagementEntities context = new ProjectManagementEntities();
            var result = context.ma_common
                            .Where(x => x.cd_category.Equals(cd_category)
                                 && x.cd_common.ToString() != cd_common)
                            .OrderBy(x => x.cd_sort)
                            .AsEnumerable()
                            .Select(cm => new CommonModelResponse
                            {
                                value = cm.cd_common,
                                name = cm.nm_common
                            }).ToList();

            return result;
        }

        //convert to column excel
        public string ToExcelColumnName(int columnNumber)
        {
            int dividend = columnNumber;
            string columnName = "";

            while (dividend > 0)
            {
                int modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar(65 + modulo) + columnName;
                dividend = (int)((dividend - modulo) / 26);
            }

            return columnName;
        }

        /// <summary>
        /// Batch insert. Real fast.
        /// </summary>
        /// <typeparam name="T">Type of generic collection</typeparam>
        /// <param name="dbContext">Connection object</param>
        /// <param name="tableName">Insert the generic collection into the table name of the local database table</param>
        /// <param name="list">To insert a large generic collection</param>
        public static bool BulkInsert<T>(DbContext dbContext, string tableName, IList<T> list)
        {
            try
            {
                if (list == null || list.Count == 0) return true;
                if (dbContext.Database.Connection.State != ConnectionState.Open)
                {
                    dbContext.Database.Connection.Open(); //Open Connection connection
                }
                using (var bulkCopy = new SqlBulkCopy(dbContext.Database.Connection.ConnectionString))
                {
                    bulkCopy.BatchSize = list.Count;
                    bulkCopy.DestinationTableName = tableName;

                    var table = new DataTable();
                    var props = TypeDescriptor.GetProperties(typeof(T))

                        .Cast<PropertyDescriptor>()
                        .Where(propertyInfo => propertyInfo.PropertyType.Namespace.Equals("System"))
                        .ToArray();

                    foreach (var propertyInfo in props)
                    {
                        bulkCopy.ColumnMappings.Add(propertyInfo.Name, propertyInfo.Name);
                        table.Columns.Add(propertyInfo.Name, Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType);
                    }

                    var values = new object[props.Length];
                    foreach (var item in list)
                    {
                        for (var i = 0; i < values.Length; i++)
                        {
                            values[i] = props[i].GetValue(item);
                        }
                        table.Rows.Add(values);
                    }

                    bulkCopy.WriteToServer(table);
                    if (dbContext.Database.Connection.State != ConnectionState.Closed)
                    {
                        dbContext.Database.Connection.Close(); //Close Connection
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                if (dbContext.Database.Connection.State != ConnectionState.Closed)
                {
                    dbContext.Database.Connection.Close(); //Close Connection
                }
                throw ex;
            }
        }

        /// <summary>
        /// Convert from Datetime(7) in C# to Datetime(2) in SQL.
        /// Using for table's primary key
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static DateTime ConvertSQLDateTime(DateTime dt)
        {
            if (dt == null)
                return dt;
            return DateTime.Parse(dt.ToString("yyyy/MM/dd HH:mm:ss.ff"));
        }

        public bool isDateFormat(string dateValue, string[] formats)
        {

            DateTime dt;
            if (dateValue == null || dateValue.ToString() == "" || dateValue.GetType().Name == "DateTime")
            {
                return true;
            }


            if (!DateTime.TryParseExact(dateValue, formats,
                            System.Globalization.CultureInfo.InvariantCulture,
                            DateTimeStyles.None, out dt))
            {
                return false;
            }
            return true;
        }

        public bool isDateFormat(object dateValue, string formats)
        {
            DateTime dt;

            if (dateValue == null || dateValue.ToString() == "" || dateValue.GetType().Name == "DateTime")
            {
                return true;
            }

            if (!DateTime.TryParseExact(dateValue.ToString(), formats,
                            System.Globalization.CultureInfo.InvariantCulture,
                            DateTimeStyles.None, out dt))
            {
                return false;
            }
            return true;
        }

        public string ConvertDateToString(object dateValue, string formats)
        {
            DateTime dt;
            if (dateValue == null || dateValue.ToString() == "" )
            {
                return null;
            }
            if (dateValue.GetType().Name == "DateTime") 
            {
                return ((DateTime)dateValue).ToString("yyyy-MM-dd HH:mm:ss");
            }
            if (!DateTime.TryParseExact(dateValue.ToString(), formats, System.Globalization.CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
            {
                return dateValue.ToString();

            }
            return DateTime.ParseExact(dateValue.ToString(), formats, CultureInfo.InvariantCulture).ToString("yyyy-MM-dd HH:mm:ss");
        }

        public void CreateFolder(string folderName)
        {
            if (!System.IO.Directory.Exists(folderName))
            {
                System.IO.Directory.CreateDirectory(folderName);
            }
        }

        public class ErrorFile
        {
            public string sheetName { get; set; }
            public string cloumn { get; set; }
            public int row { get; set; }
            public string field { get; set; }
            public string error { get; set; }
            public string value { get; set; }
        }
    }
}
