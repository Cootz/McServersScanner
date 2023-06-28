using System.Text.Json;

namespace McServersScanner.Core.Utils;

/// <summary>
/// Helps with json conversion
/// </summary>
public static class JsonHelper
{
    /// <summary>
    /// Remove everything except json data from string
    /// </summary>
    /// <param name="dirtyJson">String with json data in it</param>
    /// <returns>Clean json string ready to convert</returns>
    public static string ConvertToJsonString(string dirtyJson) => ConvertToJsonString(dirtyJson.AsSpan());

    public static string ConvertToJsonString(ReadOnlySpan<char> dirtyJson)
    {
        int balance = 0;
        int i = 0;

        do
        {
            char currentChar = dirtyJson[i];

            if (currentChar == '{')
                balance++;
            else if (currentChar == '}') balance--;

            i++;
        } while (balance > 0);

        return dirtyJson[..i].ToString();
    }

    public static JsonElement? Get(this JsonElement element, string name) =>
        element.ValueKind is JsonValueKind.Object
        && element.TryGetProperty(name, out JsonElement value)
            ? value
            : null;

    public static JsonElement? Get(this JsonElement element, int index)
    {
        if (element.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
            return null;
        // Throw if index < 0
        return index < element.GetArrayLength() ? element[index] : null;
    }
}