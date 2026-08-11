using Godot;
using Godot.Collections;
using Newtonsoft.Json;

namespace AssetLib {
	[JsonObject(MemberSerialization.OptIn)]
	public partial class Preview   : GodotObject {
		[JsonProperty] public string PreviewId;
		[JsonProperty] public string Type;
		[JsonProperty] public string Link;
		[JsonProperty] public string Thumbnail;
	}
}