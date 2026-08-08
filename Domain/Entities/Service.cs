using Domain.Entities;

namespace Domain.Entities
{
    public class Service : BaseEntity<int>
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation
        public ICollection<BookingItem> BookingItems { get; set; } = new List<BookingItem>();
    }
}