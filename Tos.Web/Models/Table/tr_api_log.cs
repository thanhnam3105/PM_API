namespace TOS.Web.Models
{
    using System;
    using System.Collections.Generic;
    public partial class tr_api_log
    {
        public string cd_create { get; set; }
        public string cd_status { get; set; }
        public string cd_type_data { get; set; }
        public string cd_update { get; set; }
        public Nullable<System.DateTime> dt_create { get; set; }
        public Nullable<System.DateTime> dt_update { get; set; }
        public string nm_result { get; set; }
        public long no_seq { get; set; }
    }
}