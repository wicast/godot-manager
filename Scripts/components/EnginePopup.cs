using Godot;
using System;
using Godot.Sharp.Extras;

public partial class EnginePopup   : PopupMenu
{
    public GodotLineEntry GodotLineEntry = null;

    public override void _Ready()
    {
        this.OnReady();
    }

    [SignalHandler("id_pressed")]
    void OnIdPressed(int id) {
        ((GodotPanel)GetParent())._IdPressed(id);
    }
}
