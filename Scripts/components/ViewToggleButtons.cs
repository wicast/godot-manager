using Godot;
using Godot.Collections;

[Tool]
public partial class ViewToggleButtons   : PanelContainer
{
    [Signal]
    delegate void ClickedEventHandler(int index);

    [Export(PropertyHint.File)]
    Array<Texture2D> Icons = null;

    [Export]
    Array<string> HelpText = null;

    Array<ColorRect> _icons;

    int toggleIndx = 0;

    public int SelectedView { get => toggleIndx; }

    public override void _Ready()
    {
        _icons = new Array<ColorRect>();

        if (Icons == null) {
            Icons = new Array<Texture2D>();
        }

        if (HelpText == null) {
            HelpText = new Array<string>();
        }

        for (var i = 0; i < Icons.Count; i++) {
            ColorRect icon_bg = new ColorRect();
            icon_bg.CustomMinimumSize = new Vector2(20,20);
            icon_bg.Color = new Color("ACACAC");
            icon_bg.SelfModulate = new Color(1, 1, 1, 0); // "00ffffff" was AARRGGBB in Godot 3 (transparent), RRGGBBAA in Godot 4 (cyan!)
            TextureRect icon = new TextureRect();
            icon.Texture = Icons[i];
            icon.CustomMinimumSize = new Vector2(20,20);
            icon.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
            icon.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
            icon_bg.AddChild(icon);
            _icons.Add(icon_bg);
            
            Godot.Collections.Array bg = new Godot.Collections.Array();
            bg.Add(icon_bg);

            if (HelpText.Count > i) {
                icon_bg.TooltipText = HelpText[i];
            }

            GetNode<HBoxContainer>("Buttons").AddChild(icon_bg);
            icon_bg.Connect("mouse_entered", Callable.From(() => Icon_MouseEntered(icon_bg)));
            icon_bg.Connect("mouse_exited", Callable.From(() => Icon_MouseExited(icon_bg)));

            int captureIndex = i;
            icon_bg.Connect("gui_input", Callable.From((InputEvent e) => Icon_GuiInput(e, icon_bg, captureIndex)));
            
            if (i == toggleIndx) {
                icon.SelfModulate = new Color("7defa7");
            }
            
            if (i+1 < Icons.Count) {
                VSeparator sep = new VSeparator();
                GetNode<HBoxContainer>("Buttons").AddChild(sep);
            }
        }
    }

    public void SetView(int index) {
        if (_icons.Count == 0) {
            CallDeferred("SetView", index);
            return;
        }
        
        if (index >= 0 && index <= _icons.Count) {
            _icons[toggleIndx].GetChild<TextureRect>(0).SelfModulate = new Color("FFFFFF");
            toggleIndx = index;
            _icons[toggleIndx].GetChild<TextureRect>(0).SelfModulate = new Color("7defa7");
        }
    }

    public void Icon_MouseEntered(ColorRect rect) {
        rect.SelfModulate = new Color(1, 1, 1, 0.7255f); // was "B9ffffff" (white @ 72.5% in Godot 3)
    }

    public void Icon_MouseExited(ColorRect rect) {
        rect.SelfModulate = new Color(1, 1, 1, 0);
    }

    public void Icon_GuiInput(InputEvent inputEvent, ColorRect bg, int index) {
        if (!(inputEvent is InputEventMouseButton))
            return;
        
        var iemb = inputEvent as InputEventMouseButton;
        if (!iemb.Pressed && (MouseButton)iemb.ButtonIndex != MouseButton.Left)
            return;
        
        // _icons[toggleIndx].GetChild<TextureRect>(0).SelfModulate = new Color("FFFFFF");
        // toggleIndx = index;
        // bg.GetChild<TextureRect>(0).SelfModulate = new Color("7defa7");
        SetView(index);
        EmitSignal("Clicked", index);
    }
}
