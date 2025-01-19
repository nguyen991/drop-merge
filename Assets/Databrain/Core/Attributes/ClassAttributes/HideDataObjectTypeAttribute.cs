/*
 *	DATABRAIN
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
using System;
using UnityEngine;

namespace Databrain.Attributes
{
	[AttributeUsage(AttributeTargets.Class, Inherited = false)]
	public class HideDataObjectTypeAttribute : Attribute 
	{
		public bool hideFromCreation;

		public HideDataObjectTypeAttribute(bool _hideFromCreation = true)
		{
			hideFromCreation = _hideFromCreation;
		}
	}
}