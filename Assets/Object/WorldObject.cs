using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RTS;

public class WorldObject : MonoBehaviour
{

		public string objectName;
		public Texture2D buildImage;
		public int cost = 100;
		public int sellValue = 75;
		public int hitPoints = 100;
		public int maxHitPoints = 100;
		public float weaponRange = 10.0f;
		public float weaponAimSpeed = 5f;
		public float weaponRechargeTime = 1.0f;

		protected Player player;
		protected string[] actions = {};
		protected bool currentlySelected = false;
		protected Bounds selectionBounds;
		protected Rect playingArea = new Rect (0.0f, 0.0f, 0.0f, 0.0f);
		protected GUIStyle healthStyle = new GUIStyle ();
		protected float healthPercentage = 1.0f;
		protected WorldObject target = null;
		protected bool attacking = false;
		protected bool movingIntoPosition = false;
		protected bool aiming = false;

		private List<Material> oldMaterials = new List<Material> ();
		private float currentWeaponChargeTime;

		protected virtual void Awake ()
		{
				selectionBounds = ResourceManager.InvalidBounds;
				CalculateBounds ();

		}

		// Use this for initialization
		protected virtual void Start ()
		{
				SetPlayer ();
				if (player)
						SetTeamColor ();
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

		protected void SetTeamColor ()
		{
				TeamColor[] teamColors = GetComponentsInChildren<TeamColor> ();
				foreach (TeamColor teamColor in teamColors)
						teamColor.renderer.material.color = player.teamColor;
		}

		protected virtual void BeginAttack (WorldObject target)
		{
				this.target = target;
				if (TargetInRange ()) {
						attacking = true;
						PerformAttack ();
				} else
						AdjustPosition ();
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
								Player owner = hitObject.transform.root.GetComponent<Player> ();
								if (owner) { //this object is controlled by a player
										if (player && player.human) { // the player is human
												//start attack if object is not owned by the same player and CanAttack()
												if (player.name != owner.name && CanAttack ())
														BeginAttack (worldObject);
												else
														ChangeSelection (worldObject, controller);
										} else
												ChangeSelection (worldObject, controller);
								} else
										ChangeSelection (worldObject, controller);
						}
				}
		}

		private bool TargetInRange ()
		{
				Vector3 targetLocation = target.transform.position;
				Vector3 direction = targetLocation - transform.position;
				if (direction.sqrMagnitude < weaponRange * weaponRange) {
						return true;
				}
				return false;
		}

		private void AdjustPosition ()
		{
				Unit self = this as Unit;
				if (self) {
						movingIntoPosition = true;
						Vector3 attackPosition = FindNearestAttackPosition ();
						self.StartMove (attackPosition);
						attacking = true;
				} else
						attacking = false;
		}

		private Vector3 FindNearestAttackPosition ()
		{
				Vector3 targetLocation = target.transform.position;
				Vector3 direction = targetLocation - transform.position;
				float targetDistance = direction.magnitude;
				float distanceToTravel = targetDistance - (0.9f * weaponRange);
				return Vector3.Lerp (transform.position, targetLocation, distanceToTravel / targetDistance);
		}

		private bool TargetInFrontOfWeapon ()
		{
				Vector3 targetLocation = target.transform.position;
				Vector3 direction = targetLocation - transform.position;
				if (direction.normalized == transform.forward.normalized)
						return true;
				else
						return false;
		}

		private void PerformAttack ()
		{
				if (!target) {
						attacking = false;
						return;
				}
				if (!TargetInRange ())
						AdjustPosition ();
				else  if (!TargetInFrontOfWeapon ())
						AimAtTarget ();
				else if (ReadyToFire ())
						UseWeapon ();
		}
		protected virtual void UseWeapon ()
		{
				currentWeaponChargeTime = 0.0f;
		}

		private bool ReadyToFire ()
		{
				if (currentWeaponChargeTime >= weaponRechargeTime)
						return true;
				return false;
		}

		protected virtual void AimAtTarget ()
		{
				aiming = true;
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
						//something other than the ground is being hovered over
						if (hoverObject.name != "Ground") {
								Player owner = hoverObject.transform.root.GetComponent< Player > ();
								Unit unit = hoverObject.transform.parent.GetComponent< Unit > ();
								Building building = hoverObject.transform.parent.GetComponent< Building > ();
								if (owner) { //the object is owned by a player
										if (owner.name == player.name) {
												player.hud.SetCursorState (CursorState.Select);
												Debug.Log ("WorldObject select 1");
										} else if (CanAttack ()) {
												player.hud.SetCursorState (CursorState.Attack);
										} else {
												player.hud.SetCursorState (CursorState.Select);
										}
								} else {
										if (unit || building && CanAttack ())
												player.hud.SetCursorState (CursorState.Attack);
										else 
												player.hud.SetCursorState (CursorState.Select);
								}
						}
				}
		}

		public virtual bool CanAttack ()
		{
				//return false by default
				return false;
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
