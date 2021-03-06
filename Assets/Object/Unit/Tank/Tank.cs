﻿using UnityEngine;
using System.Collections;
using RTS;
using Newtonsoft.Json;

public class Tank : Unit
{

		private Quaternion aimRotation;
		public GameObject tankShell;

		// Use this for initialization
		protected override void Start ()
		{
				base.Start ();
		}
	
		// Update is called once per frame
		protected override void Update ()
		{
				base.Update ();
				if (aiming) {
						transform.rotation = Quaternion.RotateTowards (transform.rotation, aimRotation, weaponAimSpeed);
						CalculateBounds ();
						//prevent getting stuck at exactly 180 degrees
						Quaternion inverseAimRotation = new Quaternion (-aimRotation.x, -aimRotation.y, -aimRotation.z, -aimRotation.w);
						if (transform.rotation == aimRotation || transform.rotation == inverseAimRotation) {
								aiming = false;
						}
				}
				
		}
		protected override void HandleLoadedProperty (JsonTextReader reader, string propertyName, object readValue)
		{
				base.HandleLoadedProperty (reader, propertyName, readValue);
				switch (propertyName) {
				case "AimRotation":
						aimRotation = LoadManager.LoadQuaternion (reader);
						break;
				default:
						break;
				}
		}

		public override void SaveDetails (JsonWriter writer)
		{
				base.SaveDetails (writer);
				SaveManager.WriteQuaternion (writer, "AimRotation", aimRotation);
		}

		public override bool CanAttack ()
		{
				return true;
		}

		protected override void AimAtTarget ()
		{
				base.AimAtTarget ();
				aimRotation = Quaternion.LookRotation (target.transform.position - transform.position);
		}

		protected override void UseWeapon ()
		{
				base.UseWeapon ();
				Vector3 spawnPoint = transform.position;
				spawnPoint.x += (2.1f * transform.forward.x);
				spawnPoint.y += 1.4f;
				spawnPoint.z += (2.1f * transform.forward.z);
				GameObject gameObject = (GameObject)Instantiate (ResourceManager.GetWorldObject ("TankProjectile"), spawnPoint, transform.rotation);
				Projectile projectile = gameObject.GetComponentInChildren<Projectile> ();
				projectile.SetRange (0.9f * weaponRange);
				projectile.SetTarget (target);
		}
}
