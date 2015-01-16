using UnityEngine;
using System.Collections;
using RTS;

public class LoadMenu : MonoBehaviour
{
		public GUISkin mainSkin;
		public GUISkin selectionSkin;
		
		void Start ()
		{
				Activate ();
		}
		void Update ()
		{
				if (Input.GetKeyDown (KeyCode.Escape))
						CancelLoad ();
		}

		public void Activate ()
		{

		}

		private void StartLoad ()
		{

		}

		private void CancelLoad ()
		{

		}
}
