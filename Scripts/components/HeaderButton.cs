using Godot;
using Godot.Sharp.Extras;
using System;

[Tool]
public partial class HeaderButton   : PanelContainer
{
    public enum SortDirection {
        Indeterminate,
        Up,
        Down
    }

    [Signal]
    public delegate void direction_changedEventHandler(SortDirection button);

    [Export]
    public string Title {
        get {
            if (_headerTitle != null)
                return _headerTitle.Text;
            else
                return _title;
        }

        set {
            _title = value;
            if (_headerTitle != null)
                _headerTitle.Text = value;
        }
    }

    [Export(PropertyHint.Enum,"Direction to Sort")]
    public SortDirection Direction {
        get {
            if (_dirIcon != null) {
                if (_dirIcon.Texture == arrow)
                    return (_dirIcon.FlipV ? SortDirection.Up : SortDirection.Down);
                else
                    return SortDirection.Indeterminate;
            } else {
                return _direction;
            }
        }

        set {
            _direction = value;
            if (_dirIcon != null) {
                if (value == SortDirection.Indeterminate)
                    _dirIcon.Texture = minus;
                else {
                    _dirIcon.Texture = arrow;
                    _dirIcon.FlipV = (value == SortDirection.Up);
                }
            }
        }
    }

    [NodePath("HC/Label")]
    Label _headerTitle = null;

    [NodePath("HC/DirIcon")]
    TextureRect _dirIcon = null;

    private string _title;
    private SortDirection _direction;

    Texture2D arrow = GD.Load<Texture2D>("res://Assets/Icons/drop_down1.svg");
    Texture2D minus = GD.Load<Texture2D>("res://Assets/Icons/minus.svg");

    public override void _Ready()
    {
        this.OnReady();
        Title = _title;
        Direction = _direction;
    }

    public void Indeterminate() {
        Direction = SortDirection.Indeterminate;
    }

    [SignalHandler("gui_input")]
    void OnGuiInput_Header(InputEvent @event) {
        if (@event is InputEventMouseButton @iemb) {
            if (@iemb.DoubleClick && @iemb.ButtonIndex == MouseButton.Left) {
                Direction = SortDirection.Indeterminate;
                EmitSignal("direction_changed", (int)Direction);
            } else if (@iemb.Pressed && @iemb.ButtonIndex == MouseButton.Left) {
                Direction = (Direction == SortDirection.Down) ? SortDirection.Up : SortDirection.Down;
                EmitSignal("direction_changed", (int)Direction);
            }
        }
    }
}
