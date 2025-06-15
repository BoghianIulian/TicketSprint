using Microsoft.AspNetCore.Mvc;
using TicketSprint.DTOs;
using TicketSprint.Model;
using TicketSprint.Services;
using TicketSprint.Utils;

namespace TicketSprint.Controllers;


[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UserController> _logger;
    private readonly IConfiguration _config;

    
    public UserController(IUserService userService, ILogger<UserController> logger,IConfiguration config)
    {
        _userService = userService;
        _logger = logger; 
        _config = config;
    }

    [HttpPost("register")]
    [EndpointSummary("Register")]
    public async Task<IActionResult> Register([FromBody] CreateUserDTO createUserDto)
    {
        
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        
        var existingUserByEmail = await _userService.GetUserByEmailAsync(createUserDto.Email);
        if (existingUserByEmail != null)
        {
            return Conflict("Există deja un cont cu acest email.");
        }
        

        // Creare utilizator
        var user = new User
        {
            FirstName = createUserDto.FirstName,
            LastName = createUserDto.LastName,
            Age = createUserDto.Age,
            Email = createUserDto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(createUserDto.Password), 
            Role = "client"
        };
        
        

        

        var createdUser = await _userService.CreateUserAsync(user);

        if (createdUser == null)
        {
            _logger.LogError("Crearea utilizatorului a eșuat.");
            return StatusCode(500, "Nu s-a reușit crearea utilizatorului.");
        }

        

        // Răspuns pozitiv cu status 201 (Created)
        return CreatedAtAction(nameof(Register), new { id = createdUser.UserId }, createdUser);
    }
    
    [HttpPost("register-admin")]
    [EndpointSummary("Register Admin")]
    public async Task<IActionResult> RegisterAdmin([FromBody] CreateUserDTO createUserDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var existingUser = await _userService.GetUserByEmailAsync(createUserDto.Email);
        if (existingUser != null)
            return Conflict("Există deja un cont cu acest email.");

        var user = new User
        {
            FirstName = createUserDto.FirstName,
            LastName = createUserDto.LastName,
            Age = createUserDto.Age,
            Email = createUserDto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(createUserDto.Password),
            Role = "administrator"
        };

        var created = await _userService.CreateUserAsync(user);
        if (created == null)
        {
            _logger.LogError("Crearea utilizatorului administrator a eșuat.");
            return StatusCode(500, "Eroare la creare.");
        }

        return CreatedAtAction(nameof(RegisterAdmin), new { id = created.UserId }, created);
    }
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDTO>>> GetAllUsers()
    {
        var users = await _userService.GetAllUsersAsync();
        var result = users.Select(u => new UserDTO
        {
            UserId = u.UserId,
            FirstName = u.FirstName,
            LastName = u.LastName,
            Age = u.Age,
            Email = u.Email,
            Role = u.Role
        });
        return Ok(result);
    }
    
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDTO loginDto)
    {
        var user = await _userService.GetUserByEmailAsync(loginDto.Email);

        if (user == null)
        {
            return Unauthorized("Emailul este incorect.");
        }

        var passwordValid = BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash);

        if (!passwordValid)
        {
            return Unauthorized("Parola este incorectă.");
        }

        var token = JwtTokenGenerator.GenerateToken(user, _config); 
        
       

        return Ok(new {
            token,
            role = user.Role 
        });
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user == null) return NotFound();

        var result = new UserDTO
        {
            UserId = user.UserId,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Age = user.Age,
            Email = user.Email,
            Role = user.Role
        };

        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDTO dto)
    {
        if (id != dto.UserId)
            return BadRequest("ID din URL nu se potrivește cu cel din corpul cererii.");

        var user = await _userService.GetByIdAsync(id);
        if (user == null)
            return NotFound("Utilizatorul nu există.");

        // Actualizăm doar câmpurile permise
        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        user.Age = dto.Age;

        await _userService.UpdateUserAsync(user);

        return NoContent();
    }







}