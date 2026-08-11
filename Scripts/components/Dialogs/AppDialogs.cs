using Godot;
using Godot.Collections;

public partial class AppDialogs   : Control
{
#region Node Paths
    public FirstRunWizard FirstRunWizard_ = null;
    public AddCustomGodot AddCustomGodot_ = null;
    public BusyDialog BusyDialog_ = null;
    public NewVersion NewVersion_ = null;
    public YesNoDialog YesNoDialog_ = null;
    public YesNoCancelDialog YesNoCancelDialog_ = null;
    public ImportProject ImportProject_ = null;
    public MessageDialog MessageDialog_ = null;
    public FileDialog ImageFileDialog_ = null;
    public FileDialog ImportFileDialog_ = null;
    public FileDialog BrowseFolderDialog_ = null;
    public FileDialog BrowseGodotDialog_ = null;
    public CreateProject CreateProject_ = null;
    public EditProject EditProject_ = null;
    public CreateCategory CreateCategory_ = null;
    public RemoveCategory RemoveCategory_ = null;
    public AssetLibPreview AssetLibPreview_ = null;
    public DownloadAddon DownloadAddon_ = null;
    public DownloadGodotManager DownloadGodotManager_ = null;
    public AddonInstaller AddonInstaller_ = null;
    public FileConflictDialog FileConflictDialog_ = null;
    public AddonMirror AddonMirror_ = null;
    public ManageCustomDownloads ManageCustomDownloads_ = null;
    public ListSelectDialog ListSelectDialog_ = null;
#endregion

#region Singleton Variables to access in program
    public static FirstRunWizard FirstRunWizard => Instance.FirstRunWizard_;
    public static AddCustomGodot AddCustomGodot => Instance.AddCustomGodot_;
    public static BusyDialog BusyDialog => Instance.BusyDialog_;
    public static NewVersion NewVersion => Instance.NewVersion_;
    public static YesNoDialog YesNoDialog => Instance.YesNoDialog_;
    public static YesNoCancelDialog YesNoCancelDialog => Instance.YesNoCancelDialog_;
    public static ImportProject ImportProject => Instance.ImportProject_;
    public static MessageDialog MessageDialog => Instance.MessageDialog_;
    public static FileDialog ImageFileDialog => Instance.ImageFileDialog_;
    public static FileDialog ImportFileDialog => Instance.ImportFileDialog_;
    public static FileDialog BrowseFolderDialog => Instance.BrowseFolderDialog_;
    public static FileDialog BrowseGodotDialog => Instance.BrowseGodotDialog_;
    public static CreateProject CreateProject => Instance.CreateProject_;
    public static EditProject EditProject => Instance.EditProject_;
    public static CreateCategory CreateCategory => Instance.CreateCategory_;
    public static RemoveCategory RemoveCategory => Instance.RemoveCategory_;
    public static AssetLibPreview AssetLibPreview => Instance.AssetLibPreview_;
    public static DownloadAddon DownloadAddon => Instance.DownloadAddon_;
    public static DownloadGodotManager DownloadGodotManager => Instance.DownloadGodotManager_;
    public static AddonInstaller AddonInstaller => Instance.AddonInstaller_;
    public static FileConflictDialog FileConflictDialog => Instance.FileConflictDialog_;
    public static AddonMirror AddonMirror => Instance.AddonMirror_;
    public static ManageCustomDownloads ManageCustomDownloads => Instance.ManageCustomDownloads_;
    public static ListSelectDialog ListSelectDialog => Instance.ListSelectDialog_;

    #endregion

    private static AppDialogs _instance = null;

    public static AppDialogs Instance {
        get {
            if (_instance == null)
                _instance = new AppDialogs();
            
            return _instance;
        }
    }

    private Array<ReferenceRect> dialogs;

