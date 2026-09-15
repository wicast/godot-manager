using System;
using System.IO;
using Godot;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Gif;
using SImage = SixLabors.ImageSharp.Image;

// Godot.AnimatedTexture is obsolete and broken in current versions; drive
// TextureRect frames from a child Node instead.
public partial class GifAnimatedTexture : Node
{
	private const int MaxFrames = 256;

	private TextureRect _target;
	private ImageTexture[] _frames = Array.Empty<ImageTexture>();
	private float[] _delays = Array.Empty<float>();
	private int _index;
	private float _elapsed;

	public int FrameCount => _frames.Length;

	public static GifAnimatedTexture Attach(TextureRect target, string file)
	{
		if (target == null || string.IsNullOrEmpty(file))
			return null;

		foreach (Node child in target.GetChildren())
		{
			if (child is GifAnimatedTexture old)
				old.QueueFree();
		}

		var anim = new GifAnimatedTexture();
		anim._target = target;
		if (!anim.Load(file))
		{
			anim.Free();
			return null;
		}

		target.AddChild(anim);
		target.Texture = anim._frames.Length > 0 ? anim._frames[0] : null;
		return anim;
	}

	private bool Load(string file)
	{
		string osPath = file.GetOSDir();
		if (!File.Exists(osPath))
			return false;

		using SImage gif = SImage.Load(osPath);
		int count = Math.Min(gif.Frames.Count, MaxFrames);
		if (count == 0)
			return false;

		_frames = new ImageTexture[count];
		_delays = new float[count];

		for (int i = 0; i < count; i++)
		{
			GifFrameMetadata meta = gif.Frames[i].Metadata.GetGifMetadata();
			// GIF delay is in 1/100s; 0 is commonly treated as 10cs.
			_delays[i] = meta.FrameDelay <= 0 ? 0.1f : meta.FrameDelay * 0.01f;

			using SImage iframe = gif.Frames.CloneFrame(i);
			using MemoryStream ms = new MemoryStream();
			iframe.SaveAsPng(ms);
			ms.Position = 0;
			Godot.Image img = new Godot.Image();
			img.LoadPngFromBuffer(ms.ToArray());
			_frames[i] = ImageTexture.CreateFromImage(img);
		}

		_index = 0;
		_elapsed = 0f;
		return true;
	}

	public override void _Process(double delta)
	{
		if (_frames.Length < 2 || _target == null)
			return;
		if (!GodotObject.IsInstanceValid(_target) || !_target.IsInsideTree())
			return;

		_elapsed += (float)delta;
		bool changed = false;
		int guard = 0;
		while (_elapsed >= _delays[_index] && guard++ < _frames.Length)
		{
			_elapsed -= _delays[_index];
			_index = (_index + 1) % _frames.Length;
			changed = true;
		}

		if (changed && _target.Texture != _frames[_index])
			_target.Texture = _frames[_index];
	}

	public override void _ExitTree()
	{
		_frames = Array.Empty<ImageTexture>();
		_delays = Array.Empty<float>();
		_target = null;
	}
}
