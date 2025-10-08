using System.ComponentModel.DataAnnotations;
using System.Data;

namespace ABC_retailers.Models.ViewModel
{
    public class OrderCreateViewModel
    {
        [Required]
        [Display(Name = "Customer")]
        public string CustomerId { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Product")]
        public string ProductId { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Order Date")]
        [DataType(DataType.Date)]
        public DateTime OrderDate { get; set; } = DateTime.UtcNow.Date;

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }

        public string Status { get; set; } = "Submitted";

        public List<Customer> Customers { get; set; } = new();
        public List<Product> Products { get; set; } = new();
    }
}