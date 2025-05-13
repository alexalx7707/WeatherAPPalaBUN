namespace WeatherAppNoi.Models
{
    public class Location
    {
        public int Id { get; set; }

        public string City { get; set; } = string.Empty;

        public string Region { get; set; } = string.Empty;

        public string Country { get; set; } = string.Empty;

        public float Latitude { get; set; }

        public float Longitude { get; set; }

        // Navigation property for FavoriteLocation
        public FavoriteLocation? FavoriteLocation { get; set; }
    }
}