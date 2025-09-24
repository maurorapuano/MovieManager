using System.Collections.Generic;
using System.Text.Json.Serialization;

public class StarWarsFilmResponse
{
    [JsonPropertyName("result")]
    public List<StarWarsFilmResult> Result { get; set; }
}

public class StarWarsFilmResult
{
    [JsonPropertyName("properties")]
    public StarWarsFilm Properties { get; set; }
}

public class StarWarsFilm
{
    [JsonPropertyName("title")]
    public string Title { get; set; }

    [JsonPropertyName("director")]
    public string Director { get; set; }

    [JsonPropertyName("opening_crawl")]
    public string Description { get; set; }

    [JsonPropertyName("release_date")]
    public string ReleaseDate { get; set; }
}