using System.ComponentModel.DataAnnotations;

namespace BackEnd.DTO.AccountDTO
{
    public class UpdateAccountInput
    {
        [Required]
        [EmailAddress]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Email must be from 6 to 100 characters")]
        public string Email { get; set; }
        [Required]
        public string Fullname { get; set; }
    }
}