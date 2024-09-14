using System.ComponentModel.DataAnnotations;
using System.Data;

namespace ItemHub.Models.Auth
{
    public class EditAccModel
    {

        [Required(ErrorMessage = "Введите логин")]
        [MaxLength(50, ErrorMessage = "Длина логина не должна превышать больше 50 символов")]
        public string? Login { get; set; }

        [Required(ErrorMessage = "Введите пароль")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [Required(ErrorMessage = "Введите имя")]
        [MinLength(2, ErrorMessage = "Минимальная длина должна быть больше 2 символов")]
        [MaxLength(50, ErrorMessage = "Длина имени не должна превышать больше 50 символов")]
        public string? Name { get; set; }

        [DataType(DataType.EmailAddress)]
        [Required(ErrorMessage = "Введите почту")]
        [RegularExpression(@"[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,4}", ErrorMessage = "Некорректно введена почта")]
        public string? Email { get; set; }

    }
}
