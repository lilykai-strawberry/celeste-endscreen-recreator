using FFMpegCore;
using FFMpegCore.Pipes;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.CommandLine;

namespace CelesteEndscreenRecreator;

public static class Program
{
    private static int _progress;
    public static async Task<int> Main(string[] args)
    {
        Option<FileInfo> metaYamlOption = new("--meta-yaml")
        {
            Description = "Path to .meta.yaml file"
        };
        Option<FileInfo> imageAtlasOption = new("--image-atlas")
        {
            Description = "Path to an image atlas"
        };
        Option<FileInfo> outputOption = new("--output")
        {
            Description = "Output path"
        };
        Option<int> frameCountOption = new("--frame-count")
        {
            Description = "Number of frames"
        };
        RootCommand rootCommand = new("Program to recreate Celeste endscreens from game files")
        {
            metaYamlOption,
            imageAtlasOption,
            outputOption,
            frameCountOption
        };
        var parseResult = rootCommand.Parse(args);
        if (parseResult.Errors.Count > 0)
        {
            foreach (var error in parseResult.Errors)
            {
                Console.WriteLine(error);
            }
            return 1;
        }
        var metaYaml = parseResult.GetValue(metaYamlOption);
        var imageAtlas = parseResult.GetValue(imageAtlasOption);
        var output = parseResult.GetValue(outputOption);
        var frameCount = parseResult.GetValue(frameCountOption);
        if (metaYaml == null || imageAtlas == null || output == null)
        {
            Console.WriteLine("Bad Option");
            return 1;
        }
        var task = Generate(metaYaml, imageAtlas, output, frameCount);
        var display = Task.Run(async () =>
            {
                while (!task.IsCompleted)
                {
                    var line = _progress + "/" + frameCount;
                    Console.Write($"\r{line.PadRight(Console.WindowWidth - 1)}");
                    await Task.Delay(100);
                }
            }
        );
        await task;
        await display;
        return 0;
    }

    private static async Task Generate(FileInfo metaYaml, FileInfo imageAtlas, FileInfo output, int frameCount)
    {
        using var reader = new StreamReader(metaYaml.FullName);
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(PascalCaseNamingConvention.Instance).IgnoreUnmatchedProperties().Build();
        var completeScreen = deserializer.Deserialize<Dictionary<string, CompleteScreen>>(reader)["CompleteScreen"];
        var layers = completeScreen.Layers;

        foreach (var layer in layers)
        {
            if (layer.Type != "layer") continue;
            foreach (var imagePath in layer.Images.Distinct())
            {
                var fullPath = imageAtlas.FullName + "/" + imagePath + ".png";
                var image = Image.Load<Rgba32>(fullPath);
                var width = (int)(image.Width * layer.Scale);
                var height = (int)(image.Height * layer.Scale);
                image.Mutate(x => x.Opacity(layer.Alpha).Resize(width, height));
                layer.ImageDictionary.Add(imagePath, image);
            }
        }


        var videoFramesSource = new RawVideoPipeSource(CreateFrames(frameCount))
        {
            FrameRate = 60 //set source frame rate
        };
        await FFMpegArguments
            .FromPipeInput(videoFramesSource)
            .OutputToFile(output.FullName, true, options => options
                .WithVideoCodec("libx264")
                .WithConstantRateFactor(20)
                .WithFastStart()
                .ForcePixelFormat("yuv420p")
                .ForceFormat("mp4"))
            .ProcessAsynchronously();
        return;

        IEnumerable<IVideoFrame> CreateFrames(int count)
        {
            for (var i = 0; i < count; i++)
            {
                _progress = i;
                yield return GetNextFrame(i);
            }
        }

        ImageSharpVideoFrame GetNextFrame(int i)
        {
            var frame = new Image<Rgba32>(1920, 1080);

            foreach (var layer in layers)
            {
                if (layer.Type != "layer") continue;
                var index = i / (60 / layer.FrameRate) % layer.Images.Count;
                var image = layer.ImageDictionary[layer.Images[index]];
                var position = completeScreen.Offset + layer.Position;
                position += layer.Speed * i / 60;
                while (position.X > 1920)
                {
                    position.X -= image.Width;
                }

                do
                {
                    frame.Mutate(x =>
                        x.DrawImage(image, new Point((int)position.X, (int)position.Y), new GraphicsOptions()));
                    position.X -= image.Width;
                } while (layer.Speed.X != 0 && position.X + image.Width >= 0);
            }

            return new ImageSharpVideoFrame(frame);
        }
    }
}