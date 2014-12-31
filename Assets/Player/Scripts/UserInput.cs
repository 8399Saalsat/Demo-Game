using UnityEngine;
using System.Collections;
using RTS;

public class UserInput : MonoBehaviour
{
		private Player player;
		// Use this for initialization
		void Start ()
		{
				player = transform.root.GetComponent<Player> ();
		}
	
		// Update is called once per frame
		void Update ()
		{
				MouseActivity ();
		}

		private void MouseActivity ()
		{
				if (Screen.showCursor == true) {
						if (Input.GetMouseButtonDown (0)) {
								LeftMouseClick ();
						} else if (Input.GetMouseButtonDown (1)) {
								RightMouseClick ();
						}
				}
		}
	
		private void LeftMouseClick ()
		{
				if (player.hud.MouseInBounds ()) {
						GameObject hitObject = FindHitObject ();
						Vector3 hitPoint = FindHitPoint ();
						if (hitObject && hitPoint != ResourceManager.InvalidPosition) {
								if (player.SelectedObject)
										player.SelectedObject.MouseClick (hitObject, hitPoint, player);
								else if (hitObject.name != "Ground") {
										WorldObject worldObject = hitObject.transform.root.GetComponent <WorldObject > ();
										if (worldObject) {
												player.SelectedObject = worldObject;
												worldObject.SetSelection (true);
										}
								}
						}
				}
		
		}
		private void RightMouseClick ()
		{
				if (player.hud.MouseInBounds () && player.SelectedObject) {
						player.SelectedObject.SetSelection (false);
						player.SelectedObject = null;
				}
		}

		private GameObject FindHitObject ()
		{
				Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
				RaycastHit hit;
				if (Physics.Raycast (ray, out hit))
						return hit.collider.gameObject;
				else
						return null;
		}

		private Vector3 FindHitPoint ()
		{
				Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
				RaycastHit hit;
				if (Physics.Raycast (ray, out hit))
						return hit.point;
				else
						return ResourceManager.InvalidPosition;
		}
}
