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
		public bool rotateEnabled = true;
		public Player player;

		

		void Start ()
		{
					
		}
	
		// Update is called once per frame
		void FixedUpdate ()
		{	
				if (mouseMoveCamera /*&&/* Screen.showCursor /*&& player.hud.MouseInBounds ()*/) {
						MouseMoveCamera ();
				}
				if (keyboardMoveCamera) {
						KeyboardMoveCamera ();
				}
				if (zoomEnabled) {
						Zoom ();
				}
				if (rotateEnabled) {
						Rotate ();
				}
				
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
				bool mouseScroll = false;

				float xpos = Input.mousePosition.x;
				float ypos = Input.mousePosition.y;
				float speed = ResourceManager.ScrollSpeed;

				Vector3 movement = new Vector3 (0, 0, 0);
				Vector3 origin = transform.position;
				Vector3 destination = origin;

				if (xpos >= 0 && xpos < ResourceManager.ScrollWidth + player.hud.LeftOffset) {
						if (xpos == 0)
								xpos = 1;
						movement.x -= ResourceManager.ScrollSpeed;
						speed = ResourceManager.SpeedMultiplier * ResourceManager.ScrollWidth / (xpos + player.hud.LeftOffset);
						player.hud.SetCursorState (CursorState.PanLeft);
						mouseScroll = true;
				} else if (xpos <= Screen.width && xpos > Screen.width - ResourceManager.ScrollWidth - player.hud.RightOffset) {
						movement.x += ResourceManager.ScrollSpeed;
						speed = ResourceManager.SpeedMultiplier * ResourceManager.ScrollWidth / (Screen.width - xpos - player.hud.RightOffset);
						player.hud.SetCursorState (CursorState.PanRight);
						mouseScroll = true;
				}

				//vertical movement
				if (ypos > 0 && ypos < ResourceManager.ScrollWidth + player.hud.BottomOffset) {
						if (ypos == 0)
								ypos = 1;
						movement.z -= ResourceManager.ScrollSpeed;
						speed = ResourceManager.SpeedMultiplier * ResourceManager.ScrollWidth / (ypos + player.hud.BottomOffset);
						player.hud.SetCursorState (CursorState.PanDown);
						mouseScroll = true;
				} else if (ypos <= Screen.height && ypos > Screen.height - ResourceManager.ScrollWidth - player.hud.TopOffset) {
						movement.z += ResourceManager.ScrollSpeed;
						speed += ResourceManager.SpeedMultiplier * ResourceManager.ScrollWidth / (Screen.height - ypos - player.hud.TopOffset);
						player.hud.SetCursorState (CursorState.PanUp);
						mouseScroll = true;
				}

				movement = transform.TransformDirection (movement);
				speed = Mathf.Clamp (speed, 0, ResourceManager.MaxSpeed);
				
				movement.y = 0;

				destination += movement;

				if (destination != origin) {
						transform.position = Vector3.MoveTowards (origin, destination, Time.deltaTime * speed);
				}
				if (!mouseScroll) {
						player.hud.SetCursorState (CursorState.Select);
				}
		}

		private void Zoom ()
		{
				Vector3 movement = new Vector3 (0, 0, 0);
				Vector3 origin = transform.position;
				Vector3 destination = origin;

				movement.y += ResourceManager.ZoomSpeed * Input.GetAxis ("Mouse ScrollWheel");
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
				//	if (Input.GetMouseButton (0) && Input.GetMouseButton (1)) {
				if (Input.GetMouseButton (2)) {
						destination.x -= Input.GetAxis ("Mouse Y") * ResourceManager.RotateAmount;
						destination.y += Input.GetAxis ("Mouse X") * ResourceManager.RotateAmount;
						//Screen.showCursor = false;
				} else {
						//Screen.showCursor = true;
				}
				if (destination != origin) {
						transform.eulerAngles = Vector3.MoveTowards (origin, destination, Time.deltaTime * ResourceManager.RotateSpeed);
				}
		}
}
