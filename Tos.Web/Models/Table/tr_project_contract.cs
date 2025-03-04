namespace TOS.Web.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class tr_project_contract
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long cd_project { get; set; }
        public string no_contract { get; set; }
        public string cd_customer { get; set; }
        public Nullable<System.DateTime> dt_contract { get; set; }
        public Nullable<System.DateTime> dt_contract_period_start { get; set; }
        public Nullable<System.DateTime> dt_contract_period_end { get; set; }
        public string mm_sale_month_plan { get; set; }
        public string mm_sale_month_actual { get; set; }
        public Nullable<decimal> qty_man_month_plan { get; set; }
        public Nullable<decimal> qty_revenue_plan { get; set; }
        public Nullable<decimal> qty_revenue_actual { get; set; }
        public Nullable<bool> flg_cost_fix { get; set; }
        public Nullable<short> cd_currency { get; set; }
        public Nullable<decimal> qty_currency_convert { get; set; }
        public string nm_contract_status { get; set; }
        public string nm_note { get; set; }
        public string cd_create { get; set; }
        public Nullable<System.DateTime> dt_create { get; set; }
        public string cd_update { get; set; }
        public Nullable<System.DateTime> dt_update { get; set; }
        public byte[] ts { get; set; }
    }
}