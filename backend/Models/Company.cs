using System.ComponentModel.DataAnnotations;
namespace EMSSolution.Models
{
    public class Company
    {
        public int Id { get; set; }

        [Required]
        public string CompanyName { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        public string City { get; set; }

        public string Landmark { get; set; }

        [Required]
        public int MonthStartfrom { get; set; }

        public string Logo { get; set; }


    }
}