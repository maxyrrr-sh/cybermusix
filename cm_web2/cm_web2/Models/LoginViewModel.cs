using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
namespace cm_web2.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Ім'я користувача обов'язкове")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Пароль обов'язковий")]
        public string Password { get; set; }
    }

}
