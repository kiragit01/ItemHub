using System.ComponentModel.DataAnnotations;

namespace ItemHub.Models.OnlyItem
{
    public class ItemModel
    {
        [Required(ErrorMessage = "Введите название")]
        public string? Title { get; set; }
        public string? Description { get; set; }
        public IFormFileCollection? Images { get; set; }

        [Required(ErrorMessage = "Введите цену")]
        [Range(1, 100000000, ErrorMessage = "Недопустимая цена")]
        public int? Price { get; set; }
    }
}
