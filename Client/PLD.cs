using CitizenFX.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Client
{
	public struct Postal
	{
		public string Code { get; }
		public float X { get; }
		public float Y { get; }
		public Vector2 Position { get => new Vector2(X, Y); }

		[JsonConstructor]
		public Postal(string code, float x, float y)
		{
			Code = code;
			X = x;
			Y = y;
		}

		public Postal(string code, Vector2 location)
		{
			Code = code;
			X = location.X;
			Y = location.Y;
		}
	}

	class PLD : BaseScript
	{
		public static List<Postal> postalList;

		public static string nearestPostalCode;
		public static string nearestPostalDistance;

		public static string streetName;
		public static string crossStreetName;
		public static string zoneName;
		public static string heading;
		public static string crossStreetSlash;

		uint streetHash;
		uint crossStreetHash;

		public PLD()
		{
			string postalsJson = LoadResourceFile(GetCurrentResourceName(), "config/postals.json");
			postalList = JsonConvert.DeserializeObject<List<Postal>>(postalsJson);
		}

		[Tick]
		private async Task CalculateLocation()
		{
			await Delay(350);

			//
			// Postal
			//
			Vector3 pos = Game.PlayerPed.Position;

			List<float> distances = new List<float>();

			foreach (Postal p in postalList)
			{
				float dist = Vector2.Distance((Vector2)pos, p.Position);
				distances.Add(dist);
			}

			int nearestPostalIndex = distances.IndexOf(distances.Min());
			nearestPostalDistance = distances.Min().ToString("n1");
			nearestPostalCode = postalList[nearestPostalIndex].Code;

			//
			// PLD
			//
			GetStreetNameAtCoord(pos.X, pos.Y, pos.Z, ref streetHash, ref crossStreetHash);
			streetName = GetStreetNameFromHashKey(streetHash);
			crossStreetName = GetStreetNameFromHashKey(crossStreetHash);
			zoneName = GetLabelText(GetNameOfZone(pos.X, pos.Y, pos.Z));

			if (String.IsNullOrEmpty(crossStreetName))
			{
				crossStreetSlash = String.Empty;
			}
			else crossStreetSlash = "/";

			// PLD Heading
			float rawHeading = Game.PlayerPed.Heading;

			if (rawHeading <= 337.5 && rawHeading > 292.5)
				heading = "NE";
			else if (rawHeading <= 292.5 && rawHeading > 247.5)
				heading = "E";
			else if (rawHeading <= 247.5 && rawHeading > 202.5)
				heading = "SE";
			else if (rawHeading <= 202.5 && rawHeading > 157.5)
				heading = "S";
			else if (rawHeading <= 157.5 && rawHeading > 112.5)
				heading = "SW";
			else if (rawHeading <= 112.5 && rawHeading > 67.5)
				heading = "W";
			else if (rawHeading <= 67.5 && rawHeading > 22.5)
				heading = "NW";
			else
				heading = "N";
		}
	}
}