using Godot;

public static partial class VERSION {
	public static string GodotManager =>
		ProjectSettings.GetSetting("application/config/version", "0.0.0").AsString();

	public static string Channel = "dev";
	public static string GodotSharpExtras = "0.3.4";
	public static string NewtonsoftJSON = "13.0.1";
	public static string SystemIOCompression = "4.3.0";
	public static string ImageSharp = "1.0.4";
}
