namespace TOS.Web.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.ComponentModel.DataAnnotations;

    public partial class ma_project
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long cd_project { get; set; }
        public string cd_project_rdm { get; set; }
        public string nm_project { get; set; }
        public string nm_project_short { get; set; }
        public Nullable<decimal> yy_fiscal { get; set; }
        public Nullable<short> cd_status { get; set; }
        public Nullable<short> cd_type { get; set; }
        public Nullable<System.DateTime> dt_start { get; set; }
        public Nullable<System.DateTime> dt_end { get; set; }
        public Nullable<decimal> qty_progress_rate { get; set; }
        public string nm_note { get; set; }
        public string cd_create { get; set; }
        public string cd_update { get; set; }
        public Nullable<System.DateTime> dt_create { get; set; }
        public Nullable<System.DateTime> dt_update { get; set; }
        public byte[] ts { get; set; }
    }
}