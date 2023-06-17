namespace McServersScanner.Core.Utils;

public static class ParsingHelper
{
    /// <summary>
    /// Convert sentences like 10K, 10M, 10G to it's byte representation
    /// </summary>
    /// <param name="formattedNumberOfBytes">Sentence to convert</param>
    /// <returns>Number of bytes given in <paramref name="formattedNumberOfBytes"/></returns>
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