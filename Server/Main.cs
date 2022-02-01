using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace Server
{
	class Main : BaseScript
	{
		public enum PcStatus
		{
			Cooldown = 0,
			OnHold = 1,
			Active = 2,
			None = 3
		}

		static JObject cfg = JObject.Parse(LoadResourceFile(GetCurrentResourceName(), "config/config.json"));
		public static string aop = (string)cfg.SelectToken("commands.aop.defaultAOP");
		public static bool peacetime = false;
		public static int priorityStatus = (int)PcStatus.None;
		public static int priorityTime = 0;

		public static void UpdateClientInfo()
		{
			TriggerClientEvent("BadgerEssentials:UpdatePeacetime", peacetime);
			TriggerClientEvent("BadgerEssentials:UpdatePriority", priorityStatus, priorityTime.ToString());
			TriggerClientEvent("BadgerEssentials:UpdateAOP", aop);
		}

		[EventHandler("BadgerEssentialsServer:GetAOP")]
		private void UpdateClientInfo([FromSource] Player player)
		{
			TriggerClientEvent(player, "BadgerEssentials:UpdatePeacetime", peacetime);
			TriggerClientEvent(player, "BadgerEssentials:UpdatePriority", priorityStatus, priorityTime.ToString());
			TriggerClientEvent(player, "BadgerEssentials:UpdateAOP", aop);
		}

		[Tick]
		private async Task PriorityTimer()
		{
			if (priorityStatus == (int)PcStatus.Cooldown && priorityTime > 0)
			{
				await Delay(60000);
				priorityTime--;
				TriggerClientEvent("BadgerEssentials:UpdatePriority", (int)priorityStatus, priorityTime.ToString());
			}
			else if (priorityStatus == (int)PcStatus.Cooldown && priorityTime <= 0)
			{
				priorityStatus = (int)PcStatus.None;
				TriggerClientEvent("BadgerEssentials:UpdatePriority", (int)priorityStatus, priorityTime.ToString());

				TriggerClientEvent("chat:addMessage", new
				{
					color = new[] { 255, 0, 0 },
					multiline = true,
					args = new[] { $"^1[BadgerEssentials] ^3Priorities have been ^5reset^3!" }
				});
			}
		}
	}

	class Messages : BaseScript
	{
		public static void NoPermission(Player player)
		{
			TriggerClientEvent(player, "chat:addMessage", new
			{
				color = new[] { 255, 0, 0 },
				multiline = true,
				args = new[] { "^1[BadgerEssentials] ^3You do not have permission to use this command!" }
			});
		}
	}
}