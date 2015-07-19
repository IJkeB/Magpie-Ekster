/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"MenuSavesList.cs"
 * 
 *	This MenuElement handles the display of any saved games recorded.
 * 
 */

using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;	
#endif

namespace AC
{

	public class MenuSavesList : MenuElement
	{

		public UISlot[] uiSlots;

		public enum SaveDisplayType { LabelOnly, ScreenshotOnly, LabelAndScreenshot };

		public string newSaveText = "New save";
		public TextEffects textEffects;
		public TextAnchor anchor;
		public AC_SaveListType saveListType;
		public int maxSlots = 5;
		public ActionListAsset actionListOnSave;
		public SaveDisplayType displayType = SaveDisplayType.LabelOnly;
		public Texture2D blankSlotTexture;

		// Import
		public string importProductName;
		public string importSaveFilename;
		public bool checkImportBool;
		public int checkImportVar;

		public bool fixedOption;
		public int optionToShow;

		public int parameterID = -1;

		public bool showNewSaveOption = true;
		public bool autoHandle = true;

		private string[] labels = null;
		private bool newSaveSlot = false;

		
		public override void Declare ()
		{
			uiSlots = null;

			newSaveText = "New save";
			isVisible = true;
			isClickable = true;
			numSlots = 1;
			maxSlots = 5;

			SetSize (new Vector2 (20f, 5f));
			anchor = TextAnchor.MiddleCenter;
			saveListType = AC_SaveListType.Save;

			actionListOnSave = null;
			newSaveSlot = false;
			textEffects = TextEffects.None;
			displayType = SaveDisplayType.LabelOnly;
			blankSlotTexture = null;

			fixedOption = false;
			optionToShow = 1;

			importProductName = "";
			importSaveFilename = "";
			checkImportBool = false;
			checkImportVar = 0;

			showNewSaveOption = true;
			autoHandle = true;

			parameterID = -1;

			base.Declare ();
		}


