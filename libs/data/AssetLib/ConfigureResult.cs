using Godot;
using Godot.Collections;
using Newtonsoft.Json;

namespace AssetLib {
	[JsonObject(MemberSerialization.OptIn)]
	public partial class ConfigureResult   : GodotObject {
		[JsonProperty] public Array<CategoryResult> Categories;
	}
}