using Dalamud.Interface.Windowing;

using ImGuiNET;

namespace VariableVixen.BetterFateList;

internal class ConfigWindow: Window {
	public ConfigWindow(string name, ImGuiWindowFlags flags = ImGuiWindowFlags.None, bool forceMainWindow = false) : base(name, flags, forceMainWindow) {
		this.SizeCondition = ImGuiCond.FirstUseEver;
		this.SizeConstraints = new() {
			MinimumSize = new(300, 150),
			MaximumSize = new(400, 500),
		};
		this.Size = this.SizeConstraints.Value.MinimumSize;
		this.Collapsed = false;
		this.CollapsedCondition = ImGuiCond.Appearing;
		this.AllowPinning = true;
		this.AllowClickthrough = false;
		this.RespectCloseHotkey = false;
	}

	public override void OnOpen() {
		base.OnOpen();
		this.Collapsed = false;
	}

	public override void Draw() {
		bool changed = false;

		bool lockMainWindow = Service.Config.LockDisplayWindow;
		if (ImGui.Checkbox("Lock FATE display window?", ref lockMainWindow)) {
			changed = true;
			Service.Config.LockDisplayWindow = lockMainWindow;
		}

		if (changed)
			Service.Interface.SavePluginConfig(Service.Config);
	}
}
