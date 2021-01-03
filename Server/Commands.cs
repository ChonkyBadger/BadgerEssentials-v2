using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Server
{
	public class Commands : BaseScript
	{
		public Commands()
		{
			JObject cfg = JObject.Parse(LoadResourceFile(GetCurrentResourceName(), "config/config.json"));

			if ((bool)cfg.SelectToken("commands.revive.enabled"))
			{
				RegisterCommand("revive", new Action<int, List<object>, string>(Revive), false);
			}
			if ((bool)cfg.SelectToken("commands.respawn.enabled"))
			{
				RegisterCommand("respawn", new Action<int, List<object>, string>(Respawn), false);
			}
			if ((bool)cfg.SelectToken("commands.announce.enabled"))
			{
				RegisterCommand("announce", new Action<int, List<object>, string>(Announce), false);
			}
			if ((bool)cfg.SelectToken("commands.pt.enabled"))
			{
				RegisterCommand("pt", new Action<int, List<object>, string>(PeaceTime), false);
			}
			if ((bool)cfg.SelectToken("commands.pc.enabled"))
			{
				RegisterCommand("pc", new Action<int, List<object>, string>(Priority), false);
				RegisterCommand("pc-active", new Action<int, List<object>, string>(PriorityActive), false);
				RegisterCommand("pc-onhold", new Action<int, List<object>, string>(PriorityOnHold), false);
				RegisterCommand("pc-reset", new Action<int, List<object>, string>(PriorityReset), false);
			}
			if ((bool)cfg.SelectToken("commands.aop.enabled"))
			{
				RegisterCommand("aop", new Action<int, List<object>, string>(AOP), false);
			}
		}

		private void Revive(int source, List<object> args, string raw)
		{
			Player player = Players[source];

			if (args.Count > 0)
			{
				if (int.TryParse(args[0].ToString(), out int targetId))
				{
					if (targetId == source)
					{
						if (IsPlayerAceAllowed(player.Handle, "BadgerEssentials.Bypass.ReviveTimer"))
						{
							TriggerClientEvent(player, "BadgerEssentials:Revive", true);
						}
						else TriggerClientEvent(player, "BadgerEssentials:Revive", false);
					}
					else
					{
						TriggerClientEvent(Players[targetId], "BadgerEssentials:Revive", true);
					}
				}
				else
				{
					TriggerClientEvent(player, "chat:addMessage", new
					{
						color = new[] { 255, 0, 0 },
						multiline = true,
						args = new[] { $"^1[BadgerEssentials] ^3Argument given is invalid!" }
					});
				}
			}
			else
			{
				if (IsPlayerAceAllowed(player.Handle, "BadgerEssentials.Bypass.ReviveTimer"))
				{
					TriggerClientEvent(player, "BadgerEssentials:Revive", true);
				}
				else TriggerClientEvent(player, "BadgerEssentials:Revive", false);
			}
		}

		private void Respawn(int source, List<object> args, string raw)
		{
			Player player = Players[source];

			if (args.Count > 0)
			{
				if (int.TryParse(args[0].ToString(), out int targetId))
				{
					if (targetId == source)
					{
						if (IsPlayerAceAllowed(player.Handle, "BadgerEssentials.Bypass.ReviveTimer"))
						{
							TriggerClientEvent(player, "BadgerEssentials:Respawn", true);
						}
						else TriggerClientEvent(player, "BadgerEssentials:Respawn", false);
					}
					else
					{
						TriggerClientEvent(Players[targetId], "BadgerEssentials:Respawn", true);
					}
				}
				else
				{
					TriggerClientEvent(player, "chat:addMessage", new
					{
						color = new[] { 255, 0, 0 },
						multiline = true,
						args = new[] { $"^1[BadgerEssentials] ^3Argument given is invalid!" }
					});
				}
			}
			else
			{
				if (IsPlayerAceAllowed(player.Handle, "BadgerEssentials.Bypass.ReviveTimer"))
				{
					TriggerClientEvent(player, "BadgerEssentials:Respawn", true);
				}
				else TriggerClientEvent(player, "BadgerEssentials:Respawn", false);
			}
		}

		private void Announce(int source, List<object> args, string raw)
		{
			Player player = Players[source];
			if (IsPlayerAceAllowed(player.Handle, "BadgerEssentials.Commands") || IsPlayerAceAllowed(player.Handle, "BadgerEssentials.Command.Announce"))
			{
				if (args.Count > 0)
				{
					string msg = String.Join(" ", args);
					TriggerClientEvent("BadgerEssentials:Announce", msg);
				}
				else
				{
					TriggerClientEvent("chat:addMessage", new
					{
						color = new[] { 255, 0, 0 },
						multiline = true,
						args = new[] { $"^1[BadgerEssentials] ^3You must type in a message that you want to announce!" }
					});
				}
			}
			else
			{
				Messages.NoPermission(player);
			}
		}

		private void PeaceTime(int source, List<object> args, string raw)
		{
			Player player = Players[source];
			if (IsPlayerAceAllowed(player.Handle, "BadgerEssentials.Commands") || IsPlayerAceAllowed(player.Handle, "BadgerEssentials.Command.Peacetime"))
			{
				if (Main.peacetime)
				{
					Main.peacetime = false;
					TriggerClientEvent("chat:addMessage", new
					{
						color = new[] { 255, 0, 0 },
						multiline = true,
						args = new[] { $"^1[BadgerEssentials] ^5Peacetime ^3is no longer in effect!" }
					});
				}
				else
				{
					Main.peacetime = true;
					TriggerClientEvent("chat:addMessage", new
					{
						color = new[] { 255, 0, 0 },
						multiline = true,
						args = new[] { $"^1[BadgerEssentials] ^5Peacetime ^3is now in effect!" }
					});
				}

				Main.UpdateClientInfo();
			}
			else
			{
				Messages.NoPermission(player);
			}
		}

		private void Priority(int source, List<object> args, string raw)
		{
			Player player = Players[source];
			if (IsPlayerAceAllowed(player.Handle, "BadgerEssentials.Commands") || IsPlayerAceAllowed(player.Handle, "BadgerEssentials.Command.PC"))
			{
				if (args.Count > 0)
				{
					if (int.TryParse(args[0].ToString(), out int duration))
					{
						Main.priorityTime = duration;
						Main.priorityStatus = (int)Main.PcStatus.Cooldown;
						Main.UpdateClientInfo();

						TriggerClientEvent("chat:addMessage", new
						{
							color = new[] { 255, 0, 0 },
							multiline = true,
							args = new[] { $"^1[BadgerEssentials] ^3Priorities are now on cooldown for ^5{duration} ^3minutes!" }
						});
					}
					else
					{
						TriggerClientEvent(player, "chat:addMessage", new
						{
							color = new[] { 255, 0, 0 },
							multiline = true,
							args = new[] { "^1[BadgerEssentials] ^3You must specify a duration in minutes!" }
						});
					}
				}
				else
				{
					TriggerClientEvent(player, "chat:addMessage", new
					{
						color = new[] { 255, 0, 0 },
						multiline = true,
						args = new[] { "^1[BadgerEssentials] ^3Duration specified was not a valid integer!" }
					});
				}
			}
			else
			{
				Messages.NoPermission(player);
			}
		}

		private void PriorityActive(int source, List<object> args, string raw)
		{
			Player player = Players[source];
			if (IsPlayerAceAllowed(player.Handle, "BadgerEssentials.Commands") || IsPlayerAceAllowed(player.Handle, "BadgerEssentials.Command.PCActive"))
			{
				Main.priorityStatus = (int)Main.PcStatus.Active;
				Main.priorityTime = 0;
				Main.UpdateClientInfo();

				TriggerClientEvent("chat:addMessage", new
				{
					color = new[] { 255, 0, 0 },
					multiline = true,
					args = new[] { $"^1[BadgerEssentials] ^3A ^5priority ^3is now ^5active^3, you may not start any new priorities." }
				});
			}
			else
			{
				Messages.NoPermission(player);
			}
		}

		private void PriorityOnHold(int source, List<object> args, string raw)
		{
			Player player = Players[source];
			if (IsPlayerAceAllowed(player.Handle, "BadgerEssentials.Commands") || IsPlayerAceAllowed(player.Handle, "BadgerEssentials.Command.PCOnHold"))
			{
				Main.priorityStatus = (int)Main.PcStatus.OnHold;
				Main.priorityTime = 0;
				Main.UpdateClientInfo();

				TriggerClientEvent("chat:addMessage", new
				{
					color = new[] { 255, 0, 0 },
					multiline = true,
					args = new[] { $"^1[BadgerEssentials] ^3Priorities are now ^5on hold^3, you may not start any new priorities!" }
				});
			}
			else
			{
				Messages.NoPermission(player);
			}
		}

		private void PriorityReset(int source, List<object> args, string raw)
		{
			Player player = Players[source];
			if (IsPlayerAceAllowed(player.Handle, "BadgerEssentials.Commands") || IsPlayerAceAllowed(player.Handle, "BadgerEssentials.Command.PCReset"))
			{
				Main.priorityStatus = (int)Main.PcStatus.None;
				Main.priorityTime = 0;
				Main.UpdateClientInfo();

				TriggerClientEvent("chat:addMessage", new
				{
					color = new[] { 255, 0, 0 },
					multiline = true,
					args = new[] { $"^1[BadgerEssentials] ^3Priorities have been ^5reset^3!" }
				});
			}
			else
			{
				Messages.NoPermission(player);
			}
		}

		private void AOP(int source, List<object> args, string raw)
		{
			Player player = Players[source];
			if (IsPlayerAceAllowed(player.Handle, "BadgerEssentials.Commands") || IsPlayerAceAllowed(player.Handle, "BadgerEssentials.Command.SetAOP"))
			{
				if (args.Count > 0)
				{
					string newAOP = String.Join(" ", args);

					Main.aop = newAOP;
					Main.UpdateClientInfo();

					TriggerClientEvent("chat:addMessage", new
					{
						color = new[] { 255, 0, 0 },
						multiline = true,
						args = new[] { $"^1[BadgerEssentials] ^3AOP has been changed to ^5{newAOP}^3!" }
					});
				}
				else
				{
					TriggerClientEvent(player, "chat:addMessage", new
					{
						color = new[] { 255, 0, 0 },
						multiline = true,
						args = new[] { "^1[BadgerEssentials] ^3You must specify what you want as the AOP!" }
					});
				}
			}
			else
			{
				Messages.NoPermission(player);
			}
		}
	}
}
