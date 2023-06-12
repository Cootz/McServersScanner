using System.Text.Json.Serialization;
using Realms;

namespace McServersScanner.Core.IO.Database.Models;

public class Version : EmbeddedObject, IEquatable<Version>
{
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;

    [JsonPropertyName("protocol")] public int? Protocol { get; set; } = null;

    public override string ToString() => $"Name: {Name}, protocol {Protocol}";

    public bool Equals(Version? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Name == other.Name && Protocol == other.Protocol;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj.GetType() == this.GetType() && Equals((Version)obj);
    }

    public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), Name, Protocol);
}