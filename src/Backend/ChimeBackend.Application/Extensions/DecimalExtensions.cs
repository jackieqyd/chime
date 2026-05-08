namespace ChimeBackend.Application.Extensions;

public static class DecimalExtensions
{
    public static decimal Round(this decimal value, int decimals = 1)
    {
        return Math.Round(value, decimals);
    }
}
