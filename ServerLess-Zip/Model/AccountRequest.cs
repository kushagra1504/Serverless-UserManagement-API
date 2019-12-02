using System.ComponentModel.DataAnnotations;

namespace ServerLess_Zip.Model
{
    public class AccountRequest
    {
        [Required(ErrorMessage = "The email address is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string EmailAddress { get; set; }

        [Required]
        [Range(0, 1000, ErrorMessage = "We allow credit upto 1000$.")]
        public double CreditRequested { get; set; }
    }
}
