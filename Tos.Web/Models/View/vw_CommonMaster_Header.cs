namespace TOS.Web.Models
{
    using System;
    using System.Collections.Generic;

    public partial class vw_CommonMaster_Header
    {
        public byte cd_category { get; set; }
        public short cd_common { get; set; }
        public string nm_common { get; set; }
        public Nullable<bool> flg_edit { get; set; }
        public Nullable<bool> flg_use { get; set; }
        public short cd_sort { get; set; }
        public DateTime? dt_update { get; set; }
        public int count_num { get; set; }
    }
}