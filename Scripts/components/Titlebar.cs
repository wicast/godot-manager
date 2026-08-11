using Godot;
using Godot.Sharp.Extras;

public partial class Titlebar   : Control
{
    private bool moving = false;
    private bool following = false;
    private Vector2 start_pos = Vector2.Zero;
    public override void _Ready()
    {
        this.OnReady();
    }

    [SignalHandler("gui_input")]
    void OnTitlebar_GuiInput(InputEvent inputEvent) {
        if (inputEvent is InputEventMouseButton iemb) {
            if (iemb.ButtonIndex == MouseButton.Left) {
                following = !following;
                start_pos = GetLocalMousePosition();
                return;
            }
        }

        if (following && !moving) {
            var movement = GetLocalMousePosition() - start_pos;
            if (movement == Vector2.Zero) return;
            moving = true;
            GetWindow().Position += (Vector2I)movement;
            moving = false;
        }
    }
}
