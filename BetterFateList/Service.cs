using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace VariableVixen.BetterFateList;

internal class Service {
	[PluginService] public static Plugin Plugin { get; internal set; } = null!;
	[PluginService] public static Configuration Config { get; private set; } = null!;

	[PluginService] public static IPluginLog Log { get; private set; } = null!;
	[PluginService] public static IDalamudPluginInterface Interface { get; private set; } = null!;
	[PluginService] public static IClientState Client { get; private set; } = null!;
	[PluginService] public static IDataManager Lumina { get; private set; } = null!;
	[PluginService] public static ICommandManager Commands { get; private set; } = null!;
	[PluginService] public static IFateTable Fates { get; private set; } = null!;
}
