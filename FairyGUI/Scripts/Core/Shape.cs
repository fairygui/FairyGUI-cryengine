using CryEngine;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class Shape : DisplayObject
	{
		int _type;
		int _lineSize;
		Color _lineColor;
		Color _fillColor;

		/// <summary>
		/// 
		/// </summary>
		public Shape()
		{
			graphics = new NGraphics();
			graphics.texture = NTexture.Empty;
		}

		/// <summary>
		/// 
		/// </summary>
		public bool empty
		{
			get { return _type == 0; }
		}

		/// <summary>
		/// 
		/// </summary>
		public Color color
		{
			get { return _fillColor; }
			set
			{
				_fillColor = value;
				_requireUpdateMesh = true;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="lineSize"></param>
		/// <param name="lineColor"></param>
		/// <param name="fillColor"></param>
		public void DrawRect(int lineSize, Color lineColor, Color fillColor)
		{
			_type = 1;
			_lineSize = lineSize;
			_lineColor = lineColor;
			_fillColor = fillColor;

			_touchDisabled = false;
			_requireUpdateMesh = true;
		}

		/// <summary>
		/// 
		/// </summary>
		public void Clear()
		{
			_type = 0;
			_touchDisabled = true;
			graphics.Clear();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="context"></param>
		public override void Visit()
		{
			if (_requireUpdateMesh)
			{
				_requireUpdateMesh = false;
				if (_type != 0)
				{
					if (_contentRect.Width > 0 && _contentRect.Height > 0)
					{
						if (_type == 1)
							graphics.DrawRect(_contentRect, _lineSize, _lineColor, _fillColor);
					}
					else
						graphics.Clear();
				}
			}

			base.Visit();
		}
	}
}
