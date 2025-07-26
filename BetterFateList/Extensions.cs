using Dalamud.Game.ClientState.Fates;

namespace VariableVixen.BetterFateList;

public static class Extensions {
	public static string TypeLabel(this IFate fate) {
		return fate.IconId switch {
			60721 => "Horde",
			60722 => "Miniboss",
			60723 => "Collection",
			60724 => "Defence",
			60725 => "Escort",
			60726 => "QUEST!",
			60727 => "Chase",
			_ => "Unknown",
		};
	}
}
