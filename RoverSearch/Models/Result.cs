namespace RoverSearch.Models;

public class Result:IComparable<Result>
{
    public string Title { get; set; } = string.Empty;
    public string Season { get; set; } = string.Empty;
    public string Episode { get; set; } = string.Empty;
    public string Filename { get; set; } = string.Empty;
    public int Weight { get; set; } = 0;

    public int CompareTo(Result other)
    {
        return Weight.CompareTo(other.Weight);
    }
    
}
