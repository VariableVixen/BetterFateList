using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Fates;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Interface.Windowing;

namespace VariableVixen.BetterFateList;

internal class FateListWindow: Window {
	public const ImGuiWindowFlags LOCKED_FLAGS = ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize;

	internal static readonly Vector4
		levelTooLow = new(176 / 255f, 23 / 255f, 31 / 255f, 1),
		levelInRange = new(60 / 255f, 179 / 255f, 113 / 255f, 1),
		levelTooHigh = new(179 / 255f, 238 / 255f, 58 / 255f, 1),
		fateHasXpBonus = new(30 / 255f, 144 / 255f, 255 / 255f, 1);

	public FateListWindow(string name, ImGuiWindowFlags flags = ImGuiWindowFlags.NoNav, bool forceMainWindow = false) : base(name, flags, forceMainWindow) {
		this.SizeCondition = ImGuiCond.FirstUseEver;
		this.SizeConstraints = new() {
			MinimumSize = new(280, 350),
			MaximumSize = new(400, 750),
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

	public override void PreDraw() {
		base.PreDraw();
		if (Service.Config.LockDisplayWindow)
			this.Flags |= LOCKED_FLAGS;
		else
			this.Flags &= ~LOCKED_FLAGS;
	}

	public override bool DrawConditions() => base.DrawConditions() && Service.Client?.LocalContentId is not (null or 0);

	public override void Draw() {
		bool anyFates = false;

		IPlayerCharacter player = Service.Client.LocalPlayer!;
		Vector3 here = player.Position;
		byte level = player.Level;
		IEnumerable<IFate> fates = Service.Fates
			.Where(fate => fate is not null && (fate.State is FateState.Preparation || (fate.State is FateState.Running && fate.TimeRemaining >= 0)))
			.OrderByDescending(fate => fate.MaxLevel)
			.ThenBy(fate => Vector3.Distance(fate.Position, here))
			.ToArray();

		byte lastMaxLevel = fates.First().MaxLevel;
		foreach (IFate fate in fates) {
			anyFates = true;

			string levelLabel = $"{fate.Level}/{fate.MaxLevel}";
			float levelWidth = ImGui.CalcTextSize(levelLabel).X;
			float offset = ImGui.GetWindowWidth() - ImGui.GetStyle().WindowPadding.X - levelWidth;

			if (lastMaxLevel != fate.MaxLevel) {
				ImGui.Separator();
				ImGui.Spacing();
				ImGui.Spacing();
				lastMaxLevel = fate.MaxLevel;
			}

			if (fate.HasBonus)
				ImGui.PushStyleColor(ImGuiCol.Text, fateHasXpBonus);
			if (ImGui.SmallButton(fate.Name.TextValue)) {
				Plugin.SetLocalMapMarker(fate.Position);
			}
			if (fate.HasBonus)
				ImGui.PopStyleColor();

			if (ImGui.IsItemHovered()) {
				ImGui.BeginTooltip();
				ImGui.PushTextWrapPos(this.SizeConstraints!.Value.MaximumSize.X);
				ImGui.BeginDisabled();
				ImGui.TextUnformatted("Click to set your flag. Hold shift to open your map.");
				ImGui.EndDisabled();
				ImGui.TextUnformatted(fate.Description.TextValue);
				ImGui.PopTextWrapPos();
				ImGui.EndTooltip();
			}

			ImGui.SameLine(offset);
			if (level < fate.Level) {
				ImGui.PushStyleColor(ImGuiCol.Text, levelTooLow);
				ImGui.TextUnformatted(levelLabel);
				ImGui.PopStyleColor();
				if (ImGui.IsItemHovered()) {
					ImGui.BeginTooltip();
					ImGui.PushTextWrapPos(this.SizeConstraints!.Value.MinimumSize.X);
					ImGui.TextUnformatted("This FATE is above your level.");
					ImGui.BeginDisabled();
					ImGui.TextUnformatted("It will be more difficult, and your rewards will be reduced.");
					ImGui.EndDisabled();
					ImGui.PopTextWrapPos();
					ImGui.EndTooltip();
				}
			}
			else if (level > fate.MaxLevel) {
				ImGui.PushStyleColor(ImGuiCol.Text, levelTooHigh);
				ImGui.TextUnformatted(levelLabel);
				ImGui.PopStyleColor();
				if (ImGui.IsItemHovered()) {
					ImGui.BeginTooltip();
					ImGui.PushTextWrapPos(this.SizeConstraints!.Value.MinimumSize.X);
					ImGui.TextUnformatted("This FATE is below your level.");
					ImGui.BeginDisabled();
					ImGui.TextUnformatted("You will need to remember to sync down, and will lose some actions.");
					ImGui.EndDisabled();
					ImGui.PopTextWrapPos();
					ImGui.EndTooltip();
				}
			}
			else {
				ImGui.PushStyleColor(ImGuiCol.Text, levelInRange);
				ImGui.TextUnformatted(levelLabel);
				ImGui.PopStyleColor();
				if (ImGui.IsItemHovered()) {
					ImGui.BeginTooltip();
					ImGui.PushTextWrapPos(this.SizeConstraints!.Value.MinimumSize.X);
					ImGui.TextUnformatted("This FATE is at your level.");
					ImGui.PopTextWrapPos();
					ImGui.EndTooltip();
				}
			}

			ImGui.Indent();
			ImGui.BeginDisabled();
			ImGui.TextUnformatted($"{fate.TypeLabel()}, {MathF.Max(0, MathF.Round(Vector3.Distance(fate.Position, here), MidpointRounding.ToZero))} yalms");
			if (fate.State is FateState.Preparation)
				ImGui.TextUnformatted("awaiting start");
			else
				ImGui.TextUnformatted($"{clockTime(fate.TimeRemaining)} remaining, {fate.Progress}% complete");
			ImGui.EndDisabled();
			ImGui.Unindent();

			ImGui.Spacing();
			ImGui.Spacing();
			ImGui.Spacing();
		}

		if (!anyFates) {
			ImGui.BeginDisabled();
			ImGui.TextUnformatted("Waiting for FATEs...");
			ImGui.EndDisabled();
		}
	}

	private static string clockTime(long seconds) {
		long minutes = (long)Math.Floor(seconds / 60f);
		seconds %= 60;
		return $"{minutes:D2}:{seconds:D2}";
	}
}
