using System.ComponentModel.DataAnnotations;
using System.Data;

namespace ItemHub.Models.Auth
{
    public class EditAccountModel
    {
        public IFormFile? Avatar { get; set; }
        
        [Required(ErrorMessage = "Введите имя")]
        [MinLength(2, ErrorMessage = "Минимальная длина: 2")]
        [MaxLength(50, ErrorMessage = "Максимальная длина: 50")]
        public required string Name { get; init; }
        
        [Required(ErrorMessage = "Введите логин")]
        [RegularExpression("[A-Za-z]{1}[A-Za-z0-9._]{1,24}", ErrorMessage = "Недопустимые символы: a-z, 0-9, _. Длина от 2 до 25.")]
        public required string Login { get; init; }

        [DataType(DataType.EmailAddress)]
        [Required(ErrorMessage = "Введите почту")]
        [RegularExpression(@"[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,4}", ErrorMessage = "Некорректно введена почта")]
        public required string Email { get; init; }
        
        [DataType(DataType.MultilineText)]
        [MaxLength(1000, ErrorMessage = "Максимальная длина: 1000")]
        public string? Description { get; set; }
        
        [DataType(DataType.PhoneNumber)]
        public string? Phone { get; set; }

    }
}
