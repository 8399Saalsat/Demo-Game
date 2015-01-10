﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
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
		public Texture2D buttonHover;
		public Texture2D buttonClick;
		public Texture2D buildFrame;
		public Texture2D buildMask;
		public Texture2D[] moveCursors;
		public Texture2D[] attackCursors;
		public Texture2D[] harvestCursors;
		public Texture2D[] resources;


		private const int ORDERS_BAR_WIDTH = 150;
		private const int RESOURCE_BAR_HEIGHT = 40;
		private const int SELECTION_NAME_HEIGHT = 15;
		private const int ICON_WIDTH = 32;
		private const int ICON_HEIGHT = 32;
		private const int TEXT_WIDTH = 128;
		private const int TEXT_HEIGHT = 32;
		private const int BUILD_IMAGE_WIDTH = 64;
		private const int BUILD_IMAGE_HEIGHT = 64;
		private const int BUTTON_SPACING = 7;
		private const int SCROLL_BAR_WIDTH = 22;
		private const int BUILD_IMAGE_PADDING = 8;

		private CursorState activeCursorState;
		private int currentFrame = 0;
		private Texture2D activeCursor;
		private Player player;
		private Dictionary<ResourceType, int> resourceValues;
		private Dictionary<ResourceType, int> resourceLimits;
		private Dictionary<ResourceType, Texture2D> resourceImages;
		private WorldObject lastSelection;
		private float sliderValue;
		private int buildAreaHeight = 0;

		// Use this for initialization
		void Start ()
		{
				player = transform.root.GetComponent < Player > (); 
				ResourceManager.StoreSelectBoxItems (selectBoxSkin);
				SetCursorState (CursorState.Select);

				resourceValues = new Dictionary<ResourceType, int > ();
				resourceLimits = new Dictionary<ResourceType, int > ();

				resourceImages = new Dictionary<ResourceType,Texture2D> ();

				buildAreaHeight = Screen.height - RESOURCE_BAR_HEIGHT - SELECTION_NAME_HEIGHT - 2 * BUTTON_SPACING;
				
				for (int i =0; i < resources.Length; i++) {
						switch (resources [i].name) {
						case "Money":
								resourceImages.Add (ResourceType.Money, resources [i]);
								resourceValues.Add (ResourceType.Money, 0);
								resourceLimits.Add (ResourceType.Money, 0);
								break;
						case "Power":
								resourceImages.Add (ResourceType.Power, resources [i]);
								resourceValues.Add (ResourceType.Power, 0);
								resourceLimits.Add (ResourceType.Power, 0);
								break;
						default:
								break;
						}
				}
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
				GUI.BeginGroup (new Rect (Screen.width - ORDERS_BAR_WIDTH - BUILD_IMAGE_WIDTH, RESOURCE_BAR_HEIGHT, ORDERS_BAR_WIDTH + BUILD_IMAGE_WIDTH, Screen.height - RESOURCE_BAR_HEIGHT));
				GUI.Box (new Rect (BUILD_IMAGE_WIDTH + SCROLL_BAR_WIDTH, 0, ORDERS_BAR_WIDTH, Screen.height - RESOURCE_BAR_HEIGHT), "");
				if (player.SelectedObject) {
						selectionName = player.SelectedObject.objectName;
						if (player.SelectedObject.IsOwnedBy (player)) {
								//reset slider value if the selected object has changed
								if (lastSelection && lastSelection != player.SelectedObject)
										sliderValue = 0.0f;
								DrawActions (player.SelectedObject.GetActions ());
								lastSelection = player.SelectedObject;
								Building selectedBuilding = lastSelection.GetComponent<Building> ();
								if (selectedBuilding) {
										DrawBuildQueue (selectedBuilding.getBuildQueueValues (), selectedBuilding.getBuildPercentage ());
								}
						}
				}
				if (!selectionName.Equals ("")) {
						int leftPos = BUILD_IMAGE_WIDTH + SCROLL_BAR_WIDTH / 2;
						int topPos = buildAreaHeight + BUTTON_SPACING;
						GUI.Label (new Rect (leftPos, topPos, ORDERS_BAR_WIDTH, SELECTION_NAME_HEIGHT), selectionName);
				}
			

				GUI.EndGroup ();
		}
		
		private void DrawResourceBar ()
		{
				GUI.skin = resourceSkin;
				GUI.BeginGroup (new Rect (0, 0, Screen.width, RESOURCE_BAR_HEIGHT));
				GUI.Box (new Rect (0, 0, Screen.width, RESOURCE_BAR_HEIGHT), "");
				int topPos = 4;
				int iconLeft = 4;
				int textLeft = 20;
				DrawResourceIcon (ResourceType.Money, iconLeft, textLeft, topPos);
				iconLeft += TEXT_WIDTH;
				textLeft += TEXT_WIDTH;
				DrawResourceIcon (ResourceType.Power, iconLeft, textLeft, topPos);
				GUI.EndGroup ();
		}
		private void DrawResourceIcon (ResourceType type, int iconLeft, int textLeft, int topPos)
		{
				Texture2D icon = resourceImages [type];
				string text = resourceValues [type].ToString () + "/" + resourceLimits [type].ToString ();
				GUI.DrawTexture (new Rect (iconLeft, topPos, ICON_WIDTH, ICON_HEIGHT), icon);
				GUI.Label (new Rect (textLeft, topPos, TEXT_WIDTH, TEXT_HEIGHT), text);
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

	

		private void DrawMouseCursor ()
		{
				bool mouseOverHud = !MouseInBounds () && activeCursorState != CursorState.PanRight && activeCursorState != CursorState.PanUp;

				
				if (mouseOverHud) {
						Screen.showCursor = true;
						//	Debug.Log ("show cursor = true");
				} else {
						Screen.showCursor = false;
						//	Debug.Log ("show cursor = false");
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
						leftPos -= activeCursor.width / 2;
			//	else if (activeCursorState == CursorState.PanDown)
						//topPos = Screen.height - activeCursor.height;
					//	topPos -= activeCursor.height / 2;
				else if (activeCursorState == CursorState.Move || activeCursorState == CursorState.Select || activeCursorState == CursorState.Harvest) {
						topPos -= activeCursor.height / 2;
						leftPos -= activeCursor.width / 2;
				}
				return new Rect (leftPos, topPos, activeCursor.width, activeCursor.height);
		}

		private void DrawActions (string[] actions)
		{
				GUIStyle buttons = new GUIStyle ();
				buttons.hover.background = buttonHover;
				buttons.active.background = buttonClick;
				GUI.skin.button = buttons;
				int numActions = actions.Length;
				//define the area to draw the ations inside
				GUI.BeginGroup (new Rect (BUILD_IMAGE_WIDTH, 0, ORDERS_BAR_WIDTH, buildAreaHeight));
				//draw scroll bar for the list of actions if need be
				if (numActions >= MaxNumRows (buildAreaHeight))
						DrawSlider (buildAreaHeight, numActions / 2.0f);
				//display possible actions as buttons and handle the button click for each
				for (int i = 0; i < numActions; i++) {
						int column = i % 2;
						int row = i / 2;
						Rect pos = GetButtonPos (row, column);
						Texture2D action = ResourceManager.GetBuildImage (actions [i]);
						if (action) {
								//create the button and handle the click of that button
								if (GUI.Button (pos, action)) {
										if (player.SelectedObject)
												player.SelectedObject.PerformAction (actions [i]);
								}
						}
				}
				GUI.EndGroup ();
		}

		private Rect GetScrollPos (int groupHeight)
		{
				return new Rect (BUTTON_SPACING, BUTTON_SPACING, SCROLL_BAR_WIDTH, groupHeight - 2 * BUTTON_SPACING);
		}

		private int MaxNumRows (int areaHeight)
		{
				return areaHeight / BUILD_IMAGE_HEIGHT;
		}

		private Rect GetButtonPos (int row, int column)
		{
				int left = SCROLL_BAR_WIDTH + column * BUILD_IMAGE_WIDTH;
				int top = row * BUILD_IMAGE_HEIGHT - (int)sliderValue * BUILD_IMAGE_HEIGHT;
				return new Rect (left, top, BUILD_IMAGE_WIDTH, BUILD_IMAGE_HEIGHT);
		}

		private void DrawSlider (int groupHeight, float numRows)
		{
				//slider goes from 0 to the number of rows that do not fit on screen
				sliderValue = GUI.VerticalSlider (GetScrollPos (groupHeight), sliderValue, 0.0f, numRows - MaxNumRows (groupHeight));
		}
		private void DrawBuildQueue (string[] buildQueue, float buildPercentage)
		{
				for (int i = 0; i < buildQueue.Length; i++) {
						float topPos = i * BUILD_IMAGE_HEIGHT - (i + 1) * BUILD_IMAGE_PADDING;
						Rect buildPos = new Rect (BUILD_IMAGE_PADDING, topPos, BUILD_IMAGE_WIDTH, BUILD_IMAGE_HEIGHT);
						GUI.DrawTexture (buildPos, ResourceManager.GetBuildImage (buildQueue [i]));
						GUI.DrawTexture (buildPos, buildFrame);
						topPos += BUILD_IMAGE_PADDING;
						float width = BUILD_IMAGE_WIDTH - 2 * BUILD_IMAGE_PADDING;
						float height = BUILD_IMAGE_HEIGHT - 2 * BUILD_IMAGE_PADDING;
						if (i == 0) {
								//shrink the build mask on the item currently being built to give an idea of progress
								topPos += height * buildPercentage;
								height *= (1 - buildPercentage);
						}
						GUI.DrawTexture (new Rect (2 * BUILD_IMAGE_PADDING, topPos, width, height), buildMask);
				}
		}
		
		public Rect GetPlayingArea ()
		{
				return new Rect (0, RESOURCE_BAR_HEIGHT, Screen.width - ORDERS_BAR_WIDTH, Screen.height - RESOURCE_BAR_HEIGHT);
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

		public void SetResourceValues (Dictionary<ResourceType, int> resourceValues, Dictionary<ResourceType, int> resourceLimits)
		{
				this.resourceValues = resourceValues;
				this.resourceLimits = resourceLimits;
		}

}
