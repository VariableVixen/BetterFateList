using System;
using System.Numerics;
using System.Reflection;

using Dalamud.Interface.Windowing;
using Dalamud.Plugin;

using FFXIVClientStructs.FFXIV.Client.UI.Agent;

using ImGuiNET;

using Lumina.Excel.Sheets;

namespace VariableVixen.BetterFateList;

internal class Plugin: IDalamudPlugin {
	public const string
		Name = "BetterFateList",
		Command = "/fatelist";

	private WindowSystem? windowSystem;
	internal WindowSystem? WindowSystem {
		get => this.windowSystem;
		set {
#if DEBUG
			if (this.windowSystem is not null)
				throw new InvalidOperationException("attempted to assign new WindowSystem while one already exists");
			this.windowSystem = value;
#else
			this.windowSystem ??= value;
#endif
		}
	}

	private FateListWindow? mainWindow;
	internal FateListWindow? DisplayWindow {
		get => this.mainWindow;
		set {
#if DEBUG
			if (this.mainWindow is not null)
				throw new InvalidOperationException("attempted to assign new FateListWindow while one already exists");
			this.mainWindow = value;
#else
			this.mainWindow ??= value;
#endif
		}
	}

	private ConfigWindow? configWindow;
	internal ConfigWindow? ConfigWindow {
		get => this.configWindow;
		set {
#if DEBUG
			if (this.configWindow is not null)
				throw new InvalidOperationException("attempted to assign new ConfigWindow while one already exists");
			this.configWindow = value;
#else
			this.configWindow ??= value;
#endif
		}
	}

	public static readonly Version Version = Assembly.GetExecutingAssembly().GetName().Version!;

	public Plugin(IDalamudPluginInterface pluginInterface) {
		pluginInterface.Create<Service>(this, pluginInterface.GetPluginConfig() ?? new Configuration());

		this.DisplayWindow = new("Available FATEs");
		this.ConfigWindow = new($"{Name} Settings");
		this.WindowSystem = new(this.GetType().Namespace!);
		this.WindowSystem.AddWindow(this.DisplayWindow);
		this.WindowSystem.AddWindow(this.ConfigWindow);

		Service.Interface.UiBuilder.OpenMainUi += this.ToggleFateListUi;
		Service.Interface.UiBuilder.OpenConfigUi += this.ToggleConfigUi;
		Service.Interface.UiBuilder.Draw += this.WindowSystem.Draw;

		Service.Commands.AddHandler(Command, new(this.PluginCommand) {
			HelpMessage = "Toggle the FATE list window",
			ShowInHelp = true,
		});
	}

	internal static unsafe void SetLocalMapMarker(Vector3 worldPos) {
		AgentMap* uiMap = AgentMap.Instance();
		if (uiMap is null)
			return;
		uint zone = Service.Client.TerritoryType;
		Map? map = Service.Lumina.GetExcelSheet<TerritoryType>()?.GetRowOrDefault(zone)?.Map.Value;
		if (map is null)
			return;
		uiMap->IsFlagMarkerSet = false;
		uiMap->SetFlagMapMarker(zone, map.Value.RowId, worldPos);
		if (ImGui.IsKeyDown(ImGuiKey.ModShift)) {
			uiMap->AgentInterface.Hide();
			uiMap->OpenMap(map.Value.RowId, zone);
		}
	}

	internal void PluginCommand(string command, string args) => this.ToggleFateListUi();

	internal void ToggleFateListUi() {
		if (this.DisplayWindow is not null) {
			this.DisplayWindow.IsOpen = !this.DisplayWindow.IsOpen;
		}
		else {
			Service.Log.Error($"Cannot toggle FATE list window, reference does not exist");
		}
	}
	internal void ToggleConfigUi() {
		if (this.ConfigWindow is not null) {
			this.ConfigWindow.IsOpen = !this.ConfigWindow.IsOpen;
		}
		else {
			Service.Log.Error($"Cannot toggle configuration window, reference does not exist");
		}
	}

	#region Dispose
	private bool disposed;

	protected virtual void Dispose(bool disposing) {
		if (this.disposed)
			return;
		this.disposed = true;

		if (disposing) {
			Service.Commands.RemoveHandler(Command);
		}

		Service.Plugin = null!;
	}

	~Plugin() {
		this.Dispose(false);
	}

	public void Dispose() {
		this.Dispose(true);
		GC.SuppressFinalize(this);
	}
	#endregion

}
