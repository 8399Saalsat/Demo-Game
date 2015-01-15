using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;

namespace RTS
{
		public static class SaveManager
		{
				public static void SaveGame (string filename)
				{
						JsonSerializer serializer = new JsonSerializer ();
						serializer.NullValueHandling = NullValueHandling.Ignore;
						Directory.CreateDirectory ("SavedGames");
						char separator = Path.DirectorySeparatorChar;
						string path = "SavedGames" + separator + PlayerManager.GetPlayerName () + separator + filename + ".json";
						using (StreamWriter sw = new StreamWriter(path)) {
								using (JsonWriter writer = new JsonTextWriter(sw)) {
										writer.WriteStartObject ();
										SaveGameDetails (writer);
										writer.WriteEndObject ();
								}
						}
				}
				private static void SaveGameDetails (JsonWriter writer)
				{

				}
		}
}