using CitizenFX.Core;
using CitizenFX.Core.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Client
{
	class Main : BaseScript
	{
		int deadTimer;
		int revDelay;
		Vector3 respawnLocation;

		public Main()
		{
			JObject cfg = JObject.Parse(LoadResourceFile(GetCurrentResourceName(), "config/config.json"));

			revDelay = (int)cfg.SelectToken("reviveRespawnTimer");
			respawnLocation = new Vector3((float)cfg.SelectToken("commands.respawn.respawnCoords.x"),
				(float)cfg.SelectToken("commands.respawn.respawnCoords.y"),
				(float)cfg.SelectToken("commands.respawn.respawnCoords.z"));
		}

		[EventHandler("BadgerEssentials:Revive")]
		private void Revive(bool bypassTimer)
		{
			Ped ped = Game.PlayerPed;

			if (IsEntityDead(ped.Handle))
			{
				if (bypassTimer)
				{
					NetworkResurrectLocalPlayer(ped.Position.X, ped.Position.Y, ped.Position.Z, Game.PlayerPed.Heading, true, false);
					ClearPedBloodDamage(ped.Handle);

					TriggerEvent("chat:addMessage", new
					{
						color = new[] { 255, 0, 0 },
						multiline = true,
						args = new[] { $"^1[BadgerEssentials] ^3You have been revived!" }
					});
				}
				else if (deadTimer > 0)
				{

					TriggerEvent("chat:addMessage", new
					{
						color = new[] { 255, 0, 0 },
						multiline = true,
						args = new[] { $"^1[BadgerEssentials] ^3You cannot be revived for ^5{deadTimer} ^3more seconds!" }
					});
				}
				else if (deadTimer <= 0)
				{
					NetworkResurrectLocalPlayer(ped.Position.X, ped.Position.Y, ped.Position.Z, Game.PlayerPed.Heading, true, false);
					ClearPedBloodDamage(ped.Handle);

					TriggerEvent("chat:addMessage", new
					{
						color = new[] { 255, 0, 0 },
						multiline = true,
						args = new[] { $"^1[BadgerEssentials] ^3You have been revived!" }
					});
				}
			}
		}

		[EventHandler("BadgerEssentials:Respawn")]
		private void Respawn(bool bypassTimer)
		{
			Ped ped = Game.PlayerPed;

			if (IsEntityDead(ped.Handle))
			{
				if (bypassTimer)
				{
					NetworkResurrectLocalPlayer(ped.Position.X, ped.Position.Y, ped.Position.Z, ped.Heading, true, false);
					ClearPedBloodDamage(ped.Handle);
					SetEntityCoords(ped.Handle, respawnLocation.X, respawnLocation.Y, respawnLocation.Z, false, false, false, false);

					TriggerEvent("chat:addMessage", new
					{
						color = new[] { 255, 0, 0 },
						multiline = true,
						args = new[] { $"^1[BadgerEssentials] ^3You have been respawned!" }
					});
				}
				else if (deadTimer > 0)
				{
					TriggerEvent("chat:addMessage", new
					{
						color = new[] { 255, 0, 0 },
						multiline = true,
						args = new[] { $"^1[BadgerEssentials] ^3You cannot be respawned for ^5{deadTimer} ^3more seconds!" }
					});
				}
				else if (deadTimer <= 0)
				{
					TriggerEvent("chat:addMessage", new
					{
						color = new[] { 255, 0, 0 },
						multiline = true,
						args = new[] { $"^1[BadgerEssentials] ^3You have been respawned!" }
					});

					NetworkResurrectLocalPlayer(ped.Position.X, ped.Position.Y, ped.Position.Z, ped.Heading, true, false);
					ClearPedBloodDamage(ped.Handle);
					SetEntityCoords(ped.Handle, respawnLocation.X, respawnLocation.Y, respawnLocation.Z, false, false, false, false);
				}
			}
		}

		bool isDead;

		[Tick]
		private async Task DeathCheck()
		{
			int ped = PlayerPedId();

			if (IsEntityDead(ped))
			{
				isDead = true;
			}
			else
			{
				isDead = false;
				deadTimer = revDelay;
			}
		}

		[Tick]
		private async Task DeadTimer()
		{
			if (isDead && deadTimer > 0)
			{
				await Delay(1000);
				deadTimer--;
			}
		}
	}
}