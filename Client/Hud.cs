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
	public struct Display
	{
		public string Text { get; }
		public int Allignment { get; }
		public float X { get; }
		public float Y { get; }
		public float Scale { get; }
		public Vector2 Position { get => new Vector2(X, Y); }

		[JsonConstructor]
		public Display(string text, int allignment, float x, float y, float scale)
		{
			Text = text;
			Allignment = allignment;
			X = x;
			Y = y;
			Scale = scale;
		}

		public Display(string text, int allignment, Vector2 location, float scale)
		{
			Text = text;
			Allignment = allignment;
			X = location.X;
			Y = location.Y;
			Scale = scale;
		}
	}

	class Hud : BaseScript
	{
		List<Display> displayList;

		public static bool enabledHud = true;

		public Hud()
		{
			TriggerServerEvent("BadgerEssentialsServer:GetAOP");

			//
			// Config json
			//
			JObject cfg = JObject.Parse(LoadResourceFile(GetCurrentResourceName(), "config/config.json"));

			ptEnabledText = (string)cfg.SelectToken("commands.pt.enabledText");
			ptDisabledTest = (string)cfg.SelectToken("commands.pt.disabledText");

			pcCooldownText = (string)cfg.SelectToken("commands.pc.cooldownText");
			pcActiveText = (string)cfg.SelectToken("commands.pc.activeText");
			pcOnHoldText = (string)cfg.SelectToken("commands.pc.onHoldText");
			pcDisabledText = (string)cfg.SelectToken("commands.pc.disabledText");

			annDuration = (int)cfg.SelectToken("commands.announce.duration");

			//
			// Displays Json
			//
			string displaysJson = LoadResourceFile(GetCurrentResourceName(), "config/displays.json");
			displayList = JsonConvert.DeserializeObject<List<Display>>(displaysJson);
		}

		string aop;
		string pcText;
		bool ptStatus;
		string ptText;

		[Tick]
		private async Task OnTick()
		{
			int ped = PlayerPedId();

			if (enabledHud)
			{
				foreach (Display d in displayList)
				{
					string text = d.Text.Replace("{playerId}", GetPlayerServerId(NetworkGetEntityOwner(ped)).ToString())
						.Replace("{postalDistance}", PLD.nearestPostalDistance).Replace("{postalCode}", PLD.nearestPostalCode)
						.Replace("{street}", PLD.streetName).Replace("{crossStreet}", PLD.crossStreetName).Replace("{crossStreetSlash}", PLD.crossStreetSlash)
						.Replace("{heading}", PLD.heading).Replace("{zone}", PLD.zoneName).Replace("{pcStatus}", pcText).Replace("{ptStatus}", ptText)
						.Replace("{aop}", aop);

					Draw2DText(text, d.Allignment, d.Position, d.Scale);
				}
			}

			if (ptStatus)
			{
				DisablePlayerFiring(ped, true);
				SetPlayerCanDoDriveBy(ped, false);
				DisableControlAction(0, 140, true); // Melee key "r"

				if (IsControlPressed(0, 106))
				{
					Screen.ShowNotification("~y~[BadgerEssentials]\n" + "~r~Peacetime is enabled! ~w~You can not attack.");
				}
			}
			
			if (annActive)
			{
				Draw2DText("~r~Announcement", 0, 0.5f, 0.175f, 1.2f);
				float baseY = 0.25f;

				for (int i = 1; i <= annLines.Count; i++, baseY += 0.05f)
				{
					Draw2DText("~b~" + annLines[i-1], 0, 0.5f, baseY, 1.2f);
				}
			}
		}

		string ptEnabledText;
		string ptDisabledTest;

		[EventHandler("BadgerEssentials:UpdatePeacetime")]
		private void UpdatePt(bool toggle)
		{
			ptStatus = toggle;

			if (toggle)
			{
				ptText = ptEnabledText;
			}
			else ptText = ptDisabledTest;
		}

		string pcCooldownText;
		string pcActiveText;
		string pcOnHoldText;
		string pcDisabledText;

		[EventHandler("BadgerEssentials:UpdatePriority")]
		private void UpdatePc(int status, string time)
		{
			switch (status)
			{
				case 0:
					pcText = pcCooldownText.Replace("{pcTime}", time);
					break;
				case 1:
					pcText = pcOnHoldText;
					break;
				case 2:
					pcText = pcActiveText;
					break;
				case 3:
					pcText = pcDisabledText;
					break;
			}
		}

		[EventHandler("BadgerEssentials:UpdateAOP")]
		private void UpdateAOP(string newAOP)
		{
			aop = newAOP;
		}

		[EventHandler("onClientMapStart")]
		public void OnClientMapStart()
		{
			Exports["spawnmanager"].spawnPlayer();
			Wait(2500);
			Exports["spawnmanager"].setAutoSpawn(false);
			
			TriggerServerEvent("BadgerEssentialsServer:GetAOP");
		}

		List<string> annLines = new List<string>();
		int annDuration;
		int annTimer;
		bool annActive;

		[EventHandler("BadgerEssentials:Announce")]
		private void Announce(string msg)
		{
			annLines.Clear();

			// Split up message
			if (msg.Length > 70)
			{
				int totalLength = msg.Length;
				string[] words = msg.Split();
				int lineLength = 0;
				string line = String.Empty;
				int reqLines;

				// calculate required lines
				if (msg.Length % 70 == 0)
				{
					reqLines = msg.Length / 70;
				}
				else reqLines = (msg.Length / 70) + 1;

				int curLine = 1;
				bool lastLine = false;

				// split message up into multiple lines
				for (int i = 1; i <= words.Length; i++)
				{
					string s = words[i-1];

					if (!lastLine)
					{
						line += s;
						lineLength += s.Length;
						totalLength -= s.Length;

						if (lineLength >= 70)
						{
							annLines.Add(line);
							line = string.Empty;
							lineLength = 0;
							curLine++;

							if (curLine == reqLines)
							{
								lastLine = true;
							}
						}
						else 
						{
							line += " ";
							lineLength++;
						}
					}
					else
					{
						line += s;
						totalLength -= s.Length;
						if (totalLength <= 0 || i == words.Length)
						{
							annLines.Add(line);
							break;
						}
						else
						{
							line += " ";
							lineLength++;
						}
					}
				}
			}

			// Less than 70 char, dont split it
			else
			{
				annLines.Add(msg);
			}

			annTimer = annDuration;
			annActive = true;
		}

		[Tick]
		private async Task AnnTimer()
		{
			if (annActive && annTimer > 0)
			{
				await Delay(1000);
				annTimer--;
			}
			else if (annTimer <= 0)
			{
				annActive = false;
			}
		}

		//
		// Draw2D Text method and overloads.
		//
		public void Draw2DText(string text, int allignment, float x, float y, float scale)
		{
			SetTextFont(4);
			SetTextScale(scale, scale);
			SetTextColour(255, 255, 255, 255);
			SetTextDropShadow();
			SetTextOutline();
			if (allignment == 0)
				// Center
				SetTextJustification(0);
			else if (allignment == 1)
				// Left
				SetTextJustification(1);
			else if (allignment == 2)
				// Right
				SetTextJustification(2);
			SetTextEntry("STRING");
			AddTextComponentString(text);
			DrawText(x, y);
		}

		public void Draw2DText(string text, int allignment, Vector2 position, float scale)
		{
			SetTextFont(4);
			SetTextScale(scale, scale);
			SetTextColour(255, 255, 255, 255);
			SetTextDropShadow();
			SetTextOutline();
			if (allignment == 0)
				// Center
				SetTextJustification(0); 
			else if (allignment == 1)
				// Left
				SetTextJustification(1);
			else if (allignment == 2)
				// Right
				SetTextJustification(2); 
			SetTextEntry("STRING");
			AddTextComponentString(text);
			DrawText(position.X, position.Y);
		}
	}
}
