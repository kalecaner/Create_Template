using System.ComponentModel.DataAnnotations;

namespace Create_Template.Models
{
    public class RegisterViewModel:LoginViewModel
    {
        [Required(ErrorMessage = "Parola En az  6 karakterden oluşmalıdır")]
        [MinLength(6)]
        [MaxLength(16)]
        [Compare(nameof(LoginViewModel.Password))]
        public string RePassword { get; set; }

        

    }
}
