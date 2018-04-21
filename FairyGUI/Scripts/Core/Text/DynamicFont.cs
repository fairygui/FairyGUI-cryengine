using System;
using System.Collections.Generic;
using System.Drawing;
using CryEngine;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class DynamicFont : BaseFont
	{
		Font _font;
		int _size;
		FontStyle _style;

		public DynamicFont(string name)
		{
			this.name = name;
			this.scaleEnabled = this.colorEnabled = true;
			LoadFont();
		}

		void LoadFont()
		{
		}

		override public void SetFormat(TextFormat format, float fontSizeScale)
		{
			int size;
			if (fontSizeScale == 1)
				size = format.size;
			else
				size = (int)Math.Floor((float)format.size * fontSizeScale);

			FontStyle style = FontStyle.Regular;
			if (format.bold)
				style |= FontStyle.Bold;
			if (format.italic)
				style |= FontStyle.Italic;
			if (format.underline)
				style |= FontStyle.Underline;

			if (_font == null || _size != size || style != _style)
			{
				_size = size;
				_style = style;
				_font = new Font(this.name, size, style, GraphicsUnit.Pixel);
			}
		}

		public Font nativeFont
		{
			get { return _font; }
		}

		override public bool GetGlyphSize(char ch, out float width, out float height)
		{
			width = height = 0;
			return false;
		}

		override public bool GetGlyph(char ch, GlyphInfo glyph)
		{
			return false;
		}
	}
}
