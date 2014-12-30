using UnityEngine;
using System.Collections;

public class RTSCamera : MonoBehaviour
{

/*		public float scrollSpeed = 15f;
		public float scrollEdge = 0.1f;
		public float panSpeed = 10f;
		private float currentZoom = 0;
		private float zoomSpeed = 1;
		private float zoomRotation = 1;
		private int horizontalScroll = 1;
		private int verticalScroll = 1;
		private int diagonalScroll = 1;
		private Vector3 initPosition;
		private Vector3 initRotation;
		private Vector2 zoomRange = new Vector2 (-5, 5);
		private Vector2 zoomAngleRange = new Vector2 (20, 70);*/

		
		public Transform target;
		public float scrollSpeed = 1f;
		public float fastScrollSpeed = 2f;
		public float smoothing = 5f;

		private Vector3 offset;

		// Use this for initialization
		void Start ()
		{
				offset = transform.position - target.position;
		}
	
		// Update is called once per frame
		void FixedUpdate ()
		{
			
				Vector3 targetCamPos = target.position + offset;
				transform.position = Vector3.Lerp (transform.position, targetCamPos, smoothing * Time.deltaTime);
	
		}
}
