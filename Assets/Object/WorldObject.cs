using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RTS;

public class WorldObject : MonoBehaviour
{

		public string objectName;
		public Texture2D buildImage;
		public int cost;
		public int sellValue;
		public int hitPoints;
		public int maxHitPoints;
	
		protected Player player;
		protected string[] actions = {};
		protected bool currentlySelected = false;
		protected Bounds selectionBounds;
		protected Rect playingArea = new Rect (0.0f, 0.0f, 0.0f, 0.0f);
		protected GUIStyle healthStyle = new GUIStyle ();
		protected float healthPercentage = 1.0f;

		private List<Material> oldMaterials = new List<Material> ();

		protected virtual void Awake ()
		{
				selectionBounds = ResourceManager.InvalidBounds;
				CalculateBounds ();

		}

		// Use this for initialization
		protected virtual void Start ()
		{
				SetPlayer ();
		}
	
		// Update is called once per frame
		protected virtual void Update ()
		{
	
		}

		protected virtual void OnGUI ()
		{
				if (currentlySelected)
						DrawSelection ();
		}
		
		protected virtual void CalculateCurrentHealth (float lowSplit, float highSplit)
		{
				healthPercentage = (float)hitPoints / (float)maxHitPoints;
				if (healthPercentage > highSplit)
						healthStyle.normal.background = ResourceManager.HealthyTexture;
				else if (healthPercentage > lowSplit)
						healthStyle.normal.background = ResourceManager.DamagedTexture;
				else
						healthStyle.normal.background = ResourceManager.CriticalTexture;
		}

		public virtual void SetSelection (bool selected, Rect playingArea)
		{
				currentlySelected = selected;
				if (selected)
						this.playingArea = playingArea;
		}

		public string[] GetActions ()
		{
				return actions;
		}

		public virtual void PerformAction (string action)
		{

		}

		public virtual void MouseClick (GameObject hitObject, Vector3 hitPoint, Player controller)
		{
				if (currentlySelected && hitObject && hitObject.name != "Ground") {
						WorldObject worldObject = hitObject.transform.parent.GetComponent < WorldObject> ();
						if (worldObject) {
								Resource resource = hitObject.transform.parent.GetComponent<Resource> ();
								if (resource && resource.isEmpty ())
										return;
								ChangeSelection (worldObject, controller);
						}
				}
		}

		private void ChangeSelection (WorldObject worldObject, Player controller)
		{
				SetSelection (false, playingArea);
				if (controller.SelectedObject)
						controller.SelectedObject.SetSelection (false, playingArea);
				controller.SelectedObject = worldObject;
				worldObject.SetSelection (true, controller.hud.GetPlayingArea ());
		}

		private void DrawSelection ()
		{
				GUI.skin = ResourceManager.SelectBoxSkin;
				Rect selectBox = WorkManager.CalculateSelectionBox (selectionBounds, playingArea);
				GUI.BeginGroup (playingArea);
				DrawSelectionBox (selectBox);
				GUI.EndGroup ();
			
		}

		public void CalculateBounds ()
		{
				selectionBounds = new Bounds (transform.position, Vector3.zero);
				foreach (Renderer r in GetComponentsInChildren<Renderer>()) {
						selectionBounds.Encapsulate (r.bounds);
				}
		}

		public virtual void DrawSelectionBox (Rect selectBox)
		{
				GUI.Box (selectBox, "");
				CalculateCurrentHealth (.35f, 0.65f);
				DrawHealthBar (selectBox, "");
		}
		protected void DrawHealthBar (Rect selectBox, string label)
		{
				healthStyle.padding.top = -20;
				healthStyle.fontStyle = FontStyle.Bold;
				GUI.Label (new Rect (selectBox.x, selectBox.y - 7, selectBox.width * healthPercentage, 5), label, healthStyle);
		}

		public virtual void SetHoverState (GameObject hoverObject)
		{
				if (player && player.human && currentlySelected) {
						if (hoverObject.name != "Ground")
								player.hud.SetCursorState (CursorState.Select);
				}
		}

		public bool IsOwnedBy (Player owner)
		{
				if (player && player.Equals (owner)) {
						return true;
				} else
						return false;
		}
		
		public Bounds GetSelectionBounds ()
		{
				return selectionBounds;
		}

		public void SetColliders (bool enabled)
		{
				Collider[] colliders = GetComponentsInChildren<Collider> ();
				foreach (Collider collider in colliders)
						collider.enabled = enabled;
		}

		public void SetTransparentMaterial (Material material, bool storeExistingMaterial)
		{
				if (storeExistingMaterial)
						oldMaterials.Clear ();
				Renderer[] renderers = GetComponentsInChildren<Renderer> ();
				foreach (Renderer renderer in renderers) {
						if (storeExistingMaterial)
								oldMaterials.Add (renderer.material);
						renderer.material = material;
				}
		}

		public void RestoreMaterials ()
		{
				Renderer[] renderers = GetComponentsInChildren<Renderer> ();
				if (oldMaterials.Count == renderers.Length) {
						for (int i = 0; i < renderers.Length; i++) {
								renderers [i].material = oldMaterials [i];
						}
				}
		}
		public void SetPlayingArea (Rect playingArea)
		{
				this.playingArea = playingArea;
		}

		public void SetPlayer ()
		{
				player = transform.root.GetComponentInChildren<Player> ();
		}
}
