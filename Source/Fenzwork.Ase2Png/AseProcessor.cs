using AsepriteDotNet.Aseprite;
using AsepriteDotNet.Aseprite.Types;
using AsepriteDotNet.Common;
using AsepriteDotNet.IO;

namespace Fenzwork.Ase2Png;

public static class AseProcessor
{
    public static void Dummy() { }

    public static void Process(string input, string output)
    {
        AsepriteFile file = AsepriteFileLoader.FromFile(input);

        var layersCollection = new List<string>(file.Layers.Length);
        for (int l = 0; l < file.Layers.Length; l++)
            if (!file.Layers[l].Name.StartsWith("_"))
                layersCollection.Add(file.Layers[l].Name);

        int frameWidth = file.CanvasWidth;
        int imageWidth = file.CanvasWidth * file.FrameCount;
        int imageHeight = file.CanvasHeight;
        var imagePixels = new Rgba32[imageWidth * imageHeight];
        for (int f = 0; f < file.FrameCount; f++)
        {
            for (int l = 0; l < file.Layers.Length; l++)
            {
                ReadOnlySpan<Rgba32> frame = file.Frames[f].FlattenFrame(layersCollection);

                for (int p = 0; p < frame.Length; p++)
                {
                    int px = (p % frameWidth) + f * frameWidth;
                    int py = (p / frameWidth);
                    int index = py * imageWidth + px;
                    imagePixels[index] = frame[p];
                }

            }

        }

        // Save the png
        PngWriter.SaveTo(output, imageWidth, imageHeight, imagePixels);
        // Make both input and output file same write time (for incremental build purpose)
        File.SetLastWriteTime(output, File.GetLastWriteTime(output));

    }
}