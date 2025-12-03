namespace Application.Workflows.ReWoo.Dto;

public record HotelSearchResultDto
{
    public string ArtifactKey { get; init; }
    public List<HotelOptionDto> Results { get; init; }
}

public record HotelOptionDto
{
    public string HotelName { get; init; }
    public string Address { get; init; }
    public DateTime CheckIn { get; init; }
    public DateTime CheckOut { get; init; }
    public decimal Rating { get; init; }
    public HotelPriceDto PricePerNight { get; init; }
    public HotelPriceDto TotalPrice { get; init; }
}

public class HotelPriceDto
{
    public decimal Amount { get; set; }
    public string Currency { get; set; }
}
