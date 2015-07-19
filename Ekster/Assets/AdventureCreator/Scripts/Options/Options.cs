/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"Options.cs"
 * 
 *	This script provides a runtime instance of OptionsData,
 *	and has functions for saving and loading this data
 *	into the PlayerPrefs
 * 
 */

using UnityEngine;
using System.Collections;

namespace AC
{
	
	public class Options : MonoBehaviour
	{
		
		public static OptionsData optionsData; // A local copy of the currently-active profile
		public static int maxProfiles = 50;
		
		
		private void Start ()
		{
			LoadPrefs ();
			OnLevelWasLoaded ();
		}
		
		
		public static void SaveDefaultPrefs (OptionsData defaultOptionsData)
		{
			SavePrefsToID (0, defaultOptionsData, false);
		}
		
		
		public static OptionsData LoadDefaultPrefs ()
		{
			return LoadPrefsFromID (0, false, false);
		}
		
		
		public static void DeleteDefaultProfile ()
		{
			DeleteProfilePrefs (0);
		}
		
		
		public static void SavePrefs ()
		{
			if (Application.isPlaying)
			{
				// Linked Variables
				RuntimeVariables.DownloadAll ();
				optionsData.linkedVariables = SaveSystem.CreateVariablesData (KickStarter.runtimeVariables.globalVars, true, VariableLocation.Global);
			}
			
			SavePrefsToID (GetActiveProfileID (), null, true);
			
			if (Application.isPlaying)
			{
				KickStarter.options.CustomSaveOptionsHook ();
			}
		}
		
		
		public static void SavePrefsToID (int ID, OptionsData _optionsData = null, bool showLog = false)
		{
			if (_optionsData == null)
			{
				_optionsData = Options.optionsData;
			}
			
			string optionsSerialized = "";
			if (SaveSystem.GetSaveMethod () == SaveMethod.XML)
			{
				optionsSerialized = Serializer.SerializeObjectXML <OptionsData> (_optionsData);
			}
			else
			{
				optionsSerialized = Serializer.SerializeObjectBinary (_optionsData);
			}
			
			if (optionsSerialized != "")
			{
				PlayerPrefs.SetString (GetPrefKeyName (ID), optionsSerialized);
				if (showLog)
				{
					Debug.Log ("PlayerPrefs Key '" + GetPrefKeyName (ID) + "' saved");
				}
			}
		}
		
		
		public static void LoadPrefs ()
		{
			if (Application.isPlaying)
			{
				KickStarter.options.CustomLoadOptionsHook ();
			}

			optionsData = LoadPrefsFromID (GetActiveProfileID (), Application.isPlaying, true);
			
			if (optionsData.language == 0 && KickStarter.speechManager && KickStarter.speechManager.ignoreOriginalText && KickStarter.speechManager.languages.Count > 1)
			{
				// Ignore original language
				optionsData.language = 1;
				SavePrefs ();
			}
			
			if (Application.isPlaying)
			{
				KickStarter.saveSystem.GatherSaveFiles ();
				PlayerMenus.RecalculateAll ();
			}
		}
		
		
		public static OptionsData LoadPrefsFromID (int ID, bool showLog = false, bool doSave = true)
		{
			if (PlayerPrefs.HasKey (GetPrefKeyName (ID)))
			{
				string optionsSerialized = PlayerPrefs.GetString (GetPrefKeyName (ID));
				
				if (optionsSerialized != null && optionsSerialized.Length > 0)
				{
					bool isXML = optionsSerialized.Contains ("xml version");
					if (SaveSystem.GetSaveMethod () == SaveMethod.XML && isXML)
					{
						if (showLog)
						{
							Debug.Log ("PlayerPrefs Key '" + GetPrefKeyName (ID) + "' loaded");
						}
						return (OptionsData) Serializer.DeserializeObjectXML <OptionsData> (optionsSerialized);
					}
					else if (SaveSystem.GetSaveMethod () == SaveMethod.Binary && !isXML)
					{
						if (showLog)
						{
							Debug.Log ("PlayerPrefs Key '" + GetPrefKeyName (ID) + "' loaded");
						}
						return (OptionsData) Serializer.DeserializeObjectBinary <OptionsData> (optionsSerialized);
					}
				}
			}
			
			// No data exists, so create new
			OptionsData _optionsData = new OptionsData (KickStarter.settingsManager.defaultLanguage, KickStarter.settingsManager.defaultShowSubtitles, KickStarter.settingsManager.defaultSfxVolume, KickStarter.settingsManager.defaultMusicVolume, KickStarter.settingsManager.defaultSpeechVolume, ID);
			if (doSave)
			{
				optionsData = _optionsData;
				SavePrefs ();
			}
			
			return _optionsData;
		}
		
		
		public bool SwitchProfileIfExists (int index, bool includeActive)
		{
			if (KickStarter.settingsManager.useProfiles)
			{
				int ID = ProfileIndexToID (index, includeActive);
				if (PlayerPrefs.HasKey (GetPrefKeyName (ID)))
				{
					SwitchProfile (ID);
					return true;
				}
				Debug.Log ("Profile switch failed - " + index + " doesn't exist");
			}
			return false;
		}
		
		
		public int ProfileIndexToID (int index, bool includeActive = true)
		{
			for (int i=0; i<maxProfiles; i++)
			{
				if (PlayerPrefs.HasKey (GetPrefKeyName (i)))
				{
					if (!includeActive && i == GetActiveProfileID ())
					{}
					else
					{
						index --;
					}
				}
				
				if (index < 0)
				{
					return i;
				}
			}
			return -1;
		}
		
		
		public static int GetActiveProfileID ()
		{
			if (KickStarter.settingsManager.useProfiles)
			{
				return PlayerPrefs.GetInt ("AC_ActiveProfile", 0);
			}
			return 0;
		}
		
		
		public static void SetActiveProfileID (int ID)
		{
			PlayerPrefs.SetInt ("AC_ActiveProfile", ID);
		}
		
		
		private int FindFirstEmptyProfileID ()
		{
			for (int i=0; i<maxProfiles; i++)
			{
				if (!PlayerPrefs.HasKey (GetPrefKeyName (i)))
				{
					return i;
				}
			}
			return 0;
		}
		
		
		public void CreateProfile (string _label = "")
		{
			int newProfileID = FindFirstEmptyProfileID ();
			
			OptionsData newOptionsData = new OptionsData (optionsData, newProfileID);
			if (_label != "")
			{
				newOptionsData.label = _label;
			}
			optionsData = newOptionsData;

			SetActiveProfileID (newProfileID);
			SavePrefs ();
				
			if (Application.isPlaying)
			{
				KickStarter.saveSystem.GatherSaveFiles ();
				PlayerMenus.RecalculateAll ();
			}
		}


