/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2015
 *	
 *	"MenuProfilesList.cs"
 * 
 *	This MenuElement handles the display of any save profiles recorded.
 * 
 */

using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;	
#endif

namespace AC
{
	
	public class MenuProfilesList : MenuElement
	{
		
		public UISlot[] uiSlots;
		
		public TextEffects textEffects;
		public TextAnchor anchor;
		public int maxSlots = 5;
		public ActionListAsset actionListOnClick;
		public bool showActive = true;

		private string[] labels = null;

		
		public override void Declare ()
		{
			uiSlots = null;
			
			isVisible = true;
			isClickable = true;
			numSlots = 1;
			maxSlots = 5;
			showActive = true;

			SetSize (new Vector2 (20f, 5f));
			anchor = TextAnchor.MiddleCenter;

			actionListOnClick = null;
			textEffects = TextEffects.None;

			base.Declare ();
		}
		
		
		public override MenuElement DuplicateSelf ()
		{
			MenuProfilesList newElement = CreateInstance <MenuProfilesList>();
			newElement.Declare ();
			newElement.CopyProfilesList (this);
			return newElement;
		}
		
		
		public void CopyProfilesList (MenuProfilesList _element)
		{
			uiSlots = _element.uiSlots;
			
			textEffects = _element.textEffects;
			anchor = _element.anchor;
			maxSlots = _element.maxSlots;
			actionListOnClick = _element.actionListOnClick;
			showActive = _element.showActive;

			base.Copy (_element);
		}
		
		
		public override void LoadUnityUI (AC.Menu _menu)
		{
			int i=0;
			foreach (UISlot uiSlot in uiSlots)
			{
				uiSlot.LinkUIElements ();
				if (uiSlot != null && uiSlot.uiButton != null)
				{
					int j=i;
					uiSlot.uiButton.onClick.AddListener (() => {
						ProcessClick (_menu, j, KickStarter.playerInput.mouseState);
					});
				}
				i++;
			}
		}
		
		
		public override GameObject GetObjectToSelect ()
		{
			if (uiSlots != null && uiSlots.Length > 0 && uiSlots[0].uiButton != null)
			{
				return uiSlots[0].uiButton.gameObject;
			}
			return null;
		}
		
		
		public override RectTransform GetRectTransform (int _slot)
		{
			if (uiSlots != null && uiSlots.Length > _slot)
			{
				return uiSlots[_slot].GetRectTransform ();
			}
			return null;
		}
		
		
		#if UNITY_EDITOR
		
		public override void ShowGUI (MenuSource source)
		{
			EditorGUILayout.BeginVertical ("Button");

			showActive = EditorGUILayout.Toggle ("Include active?", showActive);
			maxSlots = EditorGUILayout.IntField ("Max no. of slots:", maxSlots);
			if (source == MenuSource.AdventureCreator)
			{
				numSlots = EditorGUILayout.IntSlider ("Test slots:", numSlots, 1, maxSlots);
				slotSpacing = EditorGUILayout.Slider ("Slot spacing:", slotSpacing, 0f, 20f);
				orientation = (ElementOrientation) EditorGUILayout.EnumPopup ("Slot orientation:", orientation);
				if (orientation == ElementOrientation.Grid)
				{
					gridWidth = EditorGUILayout.IntSlider ("Grid size:", gridWidth, 1, 10);
				}
			}
			
			if (source == MenuSource.AdventureCreator)
			{
				anchor = (TextAnchor) EditorGUILayout.EnumPopup ("Text alignment:", anchor);
				textEffects = (TextEffects) EditorGUILayout.EnumPopup ("Text effect:", textEffects);
			}
			
			actionListOnClick = ActionListAssetMenu.AssetGUI ("ActionList after selecting:", actionListOnClick);

			if (source != MenuSource.AdventureCreator)
			{
				EditorGUILayout.EndVertical ();
				EditorGUILayout.BeginVertical ("Button");
				
				uiSlots = ResizeUISlots (uiSlots, maxSlots);
				for (int i=0; i<uiSlots.Length; i++)
				{
					uiSlots[i].LinkedUiGUI (i, source);
				}
			}
			
			EditorGUILayout.EndVertical ();
			
			base.ShowGUI (source);
		}
		
