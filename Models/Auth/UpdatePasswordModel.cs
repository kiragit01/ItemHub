using System.ComponentModel.DataAnnotations;

namespace ItemHub.Models.Auth;

public class UpdatePasswordModel
{
    [Required(ErrorMessage = "Введите старый пароль")]
    [DataType(DataType.Password)]
    public required string OldPassword { get; set; }
    
    [Required(ErrorMessage = "Введите новый пароль")]
    [StringLength(maximumLength: 32, MinimumLength = 3, ErrorMessage = "минимум 3, максимум 32 символа")]
    [DataType(DataType.Password)]
    public required string NewPassword { get; set; }
    
    [Required(ErrorMessage = "Повторите новый пароль")]
    [StringLength(maximumLength: 32, MinimumLength = 3, ErrorMessage = "минимум 3, максимум 32 символа")]
    [DataType(DataType.Password)]
    [Compare("NewPassword", ErrorMessage = "Пароль не совпадает")]
    public required string NewPasswordConfirm { get; set; }
}