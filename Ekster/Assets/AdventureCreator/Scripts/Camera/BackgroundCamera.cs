/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"BackgroundCamera.cs"
 * 
 *	The BackgroundCamera is used to display background images underneath the scene geometry.
 * 
 */

using UnityEngine;
using System.Collections;

namespace AC
{
	
	[RequireComponent (typeof (Camera))]
	public class BackgroundCamera : MonoBehaviour
	{
		
		private Camera _camera;
		
		
		private void Awake ()
		{
			_camera = GetComponent <Camera>();
			
			UpdateRect ();
			
			if (KickStarter.settingsManager)
			{
				if (LayerMask.NameToLayer (KickStarter.settingsManager.backgroundImageLayer) == -1)
				{
					Debug.LogWarning ("No '" + KickStarter.settingsManager.backgroundImageLayer + "' layer exists - please define one in the Tags Manager.");
				}
				else
				{
					GetComponent <Camera>().cullingMask = (1 << LayerMask.NameToLayer (KickStarter.settingsManager.backgroundImageLayer));
				}
			}
			else
			{
				Debug.LogWarning ("A Settings Manager is required for this camera type");
			}
		}
		
		
		public void UpdateRect ()
		{
			if (_camera == null)
			{
				_camera = GetComponent <Camera>();
			}
			_camera.rect = Camera.main.rect;
		}
		
	}
	
}