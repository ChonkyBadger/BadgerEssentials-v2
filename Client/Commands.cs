using CitizenFX.Core;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Client
{
	class Commands : BaseScript
	{
		public Commands()
		{
			JObject cfg = JObject.Parse(LoadResourceFile(GetCurrentResourceName(), "config/config.json"));

			if ((bool)cfg.SelectToken("commands.die.enabled"))
			{
				RegisterCommand("die", new Action<int, List<object>, string>(Die), false);
				TriggerEvent("chat:addSuggestion", "/die", "Kills yourself.");
			}

			if ((bool)cfg.SelectToken("commands.togglehud.enabled"))
			{
				RegisterCommand("togglehud", new Action<int, List<object>, string>(ToggleHud), false);
				TriggerEvent("chat:addSuggestion", "/togglehud", "Toggles hud.");
			}

			if ((bool)cfg.SelectToken("commands.postal.enabled"))
			{
				RegisterCommand("postal", new Action<int, List<object>, string>(Postal), false);
				TriggerEvent("chat:addSuggestion", "/postal", "Sets a waypoint to a specified postal", new[]
				{
					new { name="Postal Code", help="Postal code that you want a waypoint set to." }
				});
			}

			if ((bool)cfg.SelectToken("ragdoll.enabled"))
			{
				RegisterCommand("ragdoll", new Action(ToggleRagdoll), false);
				RegisterKeyMapping("ragdoll", "Toggle ragdoll", "keyboard", (string)cfg.SelectToken("ragdoll.key"));
				TriggerEvent("chat:addSuggestion", "/ragdoll", "Toggles ragdoll.");
			}

			//
			// Server Command chat suggestions
			//

			if ((bool)cfg.SelectToken("commands.revive.enabled"))
			{
				TriggerEvent("chat:addSuggestion", "/revive", "Revives yourself if dead.");
			}

			if ((bool)cfg.SelectToken("commands.respawn.enabled"))
			{
				TriggerEvent("chat:addSuggestion", "/respawn", "Respawns yourself if dead.");
			}

			if ((bool)cfg.SelectToken("commands.announce.enabled"))
			{
				TriggerEvent("chat:addSuggestion", "/announce", "helptext", new[]
				{
					new { name="Message", help="Send an announcement message to all players on the server." }
				});
			}

			if ((bool)cfg.SelectToken("commands.pt.enabled"))
			{
				TriggerEvent("chat:addSuggestion", "/pt", "Toggle peacetime");
			}

			if ((bool)cfg.SelectToken("commands.pc.enabled"))
			{
				TriggerEvent("chat:addSuggestion", "/pc", "Enable priority cooldown for a set duration.", new[]
				{
					new { name="Duration", help="Priority cooldown duration in seconds." }
				});
				TriggerEvent("chat:addSuggestion", "/pc-active", "Sets priority status to Active.");
				TriggerEvent("chat:addSuggestion", "/pc-onhold", "Set priority status to On hold.");
				TriggerEvent("chat:addSuggestion", "/pc-reset", "Sets priority status to Inactive.");
			}
		}

		private void Die(int source, List<object> args, string raw)
		{
			Game.PlayerPed.Kill();
		}

		private void ToggleHud(int source, List<object> args, string raw)
		{
			if (Hud.enabledHud)
			{
				Hud.enabledHud = false;
			}
			else Hud.enabledHud = true;
		}

		private void Postal(int source, List<object> args, string raw)
		{
			if (args.Count > 0)
			{
				Postal postal = PLD.postalList.Find(p => p.Code == args[0].ToString());
				SetNewWaypoint(postal.X, postal.Y);

				TriggerEvent("chat:addMessage", new
				{
					color = new[] { 255, 0, 0 },
					multiline = true,
					args = new[] { $"^1[BadgerEssentials] ^3Waypoint set to postal ^5{postal.Code}" }
				});
			}
		}

		bool ragdolled = false;

		private void ToggleRagdoll()
		{
			if (ragdolled)
			{
				ragdolled = false;
			}
			else ragdolled = true;
		}

		[Tick]
		private async Task OnTick()
		{
			int ped = PlayerPedId();

			if (ragdolled)
			{
				SetPedToRagdoll(ped, 500, 500, 0, true, true, false);
			}
		}
	}
}