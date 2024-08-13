using System.ComponentModel.DataAnnotations;

namespace ItemHub.Models.OnlyItem
{
    public class Item
    {
        [Key]
        public Guid Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime? CreatedDate { get; set; }
        public List<string> PathImages { get; set; } = new();
        public int? Price { get; set; }

        public Item(string? title, string? description, List<string> pathImages, int? price)
        {
            Id = Guid.NewGuid();
            Title = title;
            Description = description;
            CreatedDate = DateTime.UtcNow;
            PathImages = pathImages;
            Price = price;
        }
    }
}
