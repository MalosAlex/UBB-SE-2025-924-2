using System.ComponentModel.DataAnnotations;
namespace SteamProfileWeb.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "Username or email is required.")]
    [Display(Name = "Username or Email")]
    public string UsernameOrEmail { get; set; }
    
    [Required(ErrorMessage = "Password is required.")]
    [Display(Name = "Password")]
    public string Password { get; set; }
}