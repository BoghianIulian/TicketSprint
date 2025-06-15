namespace TicketSprint.DTOs;

public class FilterOptionsDTO
{
    public List<string> Sports { get; set; }
    public List<string> Cities { get; set; }
    public List<LocationOptionDTO> Locations { get; set; }
}

public class LocationOptionDTO
{
    public string LocationName { get; set; }
    public string City { get; set; }
}