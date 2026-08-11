using Godot;
using Godot.Sharp.Extras;

public partial class HeartIcon   : TextureRect
{

    [Signal]
    public delegate void clickedEventHandler();
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        this.OnReady();
    }

    public bool IsChecked() {
        return Modulate == new Color("ffffffff");
    }

    public void SetCheck(bool check) {
        if (check)
            Modulate = new Color("ffffffff");
        else
            Modulate = new Color(0.4275f, 0.8902f, 0.8588f, 0.8196f); // was "d16de3db" (teal in Godot 3, pink in Godot 4)
    }

    [SignalHandler("gui_input")]
    void OnGuiInput(InputEvent inputEvent) {
        if (inputEvent is InputEventMouseButton iemb) {
            if (iemb.Pressed) {
                if (iemb.ButtonIndex == MouseButton.Left) {
                    SetCheck(!IsChecked());
                    EmitSignal("clicked");
                }
            }
        }
    }
}
