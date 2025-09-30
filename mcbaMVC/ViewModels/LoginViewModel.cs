using System.ComponentModel.DataAnnotations;

namespace mcbaMVC.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Login ID is required")]
        [StringLength(8, MinimumLength = 8, ErrorMessage = "Login ID must be exactly 8 digits")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "Login ID must be numeric")]
        public string LoginId { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
