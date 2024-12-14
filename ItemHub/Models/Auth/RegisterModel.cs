using System.ComponentModel.DataAnnotations;
using System.Data;

namespace ItemHub.Models.Auth
{
    public class RegisterModel
    {
        
        public IFormFile? Avatar { get; set; }
        
        [Required(ErrorMessage = "Введите имя")]
        [MinLength(2, ErrorMessage = "Минимальная длина: 2")]
        [MaxLength(50, ErrorMessage = "Максимальная длина: 50")]
        public required string Name { get; init; }
        
        [Required(ErrorMessage = "Введите логин")]
        [RegularExpression("[A-Za-z]{1}[A-Za-z0-9._]{1,24}", ErrorMessage = "Недопустимые символы: a-z, 0-9, _. Длина от 2 до 25.")]
        public required string Login { get; init; }

        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Введите пароль")]
        [MinLength(3, ErrorMessage = "Минимальная длина: 3.")]
        [MaxLength(35, ErrorMessage = "Максимальная длина: 35.")]
        public required string Password { get; init; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Пароль введен неверно")]
        public required string ConfirmPassword { get; init; }

        [DataType(DataType.EmailAddress)]
        [Required(ErrorMessage = "Введите почту")]
        [RegularExpression(@"[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,4}", ErrorMessage = "Некорректно введена почта")]
        public required string Email { get; init; }

        public bool Seller { get; init; }

    }
}
