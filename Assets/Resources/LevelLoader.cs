using UnityEngine;
using System.Collections;
using RTS;

public class LevelLoader : MonoBehaviour
{

		private static int nextObjectId = 0;
		private static bool created = false;
		private bool initialized = false;

		void Awake ()
		{
				if (!created) {
						DontDestroyOnLoad (transform.gameObject);
						created = true;
						initialized = true;
				} else {
						Destroy (this.gameObject);
				}
				if (initialized) {
						SelectPlayerMenu menu = GameObject.FindObjectOfType (typeof(SelectPlayerMenu)) as SelectPlayerMenu;
						if (!menu) {
								//game was lauched inside a map rather than the main menu
								Player[] players = GameObject.FindObjectsOfType (typeof(Player)) as Player[];
								foreach (Player player in players) {
										if (player.human) {
												PlayerManager.SelectPlayer (player.playerName, 0);
										}
								}
						}
				}
		}

		void OnLevelWasLoaded ()
		{
				if (initialized) {
						if (ResourceManager.LevelName != null && ResourceManager.LevelName != "") {
								LoadManager.LoadGame (ResourceManager.LevelName);
						} else {
								WorldObject[] worldObjects = GameObject.FindObjectsOfType (typeof(WorldObject)) as WorldObject[];

								foreach (WorldObject worldObject in worldObjects) {
										worldObject.ObjectId = nextObjectId++;
										if (nextObjectId >= int.MaxValue)
												nextObjectId = 0;
								}
						}
				}
		}

		public int GetNewObjectId ()
		{
				nextObjectId++;
				if (nextObjectId >= int.MaxValue)
						nextObjectId = 0;
				return nextObjectId;
		}
}
