using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace CelesteEndscreenRecreator;

public class Layer
{
    public string Type { get; set; } = "layer";
    public List<string> Images { get; set; } = [];
    public int FrameRate { get; set; } = 6;
    public FloatPair Position { get; set; } = new FloatPair();
    public FloatPair Scroll { get; set; }  = new FloatPair();
    public float Scale { get; set; } = 1.0f;
    public float Alpha { get; set; } = 1.0f;
    public FloatPair Speed { get; set; } = new FloatPair();
    public Dictionary<string, Image<Rgba32>> ImageDictionary { get; set; } = new Dictionary<string, Image<Rgba32>>();
}