namespace TOS.Web.Models
{
    using System;
    using System.Collections.Generic;
    public partial class tr_project_member
    {
        public long no_seq { get; set; }
        public string cd_project_rdm { get; set; }
        public string cd_employee { get; set; }
        public string nm_role { get; set; }
        public Int16 flg_leader { get; set; }
        public byte[] ts { get; set; }
    }
}