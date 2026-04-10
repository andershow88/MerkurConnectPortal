using System.ComponentModel.DataAnnotations;

namespace MerkurConnectPortal.Web.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "Bitte Benutzername eingeben")]
    [Display(Name = "Benutzername")]
    public string Benutzername { get; set; } = string.Empty;

    [Required(ErrorMessage = "Bitte Passwort eingeben")]
    [DataType(DataType.Password)]
    [Display(Name = "Passwort")]
    public string Passwort { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}
