# Godot 3 → Godot 4.7 C# 迁移规范

把 Godot 3.x C# 代码迁移到 Godot 4.7。项目已配置 `Godot.NET.Sdk/4.7.0`、`net8.0`、`GodotSharpExtras 0.5.0`。
`[NodePath]`、`[SignalHandler(...)]`、`OnReady()`、`GdAssert` 等 GodotSharpExtras 特性在 0.5.0 中仍然有效，**不要改它们**。
带 `[Signal]` 的委托声明（如 `[Signal] public delegate void update_projects();`）保持不变，继续用 `EmitSignal("signal", args)` 字符串形式发射信号。

## 通用替换规则（按优先级）

1. **`Godot.Object` → `GodotObject`**。`class X : Object`（无 using Godot.Object 时）→ `class X : GodotObject`；`using Object = Godot.Object;` → `using Object = GodotObject;`。确保文件有 `using Godot;`。注意不要误改 `System.Object`。
2. **`StreamTexture` → `CompressedTexture2D`**（加载 `.svg`/`.png` 贴图时）。字段/参数类型如果赋值给 `Button.Icon`、`TextureRect.Texture`、`ItemList` 图标，用基类 `Texture2D` 更稳妥。`GD.Load<StreamTexture>(...)` → `GD.Load<Texture2D>(...)`。
3. **`PackedScene.Instance<T>()` → `PackedScene.Instantiate<T>()`**。
4. **`Control.RectMinSize` → `CustomMinimumSize`；`RectSize` → `Size`；`RectGlobalPosition` → `GlobalPosition`；`RectPivotOffset` → `PivotOffset`；`RectRotation` → `Rotation`；`RectScale` → `Scale`**。
5. **`SetAnchorsAndMarginsPreset(LayoutPreset.Wide)` → `SetAnchorsPreset(LayoutPreset.FullRect)`**。
6. **`Popup_(new Rect2(pos, size))` → `Popup(new Rect2I((Vector2I)pos, (Vector2I)size))`**。`PopupCentered(new Vector2(w,h))` → `PopupCentered(new Vector2I(w,h))`。
7. **`new Tween()` 旧 API**：`var tween = new Tween(); AddChild(tween); tween.InterpolateProperty(node, prop, from, to, dur, Tween.TransitionType.X, Tween.EaseType.Y); tween.Start();`
   → `var tween = CreateTween(); tween.TweenProperty(node, prop, to, dur).SetTrans(Tween.TransitionType.X).SetEase(Tween.EaseType.Y);`
   其它：`tween.StopAll()` → `tween.Kill()`；`tween.IsActive()` → `tween.IsRunning()`；`tween.SetLoops(n)` 仍可用。注意 `CreateTween()` 自动开始，去掉 `Start()`。
8. **`RichTextLabel.BbcodeText` → `Text`**（若需 BBCode，设 `BbcodeEnabled = true`）。
9. **`JSON.Parse(str)`（Godot JSON 类）**：Godot 4 C# 用 `Godot.Json.ParseString(str)`（静态，返回 `Variant`）。
   旧代码形如 `var jsonResponse = JSON.Parse(result.Body); if (jsonResponse.Error != Error.Ok) ...` → 
   `var jsonResponse = Json.ParseString(result.Body); if (jsonResponse.VariantType == Variant.Type.Nil) ...`，然后 `var dict = jsonResponse.As<Godot.Collections.Dictionary>();`。
10. **`Godot.File` → `FileAccess`**（Godot 4 移除了 File 类）：
    - `new File()` + `fh.Open(path, File.ModeFlags.Read) == Error.Ok` → `var fh = FileAccess.Open(path, FileAccess.ModeFlags.Read); if (fh != null)`。
    - `File.ModeFlags.Write` → `FileAccess.ModeFlags.Write`；`ReadWrite` 同理。
    - `fh.GetAsText()`、`fh.GetLine()`、`fh.StoreString(s)`、`fh.StoreLine(s)`、`fh.StoreBuffer(bytes)`、`fh.Close()`、`fh.EofReached()`、`fh.GetLength()` 同名可用；`fh.GetPosition()` 可用。
    - 路径含 `res://`/`user://` 时 `FileAccess.Open` 也接受虚拟路径（同 Godot 3）。若代码先 `GetOSDir()` 再 Open，保持不变即可。
11. **`new Directory()` / `dir.Open(path) == Error.Ok` → `var dir = DirAccess.Open(path); if (dir != null)`**。`dir.GetFiles()`、`dir.GetDirectories()`、`dir.FileExists()`、`dir.DirExists()`、`dir.MakeDirRecursive()`、`dir.GetNext()`、`dir.ListDirBegin()`、`dir.ListDirEnd()` 均可用。静态判断用 `DirAccess.DirExistsAbsolute(path)`。
12. **`HTTPClient.Status` → `GDCSHTTPClient.Status`**（本项目自建枚举，定义在 GDCSHTTPClient.cs 内，成员：`Connected, CantResolve, CantConnect, ConnectionError, SslHandshakeError, Requesting, Body`）。
    `Task<HTTPClient.Status>` → `Task<GDCSHTTPClient.Status>`。信号/委托签名 `HTTPClient.Status` 同理替换。
