using Godot;
using Godot.Collections;
using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
public partial class AssetProject   : GodotObject {
	[JsonProperty] public AssetLib.Asset Asset;
	[JsonProperty] public string Location;
}