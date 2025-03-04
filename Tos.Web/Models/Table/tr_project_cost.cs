namespace TOS.Web.Models
{
    using System;
    using System.Collections.Generic;
    public partial class tr_project_cost
    {
        public string cd_create { get; set; }
        public double cd_project { get; set; }
        public string cd_update { get; set; }
        public Nullable<System.DateTime> dt_create { get; set; }
        public Nullable<System.DateTime> dt_update { get; set; }
        public string mm_cost { get; set; }
        public Nullable<double> qty_cost { get; set; }
        public byte[] ts { get; set; }
    }
}