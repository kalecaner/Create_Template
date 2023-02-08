using System.ComponentModel.DataAnnotations;

namespace Create_Template.Models
{
    public class LoginViewModel
    {
       // [Display(Name ="Kullanıcı Adı",Prompt ="Cancan")]
        [Required(ErrorMessage ="Kullanıcı Adı Zorunlu")]
        [StringLength(100, ErrorMessage ="Kullanıcı adı en  fazla 100 karakter olabilir")]
        public string UserName { get; set; }


       // [DataType(DataType.Password)]
        [Required(ErrorMessage ="Parola En az  6 karakterden oluşmalıdır")]
        [MinLength(6)]
        [MaxLength(16)]
        public string Password { get; set; }
       

       
    }
}
