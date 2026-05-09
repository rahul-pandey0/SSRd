namespace SSRd.Configuration;

public class RdSettings
{
    public bool AutoAuthorize { get; set; }
    public List<string> AllowedDepositTypes { get; set; } = new();
    public Dictionary<int, double> InterestRates { get; set; } = new();
}
