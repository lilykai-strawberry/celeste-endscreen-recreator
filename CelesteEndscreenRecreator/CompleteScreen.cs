namespace CelesteEndscreenRecreator;

public class CompleteScreen
{
    public List<Layer> Layers { get; set; } = [];
    public FloatPair Start { get; set; } = new FloatPair();
    public FloatPair Offset { get; set; } = new FloatPair();
    public FloatPair Center { get; set; } = new FloatPair();
    public FloatPair Scroll { get; set; } = new FloatPair();
}