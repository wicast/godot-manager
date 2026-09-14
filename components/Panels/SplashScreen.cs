using Godot;
using System;
using Godot.Sharp.Extras;
using Thread = System.Threading.Thread;

public partial class SplashScreen   : Control
{
	[NodePath] private Label VersionInfo = null;
	private Thread _thread;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.OnReady();
		VersionInfo.Text = $"Version {VERSION.GodotManager}";
		var timer = GetTree().CreateTimer(0.4f);
		timer.Connect("timeout", Callable.From(OnTimeout_LoadResources));
	}

	void OnTimeout_LoadResources()
	{
		_thread = new Thread(GDThread_Loader);
		_thread.Start();
	}

	void GDThread_Loader()
	{
		const string scenePath = "res://Scenes/SceneManager.tscn";
		ResourceLoader.LoadThreadedRequest(scenePath);
		while (true)
		{
			Thread.Sleep(100);
			var status = ResourceLoader.LoadThreadedGetStatus(scenePath);
			if (status == ResourceLoader.ThreadLoadStatus.Loaded)
			{
				var res = (PackedScene)ResourceLoader.LoadThreadedGet(scenePath);
				CallDeferred("ThreadDone", res);
				break;
			}
			else if (status == ResourceLoader.ThreadLoadStatus.Failed || status == ResourceLoader.ThreadLoadStatus.InvalidResource)
			{
				GD.PrintErr("There was an error loading.");
				break;
			}
		}
	}

	void ThreadDone(PackedScene res)
	{
		_thread.Join();

		var inst = res.Instantiate<SceneManager>();
		GetTree().CurrentScene.QueueFree();
		GetTree().CurrentScene = null;
		GetTree().Root.AddChild(inst);
		GetTree().CurrentScene = inst;
	}
}
