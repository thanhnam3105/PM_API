using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TOS.Web.Models.CodeFirst
{
    [Table("AspNetUserRoles")]
    public class AspNetUserRoles
    {
        [Key]
        [Column("UserId")]
        public string UserId { get; set; }

        [Key]
        [Column("RoleId")]
        public string RoleId { get; set; }
    }
}