13. **窗口 API（`OS.Window*` → 根 Window）**：
    - `OS.WindowSize` → `GetWindow().Size`（`Vector2I`，注意把 `Vector2` 强转 `(Vector2I)`）。
    - `OS.MinWindowSize` → `GetWindow().MinSize`。
    - `OS.CenterWindow()` → `GetWindow().MoveToCenter()`。
    - `OS.WindowBorderless = b` → `GetWindow().Borderless = b`。
    - `OS.WindowPosition` → `GetWindow().Position`（`Vector2I`）。
    - `OS.WindowMinimized = true` → `GetWindow().Minimized = true`；`OS.WindowMaximized` → `GetWindow().Maximized`。
    - `OS.SetIcon(Image)` → `DisplayServer.SetIcon(image)`。
    - `OS.Clipboard = s` → `DisplayServer.ClipboardSet(s)`。
14. **`OS.Alert(title, msg)` → 删除**（Godot 4 无此 API）：UI 文件里替换为 `GD.PrintErr(title + ": " + msg)`；若上下文有 `AppDialogs.MessageDialog` 可用，则 `AppDialogs.MessageDialog.ShowMessage(title, msg)`。
15. **`OS.GetDataDir()` → `ProjectSettings.GlobalizePath("user://")`；`OS.GetUserDataDir()` → `ProjectSettings.GlobalizePath("user://")`**。
16. **`OS.DelayMsec(ms)` → `System.Threading.Thread.Sleep(ms)`**。
17. **`ResourceLoader.LoadInteractive(path)` → Godot 4 线程加载**：
    ```csharp
    ResourceLoader.LoadThreadedRequest("res://Scenes/SceneManager.tscn");
    while (ResourceLoader.LoadThreadedGetStatus("res://Scenes/SceneManager.tscn") != ResourceLoader.ThreadLoadStatus.Loaded) {
        System.Threading.Thread.Sleep(100);
    }
    var res = (PackedScene)ResourceLoader.LoadThreadedGet("res://Scenes/SceneManager.tscn");
    ```
18. **`AnimatedSprite` → `AnimatedSprite2D`**。
19. **拖放重写签名**（Godot 4 C# 用 `Variant`）：
    - `public override bool CanDropData(Vector2 atPosition, object data)` → `public override bool CanDropData(Vector2 atPosition, Variant data)`
    - `public override void DropData(Vector2 atPosition, object data)` → `public override void DropData(Vector2 atPosition, Variant data)`
    - `public override object GetDragData(Vector2 atPosition)` → `public override Variant GetDragData(Vector2 atPosition)`
    - 方法体内对 data 的使用：`(Godot.Collections.Dictionary)data` 仍可（Variant 显式转换），返回字符串/Dictionary 时直接 `return data;`（隐式转换）或用 `return new Variant(data);`。
20. **信号处理器参数类型**：Godot 4 C# 内置信号回调中 int 参数变成 `long`。方法名以 `On...`/`..._IdPressed` 等，若 Connect 用字符串（`Connect("id_pressed", this, "...")`），把处理器签名 `int id` → `long id`；`item_selected`、`pressed`（无参不变）、`toggled(bool)` 不变。
21. **`Godot.Collections.Dictionary` 用 `ContainsKey(...)`**（Godot 3 的 `Contains` 在 Godot 4 C# 中不存在）。
22. **`ImageTexture.CreateFromImage(image)`**：Godot 4 是**静态**方法：`ImageTexture.CreateFromImage(image)` 返回新 ImageTexture。不要写成实例方法调用。
23. **`Util.IdleFrame()` 扩展**：定义处 `this Godot.Object` → `this GodotObject`，信号 `"idle_frame"` → `"process_frame"`。
24. **`Godot.Collections.Array<T>`/`Dictionary`** API 基本不变（`Add`、`Remove`、`IndexOf`、`Count` 等可用）。
25. **`Tr()` 可用**（`Object.Tr`）。`Tr` 与字符串格式化组合时 `string.Format(Tr("..."), args)` 不变。
26. **`Connect("signal", this, "Method", binds, flags)` / `Connect("signal", this, "Method")` / `IsConnected(...)` / `Disconnect(...)`**：Godot 4 C# 保留了 Godot 3 风格重载，**原样保留**，不要改动。
27. **`GetNode<T>(new NodePath("..."))` → `GetNode<T>("...")`**（字符串隐式转 NodePath，可直接写）。
28. **`new Array { 0 }` 集合初始化**：可用，但建议保持原样。

