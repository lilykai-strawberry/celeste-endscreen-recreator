using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace CelesteEndscreenRecreator;

public record struct FloatPair(float X, float Y) : IYamlConvertible
{
    private FloatPair(List<float> values) : this(values.First(), values.Last())
    {
    }

    public FloatPair() : this(0, 0)
    {
    }

    public void Write(IEmitter emitter, ObjectSerializer nestedObjectSerializer)
    {
        throw new NotImplementedException();
    }

    void IYamlConvertible.Read(IParser parser, Type expectedType, ObjectDeserializer nestedObjectDeserializer)
    {
        var values = nestedObjectDeserializer(typeof(List<float>)) as List<float> ?? throw new NullReferenceException();
        this = new FloatPair(values);
    }

    public static FloatPair operator+(FloatPair a, FloatPair b)
    {
        return new FloatPair(a.X + b.X, a.Y + b.Y);
    }

    public static FloatPair operator /(FloatPair a, float b)
    {
        return new FloatPair(a.X / b, a.Y / b);
    }
    public static FloatPair operator *(FloatPair a, float b)
    {
        return new FloatPair(a.X * b, a.Y * b);
    }
}