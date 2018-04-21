﻿using System.Collections.Generic;
using CryEngine;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public enum HitTestMode
	{
		Default,
		Raycast
	}

	/// <summary>
	/// 
	/// </summary>
	public interface IHitTest
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		void SetEnabled(bool value);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="container"></param>
		/// <param name="localPoint"></param>
		/// <returns></returns>
		bool HitTest(Container container, ref Vector2 localPoint);
	}
}
