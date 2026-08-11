using Godot;
using Godot.Collections;
using Godot.Sharp.Extras;

public partial class ListSelectDialog   : ReferenceRect
{
    [Signal]
    public delegate void option_selectedEventHandler(string data);

    [Signal]
    public delegate void option_cancelledEventHandler();
    
    [NodePath("PC/CC/P/VB/MCContent/VC/Title")]
    private Label _title = null;

    [NodePath("PC/CC/P/VB/MCContent/VC/MessageText")]
    private Label _messageText = null;

    [NodePath("PC/CC/P/VB/MCContent/VC/Options")]
    private OptionButton _options = null;

    [NodePath("PC/CC/P/VB/MCContent/VC/CC/HC/Confirm")]
    private Button _confirm = null;

    [NodePath("PC/CC/P/VB/MCContent/VC/CC/HC/Cancel")]
    private Button _cancel = null;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        this.OnReady();
    }

    public void ShowDialog(string title, string message, Array<string> options)
    {
        _title.Text = title;
        _messageText.Text = message;
        _options.Clear();
        foreach (var option in options)
        {
            _options.AddItem(option);
        }

        Visible = true;
    }

    public void ShowDialog(string title, string message, Dictionary<string, string> options)
    {
        _title.Text = title;
        _messageText.Text = message;
        _options.Clear();
        foreach (var okv in options)
        {
            int indx = _options.ItemCount;
            _options.AddItem(okv.Key);
            _options.SetItemMetadata(indx, okv.Value);
        }

        Visible = true;
    }

    [SignalHandler("pressed", nameof(_cancel))]
    public void OnPressed_Cancel()
    {
        Visible = false;
        EmitSignal("option_cancelled");
    }

    [SignalHandler("pressed", nameof(_confirm))]
    public void OnPressed_Confirm()
    {
        Visible = false;
        var indx = _options.Selected;
        EmitSignal("option_selected", _options.GetItemMetadata(indx).AsString() ?? _options.GetItemText(indx));
    }
}
