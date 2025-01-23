using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Reflection;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Personalities;
using Terraria.GameInput;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI.Gamepad;

namespace NPCHappinessViewer {
	public class NPCHappinessViewer : Mod {
		internal static ModKeybind checkHappinessData;
		public static int lastHoveredCensusNPC = -1;
		public static List<Func<Player, int>> HoveredNPCFinders = [
			player => player.talkNPC > -1 ? Main.npc[player.talkNPC].type : -1,
			_ => UILinkPointNavigator.Shortcuts.NPCS_LastHovered > -1 ? Main.npc[UILinkPointNavigator.Shortcuts.NPCS_LastHovered].type : -1,
			_ => lastHoveredCensusNPC
		];
		public override void Load() {
			checkHappinessData = KeybindLoader.RegisterKeybind(this, "Check Happiness Data", "Mouse5");
		}
		public override void PostSetupContent() {
			if (ModLoader.TryGetMod("Census", out Mod census)) {
				MonoModHooks.Modify(census.Code.GetType("Census.CensusSystem").GetMethod("<ModifyInterfaceLayers>b__8_2", BindingFlags.NonPublic | BindingFlags.Instance), il => {
					ILCursor c = new(il);
					FieldInfo info = typeof(NPCHappinessViewer).GetField(nameof(lastHoveredCensusNPC));
					c.EmitLdcI4(-1);
					c.EmitStsfld(info);
					while (c.TryGotoNext(MoveType.Before, i => i.MatchCall<Lang>(nameof(Lang.GetNPCNameValue)))) {
						c.EmitDup();
						c.EmitStsfld(info);
						c.Index++;
					}
				});
			}
		}
		public override void Unload() {
			checkHappinessData = null;
			HoveredNPCFinders = null;
		}
	}
	public class NPCHappinessViewerPlayer : ModPlayer {
		public int ChosenNPC {
			get {
				for (int i = 0; i < NPCHappinessViewer.HoveredNPCFinders.Count; i++) {
					int current = NPCHappinessViewer.HoveredNPCFinders[i](Player);
					if (current > -1) return current;
				}
				return -1;
			}
		}
		public override void ProcessTriggers(TriggersSet triggersSet) {
			if (NPCHappinessViewer.checkHappinessData.JustPressed) {
				int npcType = ChosenNPC;
				if (npcType > -1) {
					PersonalityProfile profile = ((PersonalityDatabase)typeof(ShopHelper).GetField("_database", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(Main.ShopHelper)).GetOrCreateProfileByNPCID(npcType);
					if (profile is not null) {
						string text = Lang.GetNPCNameValue(npcType) + ":\n";
						foreach (var modifier in profile.ShopModifiers) {
							if (modifier is BiomePreferenceListTrait biomePreferences) {
								foreach (var biome in biomePreferences.Preferences) {
									text += $"{biome.Affection}s {Language.GetTextValue(biome.Biome.NameKey)},\n";
								}
							} else if (modifier is NPCPreferenceTrait npcPreference) {
								text += $"{npcPreference.Level}s {Lang.GetNPCNameValue(npcPreference.NpcId)},\n";
							}
						}
						Main.NewText(text.Trim());
					}
				}
			}
		}
	}
}