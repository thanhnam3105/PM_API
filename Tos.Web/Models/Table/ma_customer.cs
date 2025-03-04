namespace TOS.Web.Models
{
    using System;
    using System.Collections.Generic;
    public partial class ma_customer
    {
        public string cd_customer { get; set; }
        public string nm_customer { get; set; }
        public string nm_customer_short { get; set; }
        public Nullable<short> cd_type_customer { get; set; }
        public string nm_address { get; set; }
        public Nullable<short> cd_country { get; set; }
        public string no_tel { get; set; }
        public string nm_mail { get; set; }
        public string nm_pic { get; set; }
        public Nullable<bool> flg_active { get; set; }
        public string cd_create { get; set; }
        public Nullable<System.DateTime> dt_create { get; set; }
        public string cd_update { get; set; }
        public Nullable<System.DateTime> dt_update { get; set; }
        public byte[] ts { get; set; }
    }
}