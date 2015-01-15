﻿using UnityEngine;
using System.Collections;

public static class SelectionList
{
		private static string[] myEntries = {};
		private static int gridIndex = 0;
		private static float scrollValue = 0.0f;
		private static float leftPos;
		private static float topPos;
		private static float areaWidth;
		private static float areaHeight;
		private static float rowHeight = 25;
		private static float sliderWidth = 10;
		private static float sliderPadding = 5;
		
		public static void LoadEntries (string[] entries)
		{
				myEntries = entries;
		}
		
		public static string GetCurrentEntry ()
		{
				if (gridIndex >= 0 && gridIndex < myEntries.Length)
						return myEntries [gridIndex];
				else
						return "";
		}
		
		public static bool Contains (string entry)
		{
				bool contains = false;
				for (int i=0; i < myEntries.Length; i++) {
						if (myEntries [i] == entry)
								contains = true;
				}
				return contains;
		}
		
		public static bool MouseDoubleClick ()
		{
				Event e = Event.current;
				Vector3 mousePos = Input.mousePosition;
				mousePos.y = Screen.height - mousePos.y;
				float selHeight = myEntries.Length * rowHeight;
				float selWidth = areaWidth;
				if (selHeight > areaHeight)
						selWidth -= (sliderWidth + 2 * sliderPadding);
				bool mouseInSelection = new Rect (leftPos, topPos, selWidth, areaHeight).Contains (mousePos);
				if (e != null && e.isMouse && e.type == EventType.MouseDown && e.clickCount == 2 && mouseInSelection)
						return true;
				else
						return false;
		}
		
		public static void Draw (float left, float top, float width, float height)
		{
				leftPos = left;
				topPos = top;
				areaWidth = width;
				areaHeight = height;
				DrawBox ();
		}
		
		public static void Draw (float left, float top, float width, float height, GUISkin skin)
		{
				leftPos = left;
				topPos = top;
				areaWidth = width;
				areaHeight = height;
				GUI.skin = skin;
				DrawBox ();
		}
		
		public static void Draw (Rect drawArea)
		{
				leftPos = drawArea.x;
				topPos = drawArea.y;
				areaWidth = drawArea.width;
				areaHeight = drawArea.height;
				DrawBox ();
		}
		
		public static void Draw (Rect drawArea, GUISkin skin)
		{
				leftPos = drawArea.x;
				topPos = drawArea.y;
				areaWidth = drawArea.width;
				areaHeight = drawArea.height;
				GUI.skin = skin;
				DrawBox ();
		}
		
		
		private static void DrawBox ()
		{
				float selWidth = areaWidth;
				float selHeight = myEntries.Length * rowHeight;
		
				GUI.BeginGroup (new Rect (leftPos, topPos, areaWidth, areaHeight));
				if (selHeight > areaHeight) {
						float sliderLeft = selWidth + sliderPadding;
						float sliderMax = selHeight - areaHeight;
						scrollValue = GUI.VerticalSlider (new Rect (sliderLeft, 0, sliderWidth, areaHeight), scrollValue, 0.0f, sliderMax);
						scrollValue -= Input.GetAxis ("Mouse ScrollWheel") * rowHeight;
						if (scrollValue < 0.0f)
								scrollValue = 0.0f;
						if (scrollValue > sliderMax)
								scrollValue = sliderMax;
				}
				GUI.BeginGroup (new Rect (0, 1, areaWidth, areaHeight - 2));
				float selGridTop = 0.0f - scrollValue;
				gridIndex = GUI.SelectionGrid (new Rect (0, selGridTop, selWidth, selHeight), gridIndex, myEntries, 1);
				GUI.EndGroup ();
				GUI.EndGroup ();
		}
		
		
}
