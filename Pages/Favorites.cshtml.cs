using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TicketSprint.DTOs;

namespace TicketSprint.Pages;

public class FavoritesModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public Dictionary<string, List<FavoriteDTO>> GroupedFavorites { get; set; } = new();
    public Dictionary<string, List<ParticipantDTO>> GroupedSuggestions { get; set; } = new();
    public Dictionary<string, List<ParticipantDTO>> GroupedFrequentSuggestions { get; set; } = new();
    public Dictionary<string, List<ParticipantDTO>> GroupedOtherSuggestions { get; set; } = new();


    public FavoritesModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
            return Unauthorized();

        int userId = int.Parse(userIdClaim.Value);
        var client = _httpClientFactory.CreateClient();
        client.BaseAddress = new Uri("http://localhost:5182/"); // ajustează dacă folosești alt port

        var token = HttpContext.Request.Cookies["jwt"];
        if (!string.IsNullOrEmpty(token))
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Favorite DTOs
        var favResponse = await client.GetAsync($"api/Favorite/user/{userId}");
        if (!favResponse.IsSuccessStatusCode)
            return Page();

        var favJson = await favResponse.Content.ReadAsStringAsync();
        var favorites = JsonSerializer.Deserialize<List<FavoriteDTO>>(favJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();

        GroupedFavorites = favorites
            .GroupBy(f => f.SportType)
            .ToDictionary(g => g.Key, g => g.ToList());

        // Sugestii din backend (participantii care nu sunt in favorite)
        var sugResponse = await client.GetAsync($"api/Participant/suggestions/{userId}");
        if (!sugResponse.IsSuccessStatusCode)
            return Page();

        var sugJson = await sugResponse.Content.ReadAsStringAsync();
        var suggestions = JsonSerializer.Deserialize<List<ParticipantDTO>>(sugJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();

        // Ia participanți frecvenți de la backend
        var freqResponse = await client.GetAsync($"api/Favorite/frequent-participants/{userId}");
        if (!freqResponse.IsSuccessStatusCode)
            return Page();

        var freqJson = await freqResponse.Content.ReadAsStringAsync();
        var freqDict = JsonSerializer.Deserialize<Dictionary<int, int>>(freqJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
        var frequentIds = freqDict.Keys.ToList();


        var frequent = suggestions.Where(p => frequentIds.Contains(p.ParticipantId)).ToList();
        var others = suggestions.Where(p => !frequentIds.Contains(p.ParticipantId)).ToList();

        GroupedFrequentSuggestions = frequent
            .GroupBy(p => p.SportType)
            .ToDictionary(g => g.Key, g => g.ToList());

        GroupedOtherSuggestions = others
            .GroupBy(p => p.SportType)
            .ToDictionary(g => g.Key, g => g.ToList());


        return Page();
    }
}
