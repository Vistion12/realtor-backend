using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropertyStore.DataAccess.Entities
{
    public class PropertyEntity
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty; // novostroyki, secondary, rent, countryside, invest

        public decimal Price { get; set; }

        public string Address { get; set; } = string.Empty;

        public decimal Area { get; set; }

        public int Rooms { get; set; }
        public string Description { get; set; } = string.Empty;
        public string MainPhotoUrl { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
