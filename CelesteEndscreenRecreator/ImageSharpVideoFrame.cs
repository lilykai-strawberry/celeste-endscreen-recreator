using FFMpegCore.Pipes;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace CelesteEndscreenRecreator;

public class ImageSharpVideoFrame(Image<Rgba32> image) : IVideoFrame
{
    public int Width { get; private set; } = image.Width;
    public int Height { get; private set; } = image.Height;
    public string Format { get; private set; } = "bgr0";

    public void Serialize(Stream stream)
    {
        var pixelData = new byte[Width * Height * 4];
        image.CloneAs<Bgra32>().CopyPixelDataTo(pixelData);
        stream.Write(pixelData, 0, pixelData.Length);
    }
    public async Task SerializeAsync(Stream stream, CancellationToken cancellationToken)
    {
        var pixelData = new byte[Width * Height * 4];
        image.CloneAs<Bgra32>().CopyPixelDataTo(pixelData);
        await stream.WriteAsync(pixelData, 0, pixelData.Length, cancellationToken);
    }
}