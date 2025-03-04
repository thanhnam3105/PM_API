namespace TOS.Web.Models
{
    using System;
    using System.Collections.Generic;
    public partial class tr_project_rdm
    {
        public string cd_create { get; set; }
        public string cd_identifier { get; set; }
        public string cd_update { get; set; }
        public Nullable<System.DateTime> dt_create { get; set; }
        public Nullable<System.DateTime> dt_update { get; set; }
        public string mm_input { get; set; }
        public Nullable<double> qty_spent { get; set; }
        public byte[] ts { get; set; }
    }
}