## Connect / Disconnect / IsConnected 转换（Godot 4 只有 Callable 重载）

Godot 4.7 C# 的 `Connect` 只有 `Connect(StringName, Callable, uint flags = 0)`。所有 Godot 3 风格调用必须转换：

- `X.Connect("sig", this, "Method")` → `X.Connect("sig", Callable.From(Method))`（无参处理器）或 `X.Connect("sig", Callable.From<ParamType>(Method))`（有参处理器，按处理器参数写泛型参数）。
- `X.Connect("sig", this, "Method", bindsArray)`（4 参带 binds）→ `X.Connect("sig", Callable.From<...>(Method).Bind(bindArg1, bindArg2))`，把旧 Array 里的元素逐个展开为 Bind 参数。
- `X.Connect("sig", this, "Method", null, (int)ConnectFlags.Oneshot)` → `X.Connect("sig", Callable.From(Method), (uint)ConnectFlags.OneShot)`。
- **`ConnectFlags.Oneshot` → `ConnectFlags.OneShot`**（Godot 4 拼写）；`Deferred`、`Persist` 同理可用。
- `X.IsConnected("sig", this, "Method")` → `X.IsConnected("sig", Callable.From(Method))`。
- `X.Disconnect("sig", this, "Method")` → `X.Disconnect("sig", Callable.From(Method))`。
- 处理器方法的参数类型以现有代码为准；有参时 `Callable.From<T...>(Method)` 泛型参数与处理器参数一一对应。
- 处理器方法若返回值是 `Task`（`async Task`），改为保持 `async void`（Godot 信号回调）。

## 其它常用 Godot 3→4 替换

- `Godot.Texture`（基类）→ `Texture2D`（字段/参数/局部变量，凡是要赋给 `TextureRect.Texture`/`Button.Icon`/`ItemList` 图标的地方）。
- `(ButtonList)x` / `ButtonList.Left` / `ButtonList.Right` → `(MouseButton)x` / `MouseButton.Left` / `MouseButton.Right`。
- `Control.HintTooltip` → `TooltipText`。
- `Window.WindowTitle` → `Title`（Window/FileDialog 等）。
- `FileDialog.Mode = FileDialog.ModeEnum.OpenFile/OpenDir` → `FileDialog.FileMode = FileDialog.FileModeEnum.OpenFile/OpenDir`。`Access = FileDialog.AccessEnum.Filesystem` 保持（枚举存在）。
- `SetAnchorsAndMarginsPreset(LayoutPreset.Wide)` → `SetAnchorsPreset(LayoutPreset.FullRect)`。
- `PackedScene.Instance<T>()` → `PackedScene.Instantiate<T>()`。
- `PopupCentered(new Vector2(w,h))` → `PopupCentered(new Vector2I(w,h))`。
- `Popup_(new Rect2(pos,size))` → `Popup(new Rect2I((Vector2I)pos, (Vector2I)size))`。
- `RectMinSize` → `CustomMinimumSize`；`RectSize` → `Size`；`RectGlobalPosition` → `GlobalPosition`；`RectPivotOffset` → `PivotOffset`。
- 字符串方法：`FindLast(x)` → `RFind(x)`；`Substr(a,b)` 可用；`ReplaceN` 可用。
- `Tree.GetSelected()` 仍返回单个 `TreeItem`；多选用 `GetSelectedItems()`。
- 拖放虚方法签名（Godot 4 带下划线、用 Variant）：`_CanDropData(Vector2, Variant)`、`_DropData(Vector2, Variant)`、`_GetDragData(Vector2)` 返回 `Variant`。从 Variant 提取 GodotObject 用 `variant.AsGodotObject()`（方法）后接 `as T`；提取 Dictionary 用 `variant.As<Godot.Collections.Dictionary>()`。
- `GetMeta("ID")` 返回 Variant：`(int)GetMeta("ID")` 可用。
- `GetNodesInGroup(...)` 返回 `Godot.Collections.Array<Godot.Node>`，`foreach (var pb in ...) if (pb is PageButton)` 可用。

## 其它注意事项

- 不要把 `[Signal] public delegate` 改成事件语法，保持 `EmitSignal("...")` 字符串调用。
- 不要改动 `[NodePath("...")]` 字段、`[SignalHandler("signal", nameof(field))]`、`this.OnReady()`、`SceneNode<T>`、`GdAssert`。
- 保留 `#if GODOT_*` 平台宏（Godot 4 仍定义 `GODOT_MACOS`、`GODOT_WINDOWS`、`GODOT_LINUXBSD`、`GODOT_X11` 等）。
- `namespace Github` / `namespace Mirrors` / `namespace AssetLib` 保持不动。
- 完成后不要运行 `dotnet build`，由主代理统一编译验证。
- 用 `str_replace` 精确替换，逐文件处理；改动后回报每个文件的改动摘要。