		public override MenuElement DuplicateSelf ()
		{
			MenuSavesList newElement = CreateInstance <MenuSavesList>();
			newElement.Declare ();
			newElement.CopySavesList (this);
			return newElement;
		}
		
		
		public void CopySavesList (MenuSavesList _element)
		{
			uiSlots = _element.uiSlots;

			newSaveText = _element.newSaveText;
			textEffects = _element.textEffects;
			anchor = _element.anchor;
			saveListType = _element.saveListType;
			maxSlots = _element.maxSlots;
			actionListOnSave = _element.actionListOnSave;
			displayType = _element.displayType;
			blankSlotTexture = _element.blankSlotTexture;
			fixedOption = _element.fixedOption;
			optionToShow = _element.optionToShow;
			importProductName = _element.importProductName;
			importSaveFilename = _element.importSaveFilename;
			checkImportBool = _element.checkImportBool;
			checkImportVar = _element.checkImportVar;
			parameterID = _element.parameterID;
			showNewSaveOption = _element.showNewSaveOption;
			autoHandle = _element.autoHandle;
			
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

			fixedOption = EditorGUILayout.Toggle ("Fixed option number?", fixedOption);
			if (fixedOption)
			{
				numSlots = 1;
				slotSpacing = 0f;
				optionToShow = EditorGUILayout.IntField ("Option to display:", optionToShow);
			}
			else
			{
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
			}

			if (source == MenuSource.AdventureCreator)
			{
				anchor = (TextAnchor) EditorGUILayout.EnumPopup ("Text alignment:", anchor);
				textEffects = (TextEffects) EditorGUILayout.EnumPopup ("Text effect:", textEffects);
			}

			displayType = (SaveDisplayType) EditorGUILayout.EnumPopup ("Display:", displayType);
			if (displayType != SaveDisplayType.LabelOnly)
			{
				EditorGUILayout.BeginHorizontal ();
					EditorGUILayout.LabelField ("Empty slot texture:", GUILayout.Width (145f));
					blankSlotTexture = (Texture2D) EditorGUILayout.ObjectField (blankSlotTexture, typeof (Texture2D), false, GUILayout.Width (70f), GUILayout.Height (30f));
				EditorGUILayout.EndHorizontal ();
			}
			saveListType = (AC_SaveListType) EditorGUILayout.EnumPopup ("List type:", saveListType);
			if (saveListType == AC_SaveListType.Save)
			{
				showNewSaveOption = EditorGUILayout.Toggle ("Show 'New save' option?", showNewSaveOption);
				if (showNewSaveOption)
				{
					newSaveText = EditorGUILayout.TextField ("'New save' text:", newSaveText);
				}
				autoHandle = EditorGUILayout.Toggle ("Save when click on?", autoHandle);
				if (autoHandle)
				{
					ActionListGUI ("ActionList after saving:");
				}
				else
				{
					ActionListGUI ("ActionList when click:");
				}
			}
			else if (saveListType == AC_SaveListType.Load)
			{
				autoHandle = EditorGUILayout.Toggle ("Load when click on?", autoHandle);
				if (autoHandle)
				{
					ActionListGUI ("ActionList after loading:");
				}
				else
				{
					ActionListGUI ("ActionList when click:");
				}
			}
			else if (saveListType == AC_SaveListType.Import)
			{
				autoHandle = true;
				#if UNITY_STANDALONE
				importProductName = EditorGUILayout.TextField ("Import product name:", importProductName);
				importSaveFilename = EditorGUILayout.TextField ("Import save filename:", importSaveFilename);
				ActionListGUI ("ActionList after import:");
				checkImportBool = EditorGUILayout.Toggle ("Require Bool to be true?", checkImportBool);
				if (checkImportBool)
				{
					checkImportVar = EditorGUILayout.IntField ("Global Variable ID:", checkImportVar);
				}
				#else
				EditorGUILayout.HelpBox ("This feature is only available for standalone platforms (PC, Mac, Linux)", MessageType.Warning);
				#endif
			}

			if (source != MenuSource.AdventureCreator)
			{
				EditorGUILayout.EndVertical ();
				EditorGUILayout.BeginVertical ("Button");

				if (fixedOption)
				{
					uiSlots = ResizeUISlots (uiSlots, 1);
				}
				else
				{
					uiSlots = ResizeUISlots (uiSlots, maxSlots);
				}
				
				for (int i=0; i<uiSlots.Length; i++)
				{
					uiSlots[i].LinkedUiGUI (i, source);
				}
			}
				
			EditorGUILayout.EndVertical ();
			
			base.ShowGUI (source);
		}


		private void ActionListGUI (string label)
		{
			actionListOnSave = ActionListAssetMenu.AssetGUI (label, actionListOnSave);
			
			if (actionListOnSave != null && actionListOnSave.useParameters && actionListOnSave.parameters.Count > 0)
			{
				EditorGUILayout.BeginVertical ("Button");
				EditorGUILayout.BeginHorizontal ();
				parameterID = Action.ChooseParameterGUI ("", actionListOnSave.parameters, parameterID, ParameterType.Integer);
				if (parameterID >= 0)
				{
					EditorGUILayout.LabelField ("(= Slot index)");
				}
				EditorGUILayout.EndHorizontal ();
				EditorGUILayout.EndVertical ();
			}
		}
		
		#endif


		public override string GetLabel (int slot, int languageNumber)
		{
			if (newSaveSlot && saveListType == AC_SaveListType.Save)
			{
				if (!fixedOption && (slot + offset) == (numSlots-1))
				{
					return TranslateLabel (newSaveText, languageNumber);
				}
			}
			return SaveSystem.GetSaveSlotLabel (slot + offset, optionToShow, fixedOption);
		}


		public override void HideAllUISlots ()
		{
			LimitUISlotVisibility (uiSlots, 0);
		}