		public void RenameProfile (string newProfileLabel, int profileIndex = -2, bool includeActive = true)
		{
			if (!KickStarter.settingsManager.useProfiles || newProfileLabel.Length == 0)
			{
				return;
			}
			
			int profileID = KickStarter.options.ProfileIndexToID (profileIndex, includeActive);
			if (profileID == -1)
			{
				Debug.LogWarning ("Invalid profile index: " + profileIndex + " - nothing to delete!");
				return;
			}
			else if (profileIndex == -2)
			{
				profileID = Options.GetActiveProfileID ();
			}

			if (profileID == GetActiveProfileID ())
			{
				optionsData.label = newProfileLabel;
				SavePrefs ();
			}
			else if (PlayerPrefs.HasKey (GetPrefKeyName (profileID)))
			{
				OptionsData tempOptionsData = LoadPrefsFromID (profileID, false);
				tempOptionsData.label = newProfileLabel;
				SavePrefsToID (profileID, tempOptionsData, true);
			}

			PlayerMenus.RecalculateAll ();
		}


		public string GetProfileName (int index = -1, bool includeActive = true)
		{
			if (index == -1 || !KickStarter.settingsManager.useProfiles)
			{
				return Options.optionsData.label;
			}

			int ID = KickStarter.options.ProfileIndexToID (index, includeActive);

			if (PlayerPrefs.HasKey (GetPrefKeyName (ID)))
			{
				OptionsData tempOptionsData = LoadPrefsFromID (ID, false);
				return tempOptionsData.label;
			}
			else
			{
				return "";
			}
		}
		
		
		public int GetNumProfiles ()
		{
			if (KickStarter.settingsManager.useProfiles)
			{
				int count = 0;
				for (int i=0; i<maxProfiles; i++)
				{
					if (PlayerPrefs.HasKey (GetPrefKeyName (i)))
					{
						count ++;
					}
				}
				return count;
			}
			return 1;
		}
		
		
		public static void DeleteProfilePrefs (int ID)
		{
			bool isDeletingCurrentProfile = false;
			if (ID == GetActiveProfileID ())
			{
				isDeletingCurrentProfile = true;
			}

			Debug.Log ("PlayerPrefs Key '" + GetPrefKeyName (ID) + "' deleted");
			PlayerPrefs.DeleteKey (GetPrefKeyName (ID));
			
			if (isDeletingCurrentProfile)
			{
				for (int i=0; i<maxProfiles; i++)
				{
					if (PlayerPrefs.HasKey (GetPrefKeyName (i)))
					{
						SwitchProfile (i);
						return;
					}
				}
				
				// No other profile found, create new
				SwitchProfile (0);
			}
		}
		
		
		public static void SwitchProfile (int ID)
		{
			SetActiveProfileID (ID);
			LoadPrefs ();

			Debug.Log ("Switched to profile " + ID.ToString () + ": '" + optionsData.label + "'");
			
			if (Application.isPlaying)
			{
				KickStarter.saveSystem.GatherSaveFiles ();
				PlayerMenus.RecalculateAll ();
			}
		}
		
		
		public static string GetPrefKeyName (int ID)
		{
			string profileName = "Profile";
			if (AdvGame.GetReferences ().settingsManager != null && AdvGame.GetReferences ().settingsManager.saveFileName != "")
			{
				profileName = AdvGame.GetReferences ().settingsManager.saveFileName;
				profileName = profileName.Replace (" ", "_");
			}

			return ("AC_" + profileName + "_" + ID.ToString ());
		}


