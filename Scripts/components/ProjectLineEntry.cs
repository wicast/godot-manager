using Godot;
using Godot.Collections;
using Godot.Sharp.Extras;
using System;

public partial class ProjectLineEntry   : ColorRect
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
    [Signal]
    public delegate void FavoriteUpdatedEventHandler(ProjectLineEntry self);
    [Signal]
    public delegate void DragStartedEventHandler(ProjectLineEntry self);
    [Signal]
    public delegate void DragEndedEventHandler(ProjectLineEntry self);
#endregion

#region Private Node Variables
    [NodePath("hc/ProjectIcon")]
    private TextureRect _icon = null;
    [NodePath("hc/vc/ProjectName")]
    private Label _name = null;
    [NodePath("hc/vc/ProjectDesc")]
    private Label _desc = null;
    [NodePath("hc/vc/ProjectLocation")]
    private Label _location = null;
    [NodePath("hc/GodotVersion")]
    private Label _version = null;
    [NodePath("hc/HeartIcon")]
    public HeartIcon HeartIcon = null;
#endregion

#region Preload Resources
    private Texture2D _missingIcon = GD.Load<Texture2D>("res://Assets/Icons/missing_icon.svg");
    private Texture2D _defaultIcon = GD.Load<Texture2D>("res://Assets/Icons/default_project_icon.png");
#endregion

#region Private Variables
    private string sIcon = "res://Assets/Icons/missing_icon.svg";
    private string sName = "Project Name";
    private string sDesc = "Project Description";
    private string sLocation = "/home/eumario/Projects/Godot/ProjectName";
    private string sGodotVersion = "";
    private ProjectFile pfProjectFile = null;
#endregion

#region Public Accessors
    public bool MissingProject { get; set; } = false;

    public ProjectFile ProjectFile {
        get {
            return pfProjectFile;
        }

        set {
            pfProjectFile = value;
            Name = value.Name;
            Description = value.Description;
            Icon = value.Location.GetResourceBase(value.Icon);
            Location = MissingProject ? Tr("Unknown Location") : value.Location;
            GodotVersion = value.GodotVersion;
            if (HeartIcon != null) {
                HeartIcon.SetCheck(value.Favorite);
            }
        }
    }

    public string Icon {
        get {
            if (_icon != null)
                return _icon.Texture.ResourcePath;
            else
                return sIcon;
        }

        set {
            sIcon = value;
            if (_icon != null) {
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
    }

    new public string Name {
        get {
            if (_name != null)
                return _name.Text;
            else
                return sName;
        }
        set {
            sName = value;
            if (_name != null)
                _name.Text = value;
        }
    }

    public string Description {
        get {
            return sDesc;
        }
        set {
            sDesc = value;
            if (_desc != null) {
                if (sDesc == null || sDesc.StripEdges() == "")
                    _desc.Text = Tr("No Description");
                else
                    _desc.Text = value;
            }
        }
    }

    public string Location {
        get {
            return sLocation;
        }
        set {
            sLocation = value;
            if (_location != null)
                if (MissingProject)
                    _location.Text = value;
                else
                    _location.Text = value.GetBaseDir();
        }
    }

    public string GodotVersion {
        get {
            return sGodotVersion;
        }
        
        set {
            sGodotVersion = value;
            if (_version != null) {
                GodotVersion gv = CentralStore.Instance.FindVersion(value);
                if (gv != null) {
                    _version.Text = gv.GetDisplayName();
                } else {
                    _version.Text = Tr("Unknown");
                }
            }
        }
    }
#endregion

    public override void _Ready()
    {
        this.OnReady();

        Icon = sIcon;
        Name = sName;
        Description = sDesc;
        Location = sLocation;
        GodotVersion = sGodotVersion;
        HeartIcon.SetCheck(ProjectFile.Favorite);
    }

    [SignalHandler("clicked", nameof(HeartIcon))]
    void OnHeartClicked() {
        ProjectFile.Favorite = HeartIcon.IsChecked();
        CentralStore.Instance.SaveDatabase();
        EmitSignal("FavoriteUpdated", this);
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

    // Test Drag and Drop
    public override bool _CanDropData(Vector2 position, Variant data)
    {
        return GetParent().GetParent<CategoryList>()._CanDropData(position, data);
    }

    public override void _DropData(Vector2 position, Variant data)
    {
        GetParent().GetParent<CategoryList>()._DropData(position, data);
    }

    public override Variant _GetDragData(Vector2 position) {
        if (!(GetParent().GetParent() is CategoryList))
            return default;
        Dictionary data = new Dictionary();
        data["source"] = this;
        data["parent"] = this.GetParent().GetParent();
        var preview = GD.Load<PackedScene>("res://components/ProjectLineEntry.tscn").Instantiate<ProjectLineEntry>();
        var notifier = new VisibleOnScreenNotifier2D();
        preview.AddChild(notifier);
        notifier.Connect("screen_entered", Callable.From(OnDragStart));
        notifier.Connect("screen_exited", Callable.From(OnDragEnded));
        preview.ProjectFile = ProjectFile;
        SetDragPreview(preview);
        data["preview"] = preview;
        return data;
    }

    void OnDragStart() {
		Input.MouseMode = Input.MouseModeEnum.Confined;
		EmitSignal("DragStarted", this);
    }

    void OnDragEnded() {
		Input.MouseMode = Input.MouseModeEnum.Visible;
		EmitSignal("DragEnded", this);
    }
}