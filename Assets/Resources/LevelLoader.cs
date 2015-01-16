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
		}

		void OnLevelWasLoaded ()
		{
				if (initialized) {
						WorldObject[] worldObjects = GameObject.FindObjectsOfType (typeof(WorldObject)) as WorldObject[];

						foreach (WorldObject worldObject in worldObjects) {
								worldObject.ObjectId = nextObjectId++;
								if (nextObjectId >= int.MaxValue)
										nextObjectId = 0;
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
