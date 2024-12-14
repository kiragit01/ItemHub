using System.ComponentModel.DataAnnotations;
using Npgsql.Replication.TestDecoding;

namespace ItemHub.Models.Auth
{
    public class LoginModel
    {
        [Required(ErrorMessage = "Не указан логин")]
        [RegularExpression("[A-Za-z]{1}[A-Za-z0-9._]{1,24}", ErrorMessage = "Недопустимые символы: a-z, 0-9, _. Длина от 2 до 25.")]
        public required string Login { get; init; }

        [Required(ErrorMessage = "Не указан пароль")]
        [DataType(DataType.Password)]
        public required string Password { get; init; }
    }
}
