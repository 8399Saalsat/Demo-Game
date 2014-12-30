using UnityEngine;
using System.Collections;
using RTS;

public class RTSCameraFollow : MonoBehaviour
{

		// Use this for initialization
		//private Player player;
		public bool mouseMoveCamera = true;
		public bool keyboardMoveCamera = true;
		public bool zoomEnabled = true;

		void Start ()
		{
			
		}
	
		// Update is called once per frame
		void FixedUpdate ()
		{	
				if (mouseMoveCamera) {
						MouseMoveCamera ();
				}
				if (keyboardMoveCamera) {
						KeyboardMoveCamera ();
				}
				if (zoomEnabled) {
						Zoom ();
				}
				Rotate ();
		}

		private void KeyboardMoveCamera ()
		{
				Vector3 movement = new Vector3 (0, 0, 0);
				Vector3 origin = transform.position;
				Vector3 destination = origin;
				float h = Input.GetAxis ("Horizontal");
				float v = Input.GetAxis ("Vertical");

				if (Input.GetAxis ("Horizontal") != 0) {
						movement.x += ResourceManager.ScrollSpeed * h;
				}
				if (Input.GetAxis ("Vertical") != 0) {
						movement.z += ResourceManager.ScrollSpeed * v;
				}
				movement = movement.normalized;
				movement = transform.TransformDirection (movement);
				movement.y = 0;

				
				destination += movement;
				if (destination != origin) {
						transform.position = Vector3.MoveTowards (origin, destination, Time.deltaTime * ResourceManager.KeyboadScrollSpeed);
				}
				
		}

		private void MouseMoveCamera ()
		{
				float xpos = Input.mousePosition.x;
				float ypos = Input.mousePosition.y;
				float speed = ResourceManager.ScrollSpeed;

				Vector3 movement = new Vector3 (0, 0, 0);
				Vector3 origin = transform.position;
				Vector3 destination = origin;

				if (xpos >= 0 && xpos < ResourceManager.ScrollWidth) {
						if (xpos == 0)
								xpos = 1;
						movement.x -= ResourceManager.ScrollSpeed;
						speed = ResourceManager.SpeedMultiplier * ResourceManager.ScrollWidth / xpos;
				} else if (xpos <= Screen.width && xpos > Screen.width - ResourceManager.ScrollWidth) {
						movement.x += ResourceManager.ScrollSpeed;
						speed = ResourceManager.SpeedMultiplier * ResourceManager.ScrollWidth / (Screen.width - xpos);
				}

				//vertical movement
				if (ypos > 0 && ypos < ResourceManager.ScrollWidth) {
						if (ypos == 0)
								ypos = 1;
						movement.z -= ResourceManager.ScrollSpeed;
						speed = ResourceManager.SpeedMultiplier * ResourceManager.ScrollWidth / ypos;
				} else if (ypos <= Screen.height && ypos > Screen.height - ResourceManager.ScrollWidth) {
						movement.z += ResourceManager.ScrollSpeed;
						speed += ResourceManager.SpeedMultiplier * ResourceManager.ScrollWidth / (Screen.height - ypos);
				}

				movement = transform.TransformDirection (movement);
				speed = Mathf.Clamp (speed, 0, ResourceManager.MaxSpeed);
				
				movement.y = 0;

				destination += movement;

				if (destination != origin) {
						transform.position = Vector3.MoveTowards (origin, destination, Time.deltaTime * speed);
				}
		}

		private void Zoom ()
		{
				Vector3 movement = new Vector3 (0, 0, 0);
				Vector3 origin = transform.position;
				Vector3 destination = origin;
				float diff = 0;

				movement.z += ResourceManager.ZoomSpeed * Input.GetAxis ("Mouse ScrollWheel");
				movement = transform.TransformDirection (movement);
				
				movement.x = 0;
				
				destination += movement;

				if (destination.y > ResourceManager.CameraMaxHeight || destination.y < ResourceManager.CameraMinHeight) {
						destination = origin;
				}

				if (destination != origin) {
						transform.position = Vector3.MoveTowards (origin, destination, Time.deltaTime * ResourceManager.ZoomSpeed);
				}
		}

		private void Rotate ()
		{
				Vector3 origin = transform.eulerAngles;
				Vector3 destination = origin;

				//if ((Input.GetKey (KeyCode.LeftAlt) || Input.GetKey (KeyCode.RightAlt)) && Input.GetMouseButton (1)) {
				if (Input.GetMouseButton (0) && Input.GetMouseButton (1)) {
						destination.x -= Input.GetAxis ("Mouse Y") * ResourceManager.RotateAmount;
						destination.y += Input.GetAxis ("Mouse X") * ResourceManager.RotateAmount;
						Screen.showCursor = false;
				} else {
						Screen.showCursor = true;
				}
				if (destination != origin) {
						transform.eulerAngles = Vector3.MoveTowards (origin, destination, Time.deltaTime * ResourceManager.RotateSpeed);
				}
		}
}
