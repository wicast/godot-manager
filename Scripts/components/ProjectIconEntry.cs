using Godot;
using Godot.Sharp.Extras;
using System;

public partial class ProjectIconEntry   : ColorRect
{
#region Signals
    [Signal]
    public delegate void ClickedEventHandler(ProjectLineEntry self);
    [Signal]
    public delegate void DoubleClickedEventHandler(ProjectLineEntry self);
    [Signal]
    public delegate void RightClickedEventHandler(ProjectLineEntry self);
    [Signal]
    public delegate void RightDoubleClickedEventHandler(ProjectLineEntry self);
#endregion

#region Private Node Variables
    [NodePath("cc/vc/ProjectIcon")]
    private TextureRect _icon = null;
    [NodePath("cc/vc/ProjectName")]
    private Label _projectName = null;
    [NodePath("cc/vc/ProjectLocation")]
    private Label _projectLocation = null;
    [NodePath("cc/vc/GodotVersion")]
    private Label _godotVersion = null;
#endregion

#region Preload Resources
    private Texture2D _missingIcon = GD.Load<Texture2D>("res://Assets/Icons/missing_icon.svg");
    private Texture2D _defaultIcon = GD.Load<Texture2D>("res://Assets/Icons/default_project_icon.png");
#endregion

#region Private Variables
    private string sIcon;
    private string sProjectName;
    private string sProjectLocation;
    private string sGodotVersion;
    //private int iGodotVersion;
    private ProjectFile pfProjectFile;

    [Resource("res://Resources/Fonts/droid-bold-16.tres")] private Font titleFont = null;
    [Resource("res://Resources/Fonts/droid-regular-14.tres")]private Font subLineFont = null;
#endregion

#region Public Accessors
    public bool MissingProject { get; set; } = false;

    public string Icon {
        get {
            if (_icon != null)
                return _icon.Texture.ResourcePath;
            else
                return sIcon;
        }

        set {
            sIcon = value;
            if (_icon != null)
                if (MissingProject)
                    _icon.Texture = _missingIcon;
                else {
                    if (System.IO.File.Exists(value)) {
                        var texture = Util.LoadImage(value);
                        if (texture == null)
                            _icon.Texture = _defaultIcon;
                        else
                            _icon.Texture = texture;
                    } else {
                        _icon.Texture = _defaultIcon;
                    }
                }
        }
    }

    public string ProjectName {
        get {
            if (_projectName != null)
                return _projectName.Text;
            else
                return sProjectName;
        }

        set {
            sProjectName = value;
            if (_projectName != null) {
                Vector2 size = titleFont.GetStringSize(value);
                if (size.X > 280) {
                    _projectName.HorizontalAlignment = HorizontalAlignment.Left;
                } else {
                    _projectName.HorizontalAlignment = HorizontalAlignment.Center;
                }
                _projectName.Text = value;
            }
        }
    }

    public string Location {
        get {
            if (_projectLocation != null)
                return _projectLocation.Text;
            else
                return sProjectLocation;
        }

        set {
            sProjectLocation = value;
            if (_projectLocation != null) {
                Vector2 size = subLineFont.GetStringSize(value);
                if (size.X > 280) {
                    _projectLocation.HorizontalAlignment = HorizontalAlignment.Left;
                } else {
                    _projectLocation.HorizontalAlignment = HorizontalAlignment.Center;
                }

                if (MissingProject)
                    _projectLocation.Text = value;
                else
                    _projectLocation.Text = value.GetBaseDir();
            }
        }
    }

    public ProjectFile ProjectFile {
        get {
            return pfProjectFile;
        }

        set {
            pfProjectFile = value;
            ProjectName = value.Name;
            Icon = value.Location.GetResourceBase(value.Icon);
            Location = MissingProject ? Tr("Unknown Location") : value.Location;
            GodotVersion = value.GodotVersion;
        }
    }

    public string GodotVersion {
        get {
            return sGodotVersion;
        }

        set {
            sGodotVersion = value;
            GodotVersion gv = CentralStore.Instance.FindVersion(value);
            if (_godotVersion != null) {
                if (gv != null) {
                    _godotVersion.Text = gv.GetDisplayName();
                    Vector2 size = subLineFont.GetStringSize(gv.GetDisplayName());
                    if (size.X > 280) {
                        _godotVersion.HorizontalAlignment = HorizontalAlignment.Left;
                    } else {
                        _godotVersion.HorizontalAlignment = HorizontalAlignment.Center;
                    }
                } else {
                    _godotVersion.Text = Tr("Unknown");
                    _godotVersion.HorizontalAlignment = HorizontalAlignment.Center;
                }
            }
        }
    }
#endregion

    public override void _Ready() {
        this.OnReady();

        Icon = sIcon;
        ProjectName = sProjectName;
        Location = sProjectLocation;
        GodotVersion = sGodotVersion;
    }

    [SignalHandler("gui_input")]
    void OnGuiInput(InputEvent inputEvent) {
        if (!(inputEvent is InputEventMouseButton))
            return;
        var iemb = inputEvent as InputEventMouseButton;
        if (!iemb.Pressed)
            return;
        
        if (iemb.ButtonIndex == MouseButton.Left) {
            if (iemb.DoubleClick)
                EmitSignal("DoubleClicked", this);
            else {
                SelfModulate = new Color("ffffffff");
                EmitSignal("Clicked", this);
            }
        } else if (iemb.ButtonIndex == MouseButton.Right) {
            if (iemb.DoubleClick)
                EmitSignal("RightDoubleClicked", this);
            else {
                SelfModulate = new Color("ffffffff");
                EmitSignal("RightClicked", this);
            }
        }

    }
}
