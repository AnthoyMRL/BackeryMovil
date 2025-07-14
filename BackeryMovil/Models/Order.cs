using System.ComponentModel.DataAnnotations;
using SQLite;

namespace BackeryMovil.Models
{
    [Table("Orders")]
    public class Order
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Required, SQLite.MaxLength(100)]
        public string CustomerName { get; set; } = string.Empty;

        [Required, SQLite.MaxLength(15)]
        public string CustomerPhone { get; set; } = string.Empty;

        [SQLite.MaxLength(200)]
        public string DeliveryAddress { get; set; } = string.Empty;

        public decimal TotalAmount { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        public DateTime OrderDate { get; set; } = DateTime.Now;

        public DateTime? DeliveryDate { get; set; }

        [SQLite.MaxLength(500)]
        public string Notes { get; set; } = string.Empty;

        // API sync properties
        public int? ApiId { get; set; }
        public bool IsSynced { get; set; } = false;
        public DateTime? LastSyncAt { get; set; }
    }

    public enum OrderStatus
    {
        Pending = 0,
        Confirmed = 1,
        InPreparation = 2,
        Ready = 3,
        Delivered = 4,
        Cancelled = 5
    }
}
