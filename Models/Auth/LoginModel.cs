using System.ComponentModel.DataAnnotations;

namespace ItemHub.Models.Auth
{
    public class LoginModel
    {
        [Required(ErrorMessage = "Не указан логин")]
        public required string Login { get; set; }

        [Required(ErrorMessage = "Не указан пароль")]
        [DataType(DataType.Password)]
        public required string Password { get; set; }
    }
}
