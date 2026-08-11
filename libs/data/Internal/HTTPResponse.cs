using Godot;
using Godot.Collections;

public partial class HTTPResponse   : GodotObject {
	public int ResponseCode;
	public Dictionary Headers;
	public byte[] BodyRaw;
	public string Body;
	public bool Cancelled;
}
