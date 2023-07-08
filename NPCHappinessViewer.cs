using System.Reflection;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Personalities;
using Terraria.GameInput;
using Terraria.Localization;
using Terraria.ModLoader;

namespace NPCHappinessViewer {
	public class NPCHappinessViewer : Mod {
		internal static ModKeybind checkHappinessData;
		public override void Load() {
			checkHappinessData = KeybindLoader.RegisterKeybind(this, "Check Happiness Data", "Mouse5");
		}
		public override void Unload() {
			checkHappinessData = null;
		}
	}
	public class NPCHappinessViewerPlayer : ModPlayer {
		public override void ProcessTriggers(TriggersSet triggersSet) {
			if (NPCHappinessViewer.checkHappinessData.JustPressed && Player.talkNPC > -1) {
				PersonalityProfile profile = ((PersonalityDatabase)typeof(ShopHelper).GetField("_database", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(Main.ShopHelper)).GetOrCreateProfileByNPCID(Main.npc[Player.talkNPC].type);
				if(profile is not null){
					string text = Lang.GetNPCNameValue(Main.npc[Player.talkNPC].type)+":\n";
					foreach (var modifier in profile.ShopModifiers) {
						if (modifier is BiomePreferenceListTrait biomePreferences) {
							foreach (var biome in biomePreferences.Preferences) {
								text += $"{biome.Affection}s {Language.GetTextValue(biome.Biome.NameKey)},\n";
							}
						}else if (modifier is NPCPreferenceTrait npcPreference) {
							text += $"{npcPreference.Level}s {Lang.GetNPCNameValue(npcPreference.NpcId)},\n";
						}
					}
					Main.NewText(text.Trim());
				}
			}
		}
	}
}