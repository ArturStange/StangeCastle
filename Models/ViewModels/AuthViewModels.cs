using System.ComponentModel.DataAnnotations;

namespace StangeCastle.Models.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "O email é obrigatório.")]
    [EmailAddress(ErrorMessage = "Formato de email inválido.")]
    public string Email { get; set; }

    [Required(ErrorMessage = "A senha é obrigatória.")]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    [Display(Name = "Lembrar de mim")]
    public bool RememberMe { get; set; }
}

public class RegisterViewModel
{
    [Required(ErrorMessage = "O email é obrigatório.")]
    [EmailAddress(ErrorMessage = "Formato de email inválido.")]
    public string Email { get; set; }

    [Required(ErrorMessage = "A senha é obrigatória.")]
    [DataType(DataType.Password)]
    [StringLength(100, ErrorMessage = "A {0} deve ter no mínimo {2} e no máximo {1} caracteres.", MinimumLength = 8)]
    public string Password { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Confirme a senha")]
    [Compare("Password", ErrorMessage = "As senhas não coincidem. Tente novamente.")]
    public string ConfirmPassword { get; set; }
}