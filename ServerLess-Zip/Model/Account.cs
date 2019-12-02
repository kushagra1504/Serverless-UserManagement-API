using System.ComponentModel.DataAnnotations;

namespace ServerLess_Zip.Model
{
    public class Account
{

    [Required(ErrorMessage = "The email address is required")]
    [EmailAddress(ErrorMessage = "Invalid Email Address")]
    public string EmailAddress { get; set; }

    [Required]
    [Range(0, 1000, ErrorMessage = "Zip allows credit upto 1000$.")]
    public decimal CreditTaken { get; set; }

    [Required]
    [Range(0, 1000, ErrorMessage = "Zip allows credit upto 1000$.")]
    public decimal BalanceAvailable { get; set; }
}
}
