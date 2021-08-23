using System.ComponentModel.DataAnnotations;

namespace Caja.Models
{
    public class LoginViewModel
    {
        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Usuario")]
        public string Usuario { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; }

        [Display(Name = "Recordar")]
        public bool RememberMe { get; set; }

        [Display(Name = "Usuario y/o contraseña incorrectos")]
        public bool Mensaje { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña nueva")]
        public string PasswordNew { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Repetir contraseña")]
        public string PasswordNew2 { get; set; }

        public bool RecuperarClave { get; set; }
    }
    public class Conexion
    {
        public string Mensaje { get; set; }
        public bool Estado { get; set; }
        public string Url { get; set; }
        public int Intentos { get; set; }
        public string Usuario { get; internal set; }
        public string Clave { get; internal set; }
        public bool RecuperarClave{get; set;}
    }

    public class RecuperarClave
    {
        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Usuario")]
        public string Usuario { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña actual")]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña nueva")]
        public string PasswordNew { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Repetir contraseña")]
        public string PasswordNew2 { get; set; }
    }
}