/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2015
 *	
 *	"OptionsData.cs"
 * 
 *	This script contains any variables we want to appear in our Options menu.
 * 
 */

namespace AC
{

	[System.Serializable]
	public class OptionsData
	{
		
		public int language;
		public bool showSubtitles;
		
		public float sfxVolume;
		public float musicVolume;
		public float speechVolume;

		public string linkedVariables = "";
		public string saveFileNames = "";
		public int lastSaveID = -1;

		public string label;
		public int ID;
		
		
		public OptionsData ()
		{
			language = 0;
			showSubtitles = false;
			
			sfxVolume = 0.9f;
			musicVolume = 0.6f;
			speechVolume = 1f;

			linkedVariables = "";
			saveFileNames = "";
			lastSaveID = -1;

			ID = 0;
			label = "Profile " + (ID + 1).ToString ();
		}


		public OptionsData (int _language, bool _showSubtitles, float _sfxVolume, float _musicVolume, float _speechVolume, int _ID)
		{
			language = _language;
			showSubtitles = _showSubtitles;

			sfxVolume = _sfxVolume;
			musicVolume = _musicVolume;
			speechVolume = _speechVolume;

			linkedVariables = "";
			saveFileNames = "";
			lastSaveID = -1;

			ID = _ID;
			label = "Profile " + (ID + 1).ToString ();
		}


		public OptionsData (OptionsData _optionsData, int _ID)
		{
			language = _optionsData.language;
			showSubtitles = _optionsData.showSubtitles;
			
			sfxVolume = _optionsData.sfxVolume;
			musicVolume = _optionsData.musicVolume;
			speechVolume = _optionsData.speechVolume;
			
			linkedVariables = _optionsData.linkedVariables;
			saveFileNames = _optionsData.saveFileNames;
			lastSaveID = -1;

			ID =_ID;
			label = "Profile " + (ID + 1).ToString ();
		}

	}

}