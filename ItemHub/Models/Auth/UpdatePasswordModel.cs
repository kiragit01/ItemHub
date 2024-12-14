using System.ComponentModel.DataAnnotations;

namespace ItemHub.Models.Auth;

public class UpdatePasswordModel
{
    [DataType(DataType.Password)]
    [Required(ErrorMessage = "Введите старый пароль")]
    public required string OldPassword { get; set; }
    
    [DataType(DataType.Password)]
    [Required(ErrorMessage = "Введите новый пароль")]
    [MinLength(3, ErrorMessage = "Минимальная длина: 3.")]
    [MaxLength(35, ErrorMessage = "Максимальная длина: 35.")]
    public required string NewPassword { get; set; }
    
    [DataType(DataType.Password)]
    [Required(ErrorMessage = "Повторите новый пароль")]
    [MinLength(3, ErrorMessage = "Минимальная длина: 3.")]
    [MaxLength(35, ErrorMessage = "Максимальная длина: 35.")]
    [Compare("NewPassword", ErrorMessage = "Пароль не совпадает")]
    public required string NewPasswordConfirm { get; set; }
}