namespace TOS.Web.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.ComponentModel.DataAnnotations;

    public partial class tr_file
    {
        [Key]
        public long cd_project { get; set; }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long no_seq { get; set; }
        public string nm_path { get; set; }
        public string nm_file { get; set; }
        public string cd_create { get; set; }
        public Nullable<System.DateTime> dt_create { get; set; }
        public string cd_update { get; set; }
        public Nullable<System.DateTime> dt_update { get; set; }
    }
}