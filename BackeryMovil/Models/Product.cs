using System.ComponentModel.DataAnnotations;
using SQLite;

namespace BackeryMovil.Models
{
    [Table("Products")]
    public class Product
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Required, SQLite.MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [SQLite.MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public decimal Price { get; set; }

        // Esta propiedad debe almacenar la URL de la imagen, ya sea local o remota
        public string ImagePath { get; set; } = string.Empty;

        [Required]
        public int CategoryId { get; set; }

        public bool IsAvailable { get; set; } = true;

        public int StockQuantity { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        [Ignore]
        public Category? Category { get; set; }

        public int? ApiId { get; set; }
        public bool IsSynced { get; set; } = false;
        public DateTime? LastSyncAt { get; set; }
    }
}