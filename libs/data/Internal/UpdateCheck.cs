using Godot;
using Godot.Sharp.Extras;
using Godot.Collections;
using Newtonsoft.Json;
using DateTime = System.DateTime;

[JsonObject(MemberSerialization.OptIn)]
public partial class UpdateCheck   : GodotObject
{
	[JsonProperty] public DateTime LastCheck { get; set; }
}