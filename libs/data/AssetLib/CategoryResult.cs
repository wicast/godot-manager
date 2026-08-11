using Godot;
using Godot.Collections;
using Newtonsoft.Json;

namespace AssetLib {
	[JsonObject(MemberSerialization.OptIn)]
	public partial class CategoryResult   : GodotObject {
		[JsonProperty] public string Id;
		[JsonProperty] public string Name;
		[JsonProperty] public string Type;
	}
}