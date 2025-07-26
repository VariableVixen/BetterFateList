using Dalamud.Configuration;

namespace VariableVixen.BetterFateList;

internal class Configuration: IPluginConfiguration {

	public int Version { get; set; } = Plugin.Version.Major;

	public bool LockDisplayWindow { get; set; } = false;

}
