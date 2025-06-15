using System.ComponentModel.DataAnnotations;



namespace TicketSprint.DTOs
{
    public class CreateUserDTO
    {
        [Required(ErrorMessage = "Prenumele este obligatoriu.")]
        [StringLength(50, ErrorMessage = "Prenumele nu poate depăși 50 de caractere.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Numele este obligatoriu.")]
        [StringLength(50, ErrorMessage = "Numele nu poate depăși 50 de caractere.")]
        public string LastName { get; set; }
        
        [Range(15, 120, ErrorMessage = "Vârsta trebuie să fie între 15 și 120 de ani.")]
        public int Age { get; set; }

        [Required(ErrorMessage = "Parola este obligatorie.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Parola trebuie să aibă cel puțin 6 caractere.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Email-ul este obligatoriu.")]
        [EmailAddress(ErrorMessage = "Email invalid.")]
        public string Email { get; set; }
    }
}