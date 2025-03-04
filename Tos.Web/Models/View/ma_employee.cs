namespace TOS.Web.Models
{
    using System;
    using System.Collections.Generic;
    public partial class ma_employee
    {
        public string cd_create { get; set; }
        public string cd_department { get; set; }
        public string cd_employee { get; set; }
        public string cd_update { get; set; }
        public Nullable<System.DateTime> dt_contract { get; set; }
        public Nullable<System.DateTime> dt_create { get; set; }
        public Nullable<byte> flg_leaved { get; set; }
        public Nullable<System.DateTime> dt_update { get; set; }
        public Nullable<byte> id_authority { get; set; }
        public Nullable<byte> id_type_employment { get; set; }
        public string kbn_role { get; set; }
        public string mail { get; set; }
        public string nm_department_en { get; set; }
        public string nm_department_ja { get; set; }
        public string nm_department_vi { get; set; }
        public string nm_employee { get; set; }
        public string nm_employee_st { get; set; }
        public string nm_position_en { get; set; }
        public string nm_position_jp { get; set; }
        public string nm_position_vi { get; set; }
        public string redmine_name { get; set; }
        public byte[] ts { get; set; }
    }
}