		public override void PreDisplay (int _slot, int languageNumber, bool isActive)
		{
			if (displayType != SaveDisplayType.ScreenshotOnly)
			{
				string fullText = "";

				if (newSaveSlot && saveListType == AC_SaveListType.Save)
				{
					if (!fixedOption && (_slot + offset) == (KickStarter.saveSystem.GetNumSaves ()))
					{
						fullText = TranslateLabel (newSaveText, languageNumber);
					}
					else if (fixedOption)
					{
						fullText = TranslateLabel (newSaveText, languageNumber);
					}
					else
					{
						fullText = SaveSystem.GetSaveSlotLabel (_slot + offset, optionToShow, fixedOption);
					}
				}
				else
				{
					if (saveListType == AC_SaveListType.Import)
					{
						fullText = SaveSystem.GetImportSlotLabel (_slot + offset, optionToShow, fixedOption);
					}
					else
					{
						fullText = SaveSystem.GetSaveSlotLabel (_slot + offset, optionToShow, fixedOption);
					}
				}

				if (!Application.isPlaying)
				{
					if (labels == null || labels.Length != numSlots)
					{
						labels = new string [numSlots];
					}
				}

				labels [_slot] = fullText;
			}

			if (Application.isPlaying)
			{
				if (uiSlots != null && uiSlots.Length > _slot)
				{
					LimitUISlotVisibility (uiSlots, numSlots);
					
					if (displayType != SaveDisplayType.LabelOnly)
					{
						Texture2D tex = null;
						if (saveListType == AC_SaveListType.Import)
						{
							tex = SaveSystem.GetImportSlotScreenshot (_slot + offset, optionToShow, fixedOption);
						}
						else
						{
							tex = SaveSystem.GetSaveSlotScreenshot (_slot + offset, optionToShow, fixedOption);
						}
						if (tex == null)
						{
							tex = blankSlotTexture;
						}
						uiSlots[_slot].SetImage (tex);
					}
					if (displayType != SaveDisplayType.ScreenshotOnly)
					{
						uiSlots[_slot].SetText (labels [_slot]);
					}
				}
			}
		}
		
		
		public override void Display (GUIStyle _style, int _slot, float zoom, bool isActive)
		{
			base.Display (_style, _slot, zoom, isActive);

			if (displayType != SaveDisplayType.LabelOnly)
			{
				Texture2D tex = null;
				if (saveListType == AC_SaveListType.Import)
				{
					tex = SaveSystem.GetImportSlotScreenshot (_slot + offset, optionToShow, fixedOption);
				}
				else
				{
					tex = SaveSystem.GetSaveSlotScreenshot (_slot + offset, optionToShow, fixedOption);
				}
				if (tex == null && blankSlotTexture != null)
				{
					tex = blankSlotTexture;
				}

				if (tex != null)
				{
					GUI.DrawTexture (ZoomRect (GetSlotRectRelative (_slot), zoom), tex, ScaleMode.StretchToFill, true, 0f);
				}
			}

			if (displayType != SaveDisplayType.ScreenshotOnly)
			{
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
		}


		public override void ProcessClick (AC.Menu _menu, int _slot, MouseState _mouseState)
		{
			if (KickStarter.stateHandler.gameState == GameState.Cutscene)
			{
				return;
			}

			base.ProcessClick (_menu, _slot, _mouseState);

			bool isSuccess = true;
			if (saveListType == AC_SaveListType.Save && autoHandle)
			{
				if (newSaveSlot && _slot == (numSlots - 1))
				{
					isSuccess = SaveSystem.SaveNewGame ();

					if (KickStarter.settingsManager.orderSavesByUpdateTime)
					{
						offset = 0;
					}
					else
					{
						Shift (AC_ShiftInventory.ShiftRight, 1);
					}
				}
				else
				{
					isSuccess = SaveSystem.SaveGame (_slot + offset, optionToShow, fixedOption);
				}
			}
			else if (saveListType == AC_SaveListType.Load && autoHandle)
			{
				isSuccess = SaveSystem.LoadGame (_slot + offset, optionToShow, fixedOption);
			}
			else if (saveListType == AC_SaveListType.Import)
			{
				isSuccess = SaveSystem.ImportGame (_slot + offset, optionToShow, fixedOption);
			}

			if (isSuccess)
			{
				if (saveListType == AC_SaveListType.Save)
				{
					_menu.TurnOff (true);
				}
				else if (saveListType == AC_SaveListType.Load)
				{
					_menu.TurnOff (false);
				}

				AdvGame.RunActionListAsset (actionListOnSave, parameterID, _slot);
			}
			else if (!autoHandle && saveListType != AC_SaveListType.Import)
			{
				AdvGame.RunActionListAsset (actionListOnSave, parameterID, _slot);
			}
		}

		
		public override void RecalculateSize (MenuSource source)
		{
			newSaveSlot = false;

			if (Application.isPlaying)
			{
				if (saveListType == AC_SaveListType.Import)
				{
					if (checkImportBool)
					{
						KickStarter.saveSystem.GatherImportFiles (importProductName, importSaveFilename, checkImportVar);
					}
					else
					{
						KickStarter.saveSystem.GatherImportFiles (importProductName, importSaveFilename, -1);
					}
				}

				if (fixedOption)
				{
					numSlots = 1;
				}
				else
				{
					if (saveListType == AC_SaveListType.Import)
					{
						numSlots = SaveSystem.GetNumImportSlots ();
					}
					else
					{
						numSlots = SaveSystem.GetNumSlots ();

						if (saveListType == AC_SaveListType.Save && numSlots < KickStarter.settingsManager.maxSaves && showNewSaveOption)
						{
							newSaveSlot = true;
							numSlots ++;
						}
					}

					if (numSlots > maxSlots)
					{
						numSlots = maxSlots;
					}

					offset = Mathf.Min (offset, GetMaxOffset ());
				}
			}

			labels = new string [numSlots];

			if (Application.isPlaying && uiSlots != null)
			{
				ClearSpriteCache (uiSlots);
			}

			if (!isVisible)
			{
				LimitUISlotVisibility (uiSlots, 0);
			}

			base.RecalculateSize (source);
		}
		
		
		protected override void AutoSize ()
		{
			if (displayType == SaveDisplayType.ScreenshotOnly)
			{
				if (blankSlotTexture != null)
				{
					AutoSize (new GUIContent (blankSlotTexture));
				}
				else
				{
					AutoSize (GUIContent.none);
				}
			}
			else if (displayType == SaveDisplayType.LabelAndScreenshot)
			{
				if (blankSlotTexture != null)
				{
					AutoSize (new GUIContent (blankSlotTexture));
				}
				else
				{
					AutoSize (new GUIContent (SaveSystem.GetSaveSlotLabel (0, optionToShow, fixedOption)));
				}
			}
			else
			{
				AutoSize (new GUIContent (SaveSystem.GetSaveSlotLabel (0, optionToShow, fixedOption)));
			}
		}


		public override bool CanBeShifted (AC_ShiftInventory shiftType)
		{
			if (numSlots == 0 || fixedOption)
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
			if (numSlots == 0 || fixedOption)
			{
				return 0;
			}

			return Mathf.Max (0, GetNumFilledSlots () - maxSlots);
		}


		public override void Shift (AC_ShiftInventory shiftType, int amount)
		{
			if (isVisible && numSlots >= maxSlots)
			{
				Shift (shiftType, maxSlots, GetNumFilledSlots (), amount);
			}
		}


		private int GetNumFilledSlots ()
		{
			if (saveListType == AC_SaveListType.Save && !fixedOption && newSaveSlot && showNewSaveOption)
			{
				return KickStarter.saveSystem.GetNumSaves () + 1;
			}
			return KickStarter.saveSystem.GetNumSaves ();
		}

	}

}