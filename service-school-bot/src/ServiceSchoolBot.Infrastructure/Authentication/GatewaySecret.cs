using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ServiceSchoolBot.Infrastructure.Authentication;

[JsonConverter(typeof(GatewaySecret.SerializationConverter))]
public sealed class GatewaySecret : IDisposable
{
    private readonly byte[] buffer;
    private readonly object sync = new();
    private bool disposed;

    internal GatewaySecret(string value)
    {
        buffer = Encoding.UTF8.GetBytes(value);
    }

    public int ByteLength
    {
        get
        {
            lock (sync)
            {
                ObjectDisposedException.ThrowIf(disposed, this);
                return buffer.Length;
            }
        }
    }

    public int CopyTo(Span<byte> destination)
    {
        lock (sync)
        {
            ObjectDisposedException.ThrowIf(disposed, this);
            if (destination.Length < buffer.Length)
                throw new ArgumentException("Destination is too small.", nameof(destination));
            buffer.AsSpan().CopyTo(destination);
            return buffer.Length;
        }
    }

    public void Dispose()
    {
        lock (sync)
        {
            if (disposed) return;
            CryptographicOperations.ZeroMemory(buffer);
            disposed = true;
        }
    }

    public override string ToString() => "[REDACTED]";

    private sealed class SerializationConverter : JsonConverter<GatewaySecret>
    {
        public SerializationConverter() { }

        public override bool HandleNull => true;

        public override GatewaySecret Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => throw new NotSupportedException("Gateway secrets cannot be deserialized.");

        public override void Write(Utf8JsonWriter writer, GatewaySecret value, JsonSerializerOptions options)
            => throw new NotSupportedException("Gateway secrets cannot be serialized.");
    }
}
