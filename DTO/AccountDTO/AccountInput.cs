using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace examedu.DTO.AccountDTO
{
    public class AccountInput
    {
        [Required]
        [EmailAddress]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Email must be from 6 to 100 characters")]
        public string Email { get; set; }
        [Required]
        public string Fullname { get; set; }
        [Required]
        public int RoleID { get; set; }
    }
}