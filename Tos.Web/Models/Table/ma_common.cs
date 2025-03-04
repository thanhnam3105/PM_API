namespace TOS.Web.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class ma_common
    {
        public byte cd_category { get; set; }
        public short cd_common { get; set; }
        public string nm_common { get; set; }
        public Nullable<bool> flg_edit { get; set; }
        public Nullable<bool> flg_use { get; set; }
        public short cd_sort { get; set; }
        public string cd_create { get; set; }
        public Nullable<System.DateTime> dt_create { get; set; }
        public string cd_update { get; set; }
        public Nullable<System.DateTime> dt_update { get; set; }
        public byte[] ts { get; set; }
    }
}