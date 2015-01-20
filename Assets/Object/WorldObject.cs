using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RTS;
using Newtonsoft.Json;

public class WorldObject : MonoBehaviour
{
		public int ObjectId{ get; set; }

		public string objectName;
		public Texture2D buildImage;
		public int cost = 100;
		public int sellValue = 75;
		public int hitPoints = 100;
		public int maxHitPoints = 100;
		public float weaponRange = 10.0f;
		public float weaponAimSpeed = 5f;
		public float weaponRechargeTime = 1.0f;
		public AudioClip attackSound;
		public AudioClip selectSound;
		public AudioClip useWeaponSound;
		public float attackVolume = 1.0f;
		public float selectVolume = 1.0f;
		public float useWeaponVolume = 1.0f;

		protected AudioElement audioElement;
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
		protected bool loadedSavedValues = false;

		private List<Material> oldMaterials = new List<Material> ();
		private float currentWeaponChargeTime;
		private int loadedTargetId = -1;

		protected virtual void Awake ()
		{
				selectionBounds = ResourceManager.InvalidBounds;
				CalculateBounds ();

		}

		// Use this for initialization
		protected virtual void Start ()
		{
				SetPlayer ();
				if (player) {
						if (loadedSavedValues) {
								if (loadedSavedValues && loadedTargetId >= 0)
										target = player.GetObjectForId (loadedTargetId);
								else
										SetTeamColor ();
						}								
				}
				InitializeAudio ();
		}
	
		// Update is called once per frame
		protected virtual void Update ()
		{
				currentWeaponChargeTime += Time.deltaTime;
				if (attacking && !movingIntoPosition && !aiming)
						PerformAttack ();
		}

		protected virtual void OnGUI ()
		{
				if (currentlySelected && !ResourceManager.MenuOpen)
						DrawSelection ();
		}

		protected virtual void InitializeAudio ()
		{
				List<AudioClip> sounds = new List<AudioClip> ();
				List<float> volumes = new List<float> ();
				if (attackVolume < 0.0f)
						attackVolume = 0.0f;
				if (attackVolume > 1.0f)
						attackVolume = 1.0f;
				sounds.Add (attackSound);
				volumes.Add (attackVolume);

				if (selectVolume < 0.0f)
						selectVolume = 0.0f;
				if (selectVolume > 1.0f)
						selectVolume = 1.0f;
				sounds.Add (selectSound);
				volumes.Add (selectVolume);

				if (useWeaponVolume < 0.0f)
						useWeaponVolume = 0.0f;
				if (useWeaponVolume > 1.0f)
						useWeaponVolume = 1.0f;
				sounds.Add (useWeaponSound);
				volumes.Add (useWeaponVolume);

				audioElement = new AudioElement (sounds, volumes, objectName + ObjectId, this.transform);
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

		public void SetTeamColor ()
		{
				TeamColor[] teamColors = GetComponentsInChildren<TeamColor> ();
				foreach (TeamColor teamColor in teamColors)
						teamColor.renderer.material.color = player.teamColor;
		}

		protected virtual void BeginAttack (WorldObject target)
		{
				if (audioElement != null)
						audioElement.Play (attackSound);
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
				if (selected) {
						if (audioElement != null)
								audioElement.Play (selectSound);
						this.playingArea = playingArea;
				}
		}

		public virtual string[] GetActions ()
		{
				return actions;
		}

		public virtual void PerformAction (string action)
		{

		}

		public virtual void MouseClick (GameObject hitObject, Vector3 hitPoint, Player controller)
		{
				if (currentlySelected && hitObject && !WorkManager.ObjectIsGround (hitObject)) {
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

		public virtual void SaveDetails (JsonWriter writer)
		{
				SaveManager.WriteString (writer, "Type", name);
				SaveManager.WriteString (writer, "Name", objectName);
				SaveManager.WriteInt (writer, "Id", ObjectId);
				SaveManager.WriteVector (writer, "Position", transform.position);
				SaveManager.WriteQuaternion (writer, "Rotation", transform.rotation);
				SaveManager.WriteVector (writer, "Scale", transform.localScale);
				SaveManager.WriteInt (writer, "HitPoint", hitPoints);
				SaveManager.WriteBoolean (writer, "Attacking", attacking);
				SaveManager.WriteBoolean (writer, "MovingIntoPosition", movingIntoPosition);
				SaveManager.WriteBoolean (writer, "Aiming", aiming);
				if (attacking) {
						SaveManager.WriteFloat (writer, "CurrentWeaponChargeTime", currentWeaponChargeTime);
				}
				if (target != null)
						SaveManager.WriteInt (writer, "TargetId", target.ObjectId);
		}

		public void TakeDamage (int damage)
		{
				hitPoints -= damage;
				if (hitPoints <= 0)
						Destroy (gameObject);
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
				if (audioElement != null && Time.timeScale > 0)
						audioElement.Play (useWeaponSound);
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
						if (!WorkManager.ObjectIsGround (hoverObject)) {
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
		
		protected virtual void HandleLoadedProperty (JsonTextReader reader, string propertyName, object readValue)
		{
				switch (propertyName) {
				case "Name":
						objectName = (string)readValue;
						break;
				case "Id":
						ObjectId = (int)(System.Int64)readValue;
						break;
				case "Position":
						transform.localPosition = LoadManager.LoadVector (reader);
						break;
				case "Rotation":
						transform.localRotation = LoadManager.LoadQuaternion (reader);
						break;
				case "Scale":
						transform.localScale = LoadManager.LoadVector (reader);
						break;
				case "HitPoints":
						hitPoints = (int)(System.Int64)readValue;
						break;
				case "Attacking":
						attacking = (bool)readValue;
						break;
				case "MovingIntoPosition":
						movingIntoPosition = (bool)readValue;
						break;
				case "Aiming":
						aiming = (bool)readValue;
						break;
				case "CurrentWeaponChargeTime":
						currentWeaponChargeTime = (float)(double)readValue;
						break;
				case "TargetId":
						loadedTargetId = (int)(System.Int64)readValue;
						break;
				default :
						break;
				}
		}
		
		public void LoadDetails (JsonTextReader reader)
		{
				while (reader.Read()) {
						if (reader.Value != null) {
								if (reader.TokenType == JsonToken.PropertyName) {
										string propertyName = (string)reader.Value;
										reader.Read ();
										HandleLoadedProperty (reader, propertyName, reader.Value);
								}
						} else if (reader.TokenType == JsonToken.EndObject) {
								selectionBounds = ResourceManager.InvalidBounds;
								CalculateBounds ();
								loadedSavedValues = true;
								return;
						}
				}
				//loaded position invalidates selection bounds, must recalculate
				selectionBounds = ResourceManager.InvalidBounds;
				CalculateBounds ();
				loadedSavedValues = true;
		}
}
