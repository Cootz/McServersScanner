namespace McServersScanner.Core.Utils;

public static class ParsingHelper
{
    public static int ConvertToNumberOfBytes(string formattedNumberOfBytes)
    {
        char lastSymbol = formattedNumberOfBytes.Last();

        if (!char.IsLetter(lastSymbol))
            return Convert.ToInt32(formattedNumberOfBytes);

        int bodyDigit = Convert.ToInt32(formattedNumberOfBytes[..^1]);

        return lastSymbol switch
        {
            'K' => bodyDigit << 10,
            'M' => bodyDigit << 20,
            'G' => bodyDigit << 30,
            _ => throw new ArgumentOutOfRangeException($"Unknown metric symbol :{lastSymbol}")
        };
    }
}