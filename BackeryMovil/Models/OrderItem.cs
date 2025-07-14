using System.ComponentModel.DataAnnotations;
using SQLite;

namespace BackeryMovil.Models
{
    [Table("OrderItems")]
    public class OrderItem
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Required]
        public int OrderId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public decimal UnitPrice { get; set; }

        public decimal TotalPrice => Quantity * UnitPrice;

        // Navigation properties (not stored in SQLite)
        [Ignore]
        public Product? Product { get; set; }

        [Ignore]
        public Order? Order { get; set; }
    }
}