		public static void UpdateSaveLabels (SaveFile[] foundSaveFiles)
		{
			System.Text.StringBuilder newSaveNameData = new System.Text.StringBuilder ();

			if (foundSaveFiles != null)
			{
				foreach (SaveFile saveFile in foundSaveFiles)
				{
					newSaveNameData.Append (saveFile.ID.ToString ());
					newSaveNameData.Append (":");
					newSaveNameData.Append (saveFile.GetSafeLabel ());
					newSaveNameData.Append ("|");
				}
				
				if (foundSaveFiles.Length > 0)
				{
					newSaveNameData.Remove (newSaveNameData.Length - 1, 1);
				}
			}

			optionsData.saveFileNames = newSaveNameData.ToString ();
			SavePrefs ();
		}
		
		
		private void OnLevelWasLoaded ()
		{
			if (KickStarter.settingsManager.IsInLoadingScene ())
			{
				return;
			}
			
			#if UNITY_5
			if (KickStarter.settingsManager.volumeControl == VolumeControl.AudioMixerGroups)
			{
				AdvGame.SetMixerVolume (KickStarter.settingsManager.musicMixerGroup, KickStarter.settingsManager.musicAttentuationParameter, optionsData.musicVolume);
				AdvGame.SetMixerVolume (KickStarter.settingsManager.sfxMixerGroup, KickStarter.settingsManager.sfxAttentuationParameter, optionsData.sfxVolume);
				AdvGame.SetMixerVolume (KickStarter.settingsManager.speechMixerGroup, KickStarter.settingsManager.speechAttentuationParameter, optionsData.speechVolume);
			}
			#endif
			SetVolume (SoundType.Music);
			SetVolume (SoundType.SFX);
		}
		
		
		public void SetVolume (SoundType _soundType)
		{
			Sound[] soundObs = FindObjectsOfType (typeof (Sound)) as Sound[];
			foreach (Sound soundOb in soundObs)
			{
				if (soundOb.soundType == _soundType)
				{
					soundOb.AfterLoading ();
				}
			}
		}
		
		
		public static void SetLanguage (int i)
		{
			if (Options.optionsData != null)
			{
				Options.optionsData.language = i;
				Options.SavePrefs ();
			}
			else
			{
				Debug.LogWarning ("Could not find Options data!");
			}
		}
		
		
		public static string GetLanguageName ()
		{
			return KickStarter.speechManager.languages [GetLanguage ()];
		}
		
		
		public static int GetLanguage ()
		{
			if (Application.isPlaying && optionsData != null)
			{
				return optionsData.language;
			}
			return 0;
		}


		public static float GetSFXVolume ()
		{
			if (Application.isPlaying && optionsData != null)
			{
				return optionsData.sfxVolume;
			}
			return 1f;
		}
		
		
		private void CustomSaveOptionsHook ()
		{
			ISaveOptions[] saveOptionsHooks = GetSaveOptionsHooks (GetComponents (typeof (ISaveOptions)));
			if (saveOptionsHooks != null && saveOptionsHooks.Length > 0)
			{
				foreach (ISaveOptions saveOptionsHook in saveOptionsHooks)
				{
					saveOptionsHook.PreSaveOptions ();
				}
			}
		}
		
		
		private void CustomLoadOptionsHook ()
		{
			ISaveOptions[] saveOptionsHooks = GetSaveOptionsHooks (GetComponents (typeof (ISaveOptions)));
			if (saveOptionsHooks != null && saveOptionsHooks.Length > 0)
			{
				foreach (ISaveOptions saveOptionsHook in saveOptionsHooks)
				{
					saveOptionsHook.PostLoadOptions ();
				}
			}
		}
		
		
		private ISaveOptions[] GetSaveOptionsHooks (IList list)
		{
			ISaveOptions[] ret = new ISaveOptions[list.Count];
			list.CopyTo (ret, 0);
			return ret;
		}
		
	}
	
}