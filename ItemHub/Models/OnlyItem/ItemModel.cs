using System.ComponentModel.DataAnnotations;

namespace ItemHub.Models.OnlyItem
{
    public class ItemModel
    {
        [Required(ErrorMessage = "Введите название")]
        public required string Title { get; set; }
        public string? Description { get; set; }
        public IFormFileCollection? Images { get; set; }

        [Required(ErrorMessage = "Введите цену")]
        [Range(1, 100000000, ErrorMessage = "Недопустимая цена")]
        public required int Price { get; set; }
        public bool Published { get; set; }
    }
}
