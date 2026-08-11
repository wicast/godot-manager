using Godot;

public partial class SignalBus : Node
{
    [Signal] public delegate void update_projectsEventHandler();
}