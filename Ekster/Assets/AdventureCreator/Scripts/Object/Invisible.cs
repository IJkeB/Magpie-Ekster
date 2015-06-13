/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2015
 *	
 *	"Invisible.cs"
 * 
 *	This script makes any gameObject it is attached to invisible.
 * 
 */

using UnityEngine;
using System.Collections;

namespace AC
{

	public class Invisible : MonoBehaviour
	{
		
		void Awake ()
		{
			this.GetComponent <Renderer>().enabled = false;
		}

	}

}