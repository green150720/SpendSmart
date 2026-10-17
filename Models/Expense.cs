using System.ComponentModel.DataAnnotations;

namespace SpendSmart.Models
{
    public class Expense
    {
        public int Id { get; set; }

        [Range(0.01, 1000000000)]
        public decimal Value { get; set; }

        [Required]
        public string? Description { get; set; }

        [Required]
        public string? Category { get; set; }

        [Required]
        public DateTime? Date { get; set; } = DateTime.Today;
    }
}
