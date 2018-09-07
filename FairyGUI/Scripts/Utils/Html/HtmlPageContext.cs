﻿using System.Collections.Generic;
using CryEngine;

namespace FairyGUI.Utils
{
	/// <summary>
	/// 
	/// </summary>
	public class HtmlPageContext : IHtmlPageContext
	{
		Stack<IHtmlObject> _imagePool;
		Stack<IHtmlObject> _inputPool;
		Stack<IHtmlObject> _buttonPool;
		Stack<IHtmlObject> _selectPool;
		Stack<IHtmlObject> _linkPool;

		public static HtmlPageContext inst = new HtmlPageContext();

		public HtmlPageContext()
		{
			_imagePool = new Stack<IHtmlObject>();
			_inputPool = new Stack<IHtmlObject>();
			_buttonPool = new Stack<IHtmlObject>();
			_selectPool = new Stack<IHtmlObject>();
			_linkPool = new Stack<IHtmlObject>();
		}

		virtual public IHtmlObject CreateObject(RichTextField owner, HtmlElement element)
		{
			IHtmlObject ret = null;
			if (element.type == HtmlElementType.Image)
			{
				if (_imagePool.Count > 0)
					ret = _imagePool.Pop();
				else
					ret = new HtmlImage();
			}
			else if (element.type == HtmlElementType.Link)
			{
				if (_linkPool.Count > 0)
					ret = _linkPool.Pop();
				else
					ret = new HtmlLink();
			}
			else if (element.type == HtmlElementType.Input)
			{
				string type = element.GetString("type");
				if (type != null)
					type = type.ToLower();
				if (type == "button" || type == "submit")
				{
					if (_buttonPool.Count > 0)
						ret = _buttonPool.Pop();
					else
						ret = new HtmlButton();
				}
				else
				{
					if (_inputPool.Count > 0)
						ret = _inputPool.Pop();
					else
						ret = new HtmlInput();
				}
			}
			else if (element.type == HtmlElementType.Select)
			{
				if (_selectPool.Count > 0)
					ret = _selectPool.Pop();
				else
					ret = new HtmlSelect();
			}

			if (ret != null)
			{
				ret.Create(owner, element);
			}

			return ret;
		}

		virtual public void FreeObject(IHtmlObject obj)
		{
			obj.Release();

			//可能已经被GameObject tree deleted了，不再回收
			if (obj.displayObject != null && obj.displayObject.isDisposed)
				return;

			if (obj is HtmlImage)
				_imagePool.Push(obj);
			else if (obj is HtmlInput)
				_inputPool.Push(obj);
			else if (obj is HtmlButton)
				_buttonPool.Push(obj);
			else if (obj is HtmlLink)
				_linkPool.Push(obj);
		}

		virtual public NTexture GetImageTexture(HtmlImage image)
		{
			return null;
		}

		virtual public void FreeImageTexture(HtmlImage image, NTexture texture)
		{
		}
	}
}
