using System;
using System.IO;
using Godot;
using SixLabors.ImageSharp;
using SImage = SixLabors.ImageSharp.Image;

public static partial class GifTexture
{
	public static Texture2D Load(string file)
	{
		SImage gif = SImage.Load(file.GetOSDir());
		using (MemoryStream ms = new MemoryStream())
		{
			SImage iframe = gif.Frames.CloneFrame(0);
			iframe.SaveAsPng(ms);
			ms.Position = 0;
			Godot.Image img = new Godot.Image();
			img.LoadPngFromBuffer(ms.ToArray());
			return ImageTexture.CreateFromImage(img);
		}
	}

	public static void LoadInto(TextureRect target, string file)
	{
		if (target == null || string.IsNullOrEmpty(file))
			return;

		if (file.EndsWith(".gif", StringComparison.OrdinalIgnoreCase))
		{
			if (GifAnimatedTexture.Attach(target, file) != null)
				return;
		}

		target.Texture = Load(file);
	}
}
