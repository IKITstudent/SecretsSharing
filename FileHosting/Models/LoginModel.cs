using System.ComponentModel.DataAnnotations;

namespace FileHosting.Models
{
    public class LoginModel
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; }


        public string ReturnUrl { get; set; }
    }
}
