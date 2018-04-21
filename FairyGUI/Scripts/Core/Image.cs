using CryEngine;
using FairyGUI.Utils;
using System;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public enum FlipType
	{
		None,
		Horizontal,
		Vertical,
		Both
	}

	/// <summary>
	/// 
	/// </summary>
	public class Image : DisplayObject
	{
		protected NTexture _texture;
		protected Color _color;
		protected FlipType _flip;
		protected Rect? _scale9Grid;
		protected bool _scaleByTile;
		protected int _tileGridIndice;

		public Image() : this(null)
		{
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="texture"></param>
		public Image(NTexture texture)
		{
			_touchDisabled = true;
			graphics = new NGraphics();

			_color = Color.White;
			if (texture != null)
				UpdateTexture(texture);
		}

		void Create(NTexture texture)
		{

		}

		/// <summary>
		/// 
		/// </summary>
		public NTexture texture
		{
			get { return _texture; }
			set
			{
				UpdateTexture(value);
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
				_requireUpdateMesh = true;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public FlipType flip
		{
			get { return _flip; }
			set
			{
				if (_flip != value)
				{
					_flip = value;
					_requireUpdateMesh = true;
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public Rect? scale9Grid
		{
			get { return _scale9Grid; }
			set
			{
				if (_scale9Grid == null && value == null)
					return;

				_scale9Grid = value;
				_requireUpdateMesh = true;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public bool scaleByTile
		{
			get { return _scaleByTile; }
			set
			{
				if (_scaleByTile != value)
				{
					_scaleByTile = value;
					_requireUpdateMesh = true;
				}
			}
		}

		public int tileGridIndice
		{
			get { return _tileGridIndice; }
			set
			{
				if (_tileGridIndice != value)
				{
					_tileGridIndice = value;
					_requireUpdateMesh = true;
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public void SetNativeSize()
		{
			float oldWidth = _contentRect.Width;
			float oldHeight = _contentRect.Height;
			if (_texture != null)
			{
				_contentRect.Width = _texture.width;
				_contentRect.Height = _texture.height;
			}
			else
			{
				_contentRect.Width = 0;
				_contentRect.Height = 0;
			}
			if (oldWidth != _contentRect.Width || oldHeight != _contentRect.Height)
				OnSizeChanged(true, true);
		}

		public override void Visit()
		{
			if (_requireUpdateMesh)
				Rebuild();

			base.Visit();
		}

		virtual protected void UpdateTexture(NTexture value)
		{
			if (value == _texture)
				return;

			_requireUpdateMesh = true;
			_texture = value;
			if (_contentRect.Width == 0)
				SetNativeSize();
			graphics.texture = _texture;
		}

		static int[] gridTileIndice = new int[] { -1, 0, -1, 2, 4, 3, -1, 1, -1 };
		static float[] gridX = new float[4];
		static float[] gridY = new float[4];
		static float[] gridTexX = new float[4];
		static float[] gridTexY = new float[4];

		void GenerateGrids(Rect gridRect, Rect uvRect)
		{
			float sx = uvRect.Width / (float)_texture.width;
			float sy = uvRect.Height / (float)_texture.height;
			gridTexX[0] = uvRect.x;
			gridTexX[1] = uvRect.x + gridRect.x * sx;
			gridTexX[2] = uvRect.x + (gridRect.x + gridRect.Width) * sx;
			gridTexX[3] = uvRect.x + uvRect.Width;
			gridTexY[0] = uvRect.y + uvRect.Height;
			gridTexY[1] = uvRect.y + uvRect.Height - gridRect.y * sy;
			gridTexY[2] = uvRect.y + uvRect.Height - (gridRect.y + gridRect.Height) * sy;
			gridTexY[3] = uvRect.y;

			if (_contentRect.Width >= (_texture.width - gridRect.Width))
			{
				gridX[1] = gridRect.x;
				gridX[2] = _contentRect.Width - (_texture.width - (gridRect.x + gridRect.Width));
				gridX[3] = _contentRect.Width;
			}
			else
			{
				float tmp = gridRect.x / (_texture.width - (gridRect.x + gridRect.Width));
				tmp = _contentRect.Width * tmp / (1 + tmp);
				gridX[1] = tmp;
				gridX[2] = tmp;
				gridX[3] = _contentRect.Width;
			}

			if (_contentRect.Height >= (_texture.height - gridRect.Height))
			{
				gridY[1] = gridRect.y;
				gridY[2] = _contentRect.Height - (_texture.height - (gridRect.y + gridRect.Height));
				gridY[3] = _contentRect.Height;
			}
			else
			{
				float tmp = gridRect.y / (_texture.height - (gridRect.y + gridRect.Height));
				tmp = _contentRect.Height * tmp / (1 + tmp);
				gridY[1] = tmp;
				gridY[2] = tmp;
				gridY[3] = _contentRect.Height;
			}
		}

		void TileFill(Rect destRect, Rect uvRect, float sourceW, float sourceH)
		{
			int hc = (int)Math.Ceiling(destRect.Width / sourceW);
			int vc = (int)Math.Ceiling(destRect.Height / sourceH);
			float tailWidth = destRect.Width - (hc - 1) * sourceW;
			float tailHeight = destRect.Height - (vc - 1) * sourceH;

			for (int i = 0; i < hc; i++)
			{
				for (int j = 0; j < vc; j++)
				{
					Rect uvTmp = uvRect;
					if (i == hc - 1)
						uvTmp.Width = MathHelpers.Lerp(uvRect.x, uvRect.x + uvRect.Width, tailWidth / sourceW) - uvTmp.x;
					if (j == vc - 1)
					{
						uvTmp.Height = MathHelpers.Lerp(0, uvRect.Height, tailHeight / sourceH);
						uvTmp.y += (uvRect.Height - uvTmp.Height);
					}

					graphics.AddQuad(new Rect(destRect.x + i * sourceW, destRect.y + j * sourceH,
							i == (hc - 1) ? tailWidth : sourceW, j == (vc - 1) ? tailHeight : sourceH), uvTmp,
							_color);
				}
			}
		}

		virtual protected void Rebuild()
		{
			_requireUpdateMesh = false;
			graphics.Clear();

			if (_texture == null)
				return;

			Rect uvRect = _texture.uvRect;
			if (_flip != FlipType.None)
				ToolSet.FlipRect(ref uvRect, _flip);

			if (_texture.width == _contentRect.Width && _texture.height == _contentRect.Height)
			{
				graphics.AddQuad(_contentRect, uvRect, _color);
			}
			else if (_scaleByTile)
			{
				TileFill(_contentRect, uvRect, _texture.width, _texture.height);
			}
			else if (_scale9Grid != null)
			{
				Rect gridRect = (Rect)_scale9Grid;

				if (_flip != FlipType.None)
					ToolSet.FlipInnerRect(_texture.width, _texture.height, ref gridRect, _flip);

				GenerateGrids(gridRect, uvRect);

				for (int i = 0; i < 9; i++)
				{
					int col = i % 3;
					int row = i / 3;

					graphics.AddQuad(new Rect(gridX[col], gridY[row], gridX[col + 1] - gridX[col], gridY[row + 1] - gridY[row]),
						new Rect(gridTexX[col], gridTexY[row + 1], gridTexX[col + 1] - gridTexX[col], gridTexY[row] - gridTexY[row + 1]),
						_color);
				}
			}
			else
			{
				graphics.AddQuad(_contentRect, uvRect, _color);
			}

			if (_texture.rotated)
				graphics.RotateUV(ref uvRect);
		}
	}
}
