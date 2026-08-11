using Godot;
using Godot.Collections;
using DateTime = System.DateTime;

namespace Github {
	public partial class RateLimit   : GodotObject {
		public int Limit;
		public int Remaining;
		public int Used;
		public DateTime Reset;
	}
}