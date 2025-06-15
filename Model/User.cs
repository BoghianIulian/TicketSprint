using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TicketSprint.Model;


public class User
{
   
    public int UserId { get; set; }
    
    
    public string FirstName { get; set; } = string.Empty;
    
    public string LastName { get; set; } = string.Empty;
    
    public int Age { get; set; }
    
    public string PasswordHash { get; set; }
    
    public string Email { get; set; }
    
    public string Role { get; set; }
    
    public ICollection<Favorite> Favorites { get; set; }
    
    
}