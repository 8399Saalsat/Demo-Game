using UnityEngine;
using System.Collections;
using RTS;

public class HUD : MonoBehaviour
{
		public int RightOffset { get { return RightSideOffset (); } }
		public int LeftOffset { get { return LeftSideOffset (); } }
		public int BottomOffset { get { return BottomSideOffset (); } }
		public int TopOffset { get { return TopSideOffset (); } }
		
		
		public GUISkin resourceSkin ;
		public GUISkin ordersSkin;
		public GUISkin selectBoxSkin;
		public GUISkin mouseCursorSkin;

		//public Texture2D activeCursor;
		public Texture2D selectCursor;
		public Texture2D leftCursor;
		public Texture2D rightCursor;
		public Texture2D upCursor;
		public Texture2D downCursor;
		public Texture2D[] moveCursors;
		public Texture2D[] attackCursors;
		public Texture2D[] harvestCursors;

		private const int ORDERS_BAR_WIDTH = 150;
		private const int RESOURCE_BAR_HEIGHT = 40;
		private const int SELECTION_NAME_HEIGHT = 15;

		private CursorState activeCursorState;
		private int currentFrame = 0;

		private Texture2D activeCursor;
		private Player player;
		// Use this for initialization
		void Start ()
		{
				player = transform.root.GetComponent < Player > (); 
				ResourceManager.StoreSelectBoxItems (selectBoxSkin);
				SetCursorState (CursorState.Select);
		}
	
		void OnGUI ()
		{
				if (player && player.human) {
						DrawOrdersBar ();
						DrawResourceBar ();
				}
				DrawMouseCursor ();
			
		}

		private void DrawOrdersBar ()
		{
				string selectionName = "";
				GUI.skin = ordersSkin;
				GUI.BeginGroup (new Rect (Screen.width - ORDERS_BAR_WIDTH, RESOURCE_BAR_HEIGHT, ORDERS_BAR_WIDTH, Screen.height - RESOURCE_BAR_HEIGHT));
				GUI.Box (new Rect (0, 0, ORDERS_BAR_WIDTH, Screen.height - RESOURCE_BAR_HEIGHT), "");
				if (player.SelectedObject)
						selectionName = player.SelectedObject.objectName;
				if (!selectionName.Equals (""))
						GUI.Label (new Rect (0, 10, ORDERS_BAR_WIDTH, SELECTION_NAME_HEIGHT), selectionName);

				GUI.EndGroup ();
		}
		
		private void DrawResourceBar ()
		{
				GUI.skin = resourceSkin;
				GUI.BeginGroup (new Rect (0, 0, Screen.width, RESOURCE_BAR_HEIGHT));
				GUI.Box (new Rect (0, 0, Screen.width, RESOURCE_BAR_HEIGHT), "");
				GUI.EndGroup ();
		}

		public bool MouseInBounds ()
		{
				Vector3 mousePos = Input.mousePosition;
				bool insideWidth = mousePos.x >= 0 && mousePos.x <= Screen.width - ORDERS_BAR_WIDTH;
				bool insideHeight = mousePos.y >= 0 && mousePos.y <= Screen.height - RESOURCE_BAR_HEIGHT;
				return insideWidth && insideHeight;
		}

		private int LeftSideOffset ()
		{
				return 0;
		}
		private int RightSideOffset ()
		{
				return ORDERS_BAR_WIDTH;
		}
		private int TopSideOffset ()
		{
				return RESOURCE_BAR_HEIGHT;
		}
		private int BottomSideOffset ()
		{
				return 0;
		}

		public Rect GetPlayingArea ()
		{
				return new Rect (0, RESOURCE_BAR_HEIGHT, Screen.width - ORDERS_BAR_WIDTH, Screen.height - RESOURCE_BAR_HEIGHT);
		}

		private void DrawMouseCursor ()
		{
				bool mouseOverHud = !MouseInBounds () && activeCursorState != CursorState.PanRight && activeCursorState != CursorState.PanUp;
				if (mouseOverHud) {
						Screen.showCursor = true;
						Debug.Log ("show cursor = true");
				} else {
						Screen.showCursor = false;
						Debug.Log ("show cursor = false");
						GUI.skin = mouseCursorSkin;
						GUI.BeginGroup (new Rect (0, 0, Screen.width, Screen.height));
						UpdateCursorAnimation ();
						Rect cursorPosition = GetCursorDrawPosition ();
						GUI.Label (cursorPosition, activeCursor);
						GUI.EndGroup ();
				}
		}

		private void UpdateCursorAnimation ()
		{
				if (activeCursorState == CursorState.Move) {
						currentFrame = (int)Time.time % moveCursors.Length;
						activeCursor = moveCursors [currentFrame];
				} else if (activeCursorState == CursorState.Attack) {
						currentFrame = (int)Time.time % attackCursors.Length;
						activeCursor = attackCursors [currentFrame];
				} else if (activeCursorState == CursorState.Harvest) {
						currentFrame = (int)Time.time % harvestCursors.Length;
						activeCursor = harvestCursors [currentFrame];
				}
		}

		private Rect GetCursorDrawPosition ()
		{
				float leftPos = Input.mousePosition.x;
				float topPos = Screen.height - Input.mousePosition.y;
				if (activeCursorState == CursorState.PanRight)
						leftPos = Screen.width - activeCursor.width - ORDERS_BAR_WIDTH;
				else if (activeCursorState == CursorState.PanDown)
						topPos = Screen.height - activeCursor.height;
				else if (activeCursorState == CursorState.Move || activeCursorState == CursorState.Select || activeCursorState == CursorState.Harvest) {
						topPos -= activeCursor.height / 2;
						leftPos -= activeCursor.width / 2;
				}
				return new Rect (leftPos, topPos, activeCursor.width, activeCursor.height);
		}

		public void SetCursorState (CursorState newState)
		{
				activeCursorState = newState;
				switch (newState) {
				case CursorState.Select:
						activeCursor = selectCursor;
						break;
				case CursorState.Attack:
						currentFrame = (int)Time.time % attackCursors.Length;
						activeCursor = attackCursors [currentFrame];
						break;
				case CursorState.Harvest:
						currentFrame = (int)Time.time % harvestCursors.Length;
						activeCursor = harvestCursors [currentFrame];
						break;
				case CursorState.Move:
						currentFrame = (int)Time.time % moveCursors.Length;
						activeCursor = moveCursors [currentFrame];
						break;
				case CursorState.PanLeft:
						activeCursor = leftCursor;
						break;
				case CursorState.PanRight:
						activeCursor = rightCursor;
						break;
				case CursorState.PanUp:
						activeCursor = upCursor;
						break;
				case CursorState.PanDown:
						activeCursor = downCursor;
						break;
				default :
						break;
				}
		}

}
