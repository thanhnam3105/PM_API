namespace TOS.Web.Models
{
    using System;
    using System.Collections.Generic;
    public partial class ma_project_rdm
    {
        public string cd_create { get; set; }
        public string cd_identifier { get; set; }
        public string cd_project_rdm { get; set; }
        public string cd_status { get; set; }
        public string cd_update { get; set; }
        public Nullable<System.DateTime> dt_create { get; set; }
        public Nullable<System.DateTime> dt_update { get; set; }
        public Nullable<byte> flg_cogs_target { get; set; }
        public string nm_description { get; set; }
        public string nm_project { get; set; }
        public string nm_type_project { get; set; }
        public byte[] ts { get; set; }
    }
}