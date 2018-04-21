﻿using System;
using System.Collections.Generic;
using CryEngine;

namespace FairyGUI.Utils
{
	/// <summary>
	/// Create->SetPosition->(Add<->Remove)->Release->Dispose
	/// </summary>
	public interface IHtmlObject
	{
		float width { get; }
		float height { get; }
		DisplayObject displayObject { get; }
		HtmlElement element { get; }

		void Create(RichTextField owner, HtmlElement element);
		void SetPosition(float x, float y);
		void Add();
		void Remove();
		void Release();
		void Dispose();
	}
}
