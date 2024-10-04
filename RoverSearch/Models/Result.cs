namespace RoverSearch.Models;

public class Result
{
    public string Title { get; set; } = string.Empty;
    public string Season { get; set; } = string.Empty;
    public string Episode { get; set; } = string.Empty;
    public string Filename { get; set; } = string.Empty;
    private int Weight { get; set; } = 0;
}
