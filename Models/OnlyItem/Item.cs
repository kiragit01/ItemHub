using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace ItemHub.Models.OnlyItem
{
    public class Item
    {
        [Key]
        public Guid Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string Creator { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public List<string> PathImages { get; set; } = new();
        public int? Price { get; set; }

        public Item(Guid id, string? title, string? description, string creator, List<string> pathImages, int? price)
        {
            Id = id;
            Title = title;
            Description = description;
            Creator = creator;
            CreatedDate = DateTime.UtcNow;
            PathImages = pathImages;
            Price = price;
        }

        public void UpdateDate() => UpdatedDate = DateTime.UtcNow;
    }
}
