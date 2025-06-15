using TicketSprint.DTOs;
using TicketSprint.Model;
using TicketSprint.Repositories;

namespace TicketSprint.Services;

public class TicketService : ITicketService
{
    private readonly ITicketRepository _repository;
    private readonly IUserRepository _userRepository;

    public TicketService(ITicketRepository repository, IUserRepository userRepository)
    {
        _repository = repository;
        _userRepository = userRepository;
    }

    public async Task<IEnumerable<TicketDTO>> GetAllAsync()
    {
        var tickets = await _repository.GetAllAsync();
        return tickets.Select(MapToDTO);
    }

    public async Task<TicketDTO?> GetByIdAsync(int id)
    {
        var ticket = await _repository.GetByIdAsync(id);
        return ticket == null ? null : MapToDTO(ticket);
    }

    public async Task<IEnumerable<TicketDTO>> GetByEventSectorIdAsync(int eventSectorId)
    {
        var tickets = await _repository.GetByEventSectorIdAsync(eventSectorId);
        return tickets.Select(MapToDTO);
    }

    public async Task<IEnumerable<TicketDTO>> GetByEmailAsync(string email)
    {
        var tickets = await _repository.GetByEmailAsync(email);
        return tickets.Select(MapToDTO);
    }
    

    public async Task<TicketDTO> CreateAsync(CreateTicketDTO dto)
    {
        var code = Guid.NewGuid().ToString();
        
        var ticket = new Ticket
        {
            EventSectorId = dto.EventSectorId,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Age = dto.Age,
            Email = dto.Email,
            Row = dto.Row,
            Seat = dto.Seat,
            PurchaseDate = DateTime.UtcNow,
            QRCode = code,
            IsScanned = false
        };

        await _repository.AddAsync(ticket);
        

        return MapToDTO(ticket);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var ticket = await _repository.GetByIdAsync(id);
        if (ticket == null) return false;

        await _repository.DeleteAsync(id);
        return true;
    }

    private static TicketDTO MapToDTO(Ticket t)
    {
        return new TicketDTO
        {
            TicketId = t.TicketId,
            EventSectorId = t.EventSectorId,
            FirstName = t.FirstName,
            LastName = t.LastName,
            Age = t.Age,
            Email = t.Email,
            Row = t.Row,
            Seat = t.Seat,
            PurchaseDate = t.PurchaseDate,
            QRCode = t.QRCode,
            IsScanned = t.IsScanned

        };
    }
    
    public async Task<Ticket?> GetFullTicketModelAsync(int ticketId)
    {
        return await _repository.GetFullTicketModelAsync(ticketId);
    }
    
    public async Task<Ticket?> GetByQRCodeAsync(string qrCode)
    {
        return await _repository.GetByQRCodeAsync(qrCode);
    }
    
    public async Task UpdateAsync(Ticket ticket)
    {
        await _repository.UpdateAsync(ticket);
    }
    
    public async Task<List<TicketWithRoleDTO>> GetOccupiedSeatsWithRoleForAdmin(int eventSectorId)
    {
        var tickets = await _repository.GetByEventSectorIdAsync(eventSectorId);
        var result = new List<TicketWithRoleDTO>();

        foreach (var ticket in tickets)
        {
            var user = await _userRepository.GetByEmailAsync(ticket.Email);

            var role = user?.Role ?? "client"; // dacă nu există user, fallback la "Client"

            result.Add(new TicketWithRoleDTO
            {
                TicketId = ticket.TicketId,
                Row = ticket.Row,
                Seat = ticket.Seat,
                FirstName = ticket.FirstName,
                LastName = ticket.LastName,
                Email = ticket.Email,
                Role = role
            });
        }

        return result;
    }




}