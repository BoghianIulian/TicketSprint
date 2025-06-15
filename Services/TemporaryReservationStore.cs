using TicketSprint.DTOs;

namespace TicketSprint.Services;

public static class TemporaryReservationStore
{
    private static readonly List<TemporaryReservation> _reservations = new();

    public static void Add(TemporaryReservation reservation)
    {
        _reservations.Add(reservation);
    }

    public static void CleanExpired()
    {
        var now = DateTime.UtcNow;
        _reservations.RemoveAll(r => r.ExpiresAt < now);
    }

    public static List<TemporaryReservation> GetValidForSector(int eventSectorId)
    {
        CleanExpired();
        return _reservations
            .Where(r => r.EventSectorId == eventSectorId)
            .ToList();
    }
    
    public static List<TemporaryReservation> GetUserReservations(string userId) =>
        _reservations.Where(r => r.UserId == userId && r.ExpiresAt > DateTime.UtcNow).ToList();

    public static List<TemporaryReservation> GetCartReservations(string cartId) =>
        _reservations.Where(r => r.CartId == cartId && r.ExpiresAt > DateTime.UtcNow).ToList();
    
    public static void RemoveReservation(string userId, string? cartId, int eventSectorId, int row, int seat)
    {
        _reservations.RemoveAll(r =>
            r.EventSectorId == eventSectorId &&
            r.Row == row &&
            r.Seat == seat &&
            (r.UserId == userId || (!string.IsNullOrEmpty(cartId) && r.CartId == cartId))
        );
    }

}