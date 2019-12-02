using System.ComponentModel.DataAnnotations;

namespace ServerLess_Zip.Model
{

    public class User
    {

        [Required]
        public string Name { get; set; }

        [Required(ErrorMessage = "The email address is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string EmailAddress { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Salary should be a positive value")]
        public double MonthlySalary { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Expenses should be a positive value")]
        public double MonthlyExpenses { get; set; }
    }
}