		#endif


		public override void Shift (AC_ShiftInventory shiftType, int amount)
		{
			if (isVisible && numSlots >= maxSlots)
			{
				Shift (shiftType, maxSlots, KickStarter.options.GetNumProfiles (), amount);
			}
		}


		public override bool CanBeShifted (AC_ShiftInventory shiftType)
		{
			if (numSlots == 0)
			{
				return false;
			}
			if (shiftType == AC_ShiftInventory.ShiftLeft)
			{
				if (offset == 0)
				{
					return false;
				}
			}
			else
			{
				if (offset >= GetMaxOffset ())
				{
					return false;
				}
			}
			return true;
		}


		private int GetMaxOffset ()
		{
			if (!showActive)
			{
				return Mathf.Max (0, KickStarter.options.GetNumProfiles () - 1 - maxSlots);
			}
			return Mathf.Max (0, KickStarter.options.GetNumProfiles () - maxSlots);
		}
		
		
		public override string GetLabel (int slot, int languageNumber)
		{
			if (Application.isPlaying)
			{
				return KickStarter.options.GetProfileName (slot + offset, showActive);
			}

			return ("Profile " + slot.ToString ());
		}
		
		
		public override void HideAllUISlots ()
		{
			LimitUISlotVisibility (uiSlots, 0);
		}
		
		
		public override void PreDisplay (int _slot, int languageNumber, bool isActive)
		{
			string fullText = GetLabel (_slot, languageNumber);

			if (!Application.isPlaying)
			{
				if (labels == null || labels.Length != numSlots)
				{
					labels = new string [numSlots];
				}
			}

			labels [_slot] = fullText;

			if (Application.isPlaying)
			{
				if (uiSlots != null && uiSlots.Length > _slot)
				{
					LimitUISlotVisibility (uiSlots, numSlots);
					uiSlots[_slot].SetText (labels [_slot]);
				}
			}
		}
		
		
		public override void Display (GUIStyle _style, int _slot, float zoom, bool isActive)
		{
			base.Display (_style, _slot, zoom, isActive);
			
			_style.alignment = anchor;
			if (zoom < 1f)
			{
				_style.fontSize = (int) ((float) _style.fontSize * zoom);
			}
			
			if (textEffects != TextEffects.None)
			{
				AdvGame.DrawTextEffect (ZoomRect (GetSlotRectRelative (_slot), zoom), labels[_slot], _style, Color.black, _style.normal.textColor, 2, textEffects);
			}
			else
			{
				GUI.Label (ZoomRect (GetSlotRectRelative (_slot), zoom), labels[_slot], _style);
			}
		}
		
		
		public override void ProcessClick (AC.Menu _menu, int _slot, MouseState _mouseState)
		{
			if (KickStarter.stateHandler.gameState == GameState.Cutscene)
			{
				return;
			}
			
			base.ProcessClick (_menu, _slot, _mouseState);

			bool isSuccess = KickStarter.options.SwitchProfileIfExists (_slot + offset, showActive);
			if (isSuccess)
			{
				AdvGame.RunActionListAsset (actionListOnClick);
			}
		}
		
		
		public override void RecalculateSize (MenuSource source)
		{
			if (Application.isPlaying)
			{
				numSlots = KickStarter.options.GetNumProfiles ();

				if (!showActive)
				{
					numSlots --;
				}

				if (numSlots > maxSlots)
				{
					numSlots = maxSlots;
				}

				offset = Mathf.Min (offset, GetMaxOffset ());
			}
			
			labels = new string [numSlots];
			
			if (!isVisible)
			{
				LimitUISlotVisibility (uiSlots, 0);
			}
			
			base.RecalculateSize (source);
		}
		
		
		protected override void AutoSize ()
		{
			AutoSize (new GUIContent (GetLabel (0, 0)));
		}
		
	}
	
}