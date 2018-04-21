using System;
using System.Collections.Generic;
using CryEngine;
using FairyGUI.Utils;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class SelectionShape : DisplayObject
	{
		List<Rect> _rects;
		Color _color;

		public SelectionShape()
		{
			graphics = new NGraphics();
			graphics.texture = NTexture.Empty;
			_color = Color.White;
		}

		/// <summary>
		/// 
		/// </summary>
		public List<Rect> rects
		{
			get { return _rects; }
			set
			{
				_rects = value;

				if (_rects != null)
				{
					int count = _rects.Count;
					if (count > 0)
					{
						_contentRect = _rects[0];
						Rect tmp;
						for (int i = 1; i < count; i++)
						{
							tmp = _rects[i];
							_contentRect = ToolSet.Union(ref _contentRect, ref tmp);
						}
					}
					else
						_contentRect = new Rect(0, 0, 0, 0);
				}
				else
					_contentRect = new Rect(0, 0, 0, 0);
				OnSizeChanged(true, true);
				_requireUpdateMesh = true;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public Color color
		{
			get { return _color; }
			set
			{
				_color = value;
				graphics.Tint(_color);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public void Clear()
		{
			if (_rects != null && _rects.Count > 0)
			{
				_rects.Clear();
				_contentRect = new Rect(0, 0, 0, 0);
				OnSizeChanged(true, true);
				_requireUpdateMesh = true;
			}
		}

		public override void Visit()
		{
			if (_requireUpdateMesh)
			{
				_requireUpdateMesh = false;
				if (_rects != null && _rects.Count > 0)
				{
					graphics.Clear();
					Rect uvRect = new Rect(0, 0, 1, 1);
					for (int i = 0; i < _rects.Count; i++)
						graphics.AddQuad(_rects[i], uvRect, _color);
				}
				else
					graphics.Clear();
			}

			base.Visit();
		}

		protected override DisplayObject HitTest()
		{
			if (_rects == null)
				return null;

			int count = _rects.Count;
			if (count == 0)
				return null;

			Vector2 localPoint = GlobalToLocal(HitTestContext.screenPoint);
			if (!_contentRect.Contains(localPoint.x, localPoint.y))
				return null;

			for (int i = 0; i < count; i++)
			{
				if (_rects[i].Contains(localPoint.x, localPoint.y))
					return this;
			}

			return null;
		}
	}
}