    protected AppDialogs() {

        // Initialize Dialogs
        FirstRunWizard_ = GD.Load<PackedScene>("res://components/Dialogs/FirstRunWizard.tscn").Instantiate<FirstRunWizard>();
        AddCustomGodot_ = GD.Load<PackedScene>("res://components/Dialogs/AddCustomGodot.tscn").Instantiate<AddCustomGodot>();
        BusyDialog_ = GD.Load<PackedScene>("res://components/Dialogs/BusyDialog.tscn").Instantiate<BusyDialog>();
        NewVersion_ = GD.Load<PackedScene>("res://components/Dialogs/NewVersion.tscn").Instantiate<NewVersion>();
        YesNoDialog_ = GD.Load<PackedScene>("res://components/Dialogs/YesNoDialog.tscn").Instantiate<YesNoDialog>();
        YesNoCancelDialog_ = GD.Load<PackedScene>("res://components/Dialogs/YesNoCancelDialog.tscn").Instantiate<YesNoCancelDialog>();
        ImportProject_ = GD.Load<PackedScene>("res://components/Dialogs/ImportProject.tscn").Instantiate<ImportProject>();
        MessageDialog_ = GD.Load<PackedScene>("res://components/Dialogs/MessageDialog.tscn").Instantiate<MessageDialog>();
        CreateProject_ = GD.Load<PackedScene>("res://components/Dialogs/CreateProject.tscn").Instantiate<CreateProject>();
        EditProject_ = GD.Load<PackedScene>("res://components/Dialogs/EditProject.tscn").Instantiate<EditProject>();
        CreateCategory_ = GD.Load<PackedScene>("res://components/Dialogs/CreateCategory.tscn").Instantiate<CreateCategory>();
        RemoveCategory_ = GD.Load<PackedScene>("res://components/Dialogs/RemoveCategory.tscn").Instantiate<RemoveCategory>();
        AssetLibPreview_ = GD.Load<PackedScene>("res://components/Dialogs/AssetLibPreview.tscn").Instantiate<AssetLibPreview>();
        DownloadAddon_ = GD.Load<PackedScene>("res://components/Dialogs/DownloadAddon.tscn").Instantiate<DownloadAddon>();
        DownloadGodotManager_ = GD.Load<PackedScene>("res://components/Dialogs/DownloadGodotManager.tscn").Instantiate<DownloadGodotManager>();
        AddonInstaller_ = GD.Load<PackedScene>("res://components/Dialogs/AddonInstaller.tscn").Instantiate<AddonInstaller>();
        FileConflictDialog_ = GD.Load<PackedScene>("res://components/Dialogs/FileConflictDialog.tscn").Instantiate<FileConflictDialog>();
        AddonMirror_ = GD.Load<PackedScene>("res://components/Dialogs/AddonMirror.tscn").Instantiate<AddonMirror>();
        ManageCustomDownloads_ = GD.Load<PackedScene>("res://components/Dialogs/ManageCustomDownloads.tscn").Instantiate<ManageCustomDownloads>();
        ListSelectDialog_ = GD.Load<PackedScene>("res://components/Dialogs/ListSelectDialog.tscn").Instantiate<ListSelectDialog>();

        ImageFileDialog_ = new FileDialog();
        ImageFileDialog_.Name = "ImageFileDialog";
        ImageFileDialog_.FileMode = FileDialog.FileModeEnum.OpenFile;
        ImageFileDialog_.Access = FileDialog.AccessEnum.Filesystem;
        ImageFileDialog_.Title = Tr("Open Icon...");
        ImageFileDialog_.Filters = new string[] {"*.png", "*.webp", "*.svg", "*.svgz"};
        ImageFileDialog_.MinSize = new Vector2I(510, 390);
        ImageFileDialog_.Theme = GD.Load<Theme>("res://Resources/DefaultTheme.tres");

        // Internal File Dialog
        ImportFileDialog_ = new FileDialog();
        ImportFileDialog_.Name = "ImportFileDialog";
        ImportFileDialog_.FileMode = FileDialog.FileModeEnum.OpenFile;
        ImportFileDialog_.Access = FileDialog.AccessEnum.Filesystem;
        ImportFileDialog_.Title = Tr("Open Godot Project...");
        ImportFileDialog_.Filters = new string[] {"*.godot"};
        ImportFileDialog_.MinSize = new Vector2I(510, 390);
        ImportFileDialog_.Theme = GD.Load<Theme>("res://Resources/DefaultTheme.tres");

        // Internal Browse Folder Dialog
        BrowseFolderDialog_ = new FileDialog();
        BrowseFolderDialog_.Name = "BrowseFileDialog";
        BrowseFolderDialog_.FileMode = FileDialog.FileModeEnum.OpenDir;
        BrowseFolderDialog_.Access = FileDialog.AccessEnum.Filesystem;
        BrowseFolderDialog_.Title = Tr("Open Folder");
        BrowseFolderDialog_.MinSize = new Vector2I(510, 390);
        BrowseFolderDialog_.Theme = GD.Load<Theme>("res://Resources/DefaultTheme.tres");

        // Internal Browse Godot Dialog
        BrowseGodotDialog_ = new FileDialog();
        BrowseGodotDialog_.Name = "BrowseGodotDialog";
        BrowseGodotDialog_.FileMode = FileDialog.FileModeEnum.OpenFile;
        BrowseGodotDialog_.Access = FileDialog.AccessEnum.Filesystem;
        BrowseGodotDialog_.Title = Tr("Find Godot...");
        BrowseGodotDialog_.Filters = new string[] { "*.exe", "*.x86_64", "*.x86", "*.64", "*.32", ".app", "godot"};
        BrowseGodotDialog_.MinSize = new Vector2I(510, 390);
        BrowseGodotDialog_.Theme = GD.Load<Theme>("res://Resources/DefaultTheme.tres");

        dialogs = new Array<ReferenceRect> {    // Hierarchy of Dialogs in window, for proper displaying
            FirstRunWizard_,                    // First Run Wizard Helper
            AddCustomGodot_, NewVersion_,       // Add Custom Godot / New Godot Version Prompt
            CreateProject_, ImportProject_,     // Create Project / Import Project
            EditProject_,                       // Edit Project
            AssetLibPreview_, DownloadAddon_,   // Asset Library Preview / Download Addon/Project
            ManageCustomDownloads_,             // Custom Godot Editor Downloads
            DownloadGodotManager_,              // Download Godot Manager Update
            AddonMirror_,                       // Adding Addon Mirror to list
            CreateCategory_,                    // Create a Category
            RemoveCategory_,                    // Remove a Category
            AddonInstaller_,                    // Installer Dialog for Addon/Plugins
            FileConflictDialog_,                // File Conflict Dialog
            YesNoDialog_,                       // Yes No Prompt
            YesNoCancelDialog_,                 // Yes, No, Cancel Prompt
            BusyDialog_,                        // Busy Dialog
            MessageDialog_,                     // Message Dialog
            ListSelectDialog_,                  // Dialog for Selecting a Specific Option from a List.
        };

        MouseFilter = Control.MouseFilterEnum.Ignore;
    }

    public override void _EnterTree() {
        // Setup Full Rect for dialogs:
        foreach(ReferenceRect dlg in dialogs ) {
            dlg.SetAnchorsPreset(LayoutPreset.FullRect);
            dlg.Visible = false;
            AddChild(dlg);
        }
        AddChild(ImageFileDialog_);
        AddChild(ImportFileDialog_);
        AddChild(BrowseFolderDialog_);
        AddChild(BrowseGodotDialog_);
    }
}
