using System.ComponentModel.DataAnnotations;
using SQLite;

namespace BackeryMovil.Models
{
    [Table("Categories")]
    public class Category
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Required, SQLite.MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        [SQLite.MaxLength(200)]
        public string Description { get; set; } = string.Empty;

        public string IconName { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // API sync properties
        public int? ApiId { get; set; }
        public bool IsSynced { get; set; } = false;
        public DateTime? LastSyncAt { get; set; }
    }
}

