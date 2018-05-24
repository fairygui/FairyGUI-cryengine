using System;
using System.Collections.Generic;
using CryEngine;
using CryEngine.Common;
using FairyGUI.Utils;
using CryEngine.Resources;

namespace FairyGUI
{
	public class RenderTarget
	{
		public NTexture texture;
		public Vector2 origin;
	}

	/// <summary>
	/// 
	/// </summary>
	public class NGraphics
	{
		public bool enabled;
		public bool skipMask;
		public BlendMode blendMode;
		public bool pixelSnapping;

		class Quad
		{
			public Rect drawRect;
			public Vector2[] uv;
			public Color color;
		}
		List<Quad> _quads;
		int _quadCount;
		NTexture _texture;

		internal static Vector2 viewportReverseScale;

		public NGraphics()
		{
			enabled = true;
			blendMode = BlendMode.Normal;
			_quads = new List<Quad>(1);
			_quadCount = 0;

			Stats.LatestGraphicsCreation++;
		}

		/// <summary>
		/// 
		/// </summary>
		public NTexture texture
		{
			get { return _texture; }
			set
			{
				_texture = value;
			}
		}

		static Matrix4x4 sHelperMatrix = new Matrix4x4();
		public void Render(Rect rect, Vector2 scale, float rotation, float alpha, UpdateContext context)
		{
			if (_texture == null || !enabled)
				return;

			if (skipMask && UpdateContext.current.clipped)
				UpdateContext.current.SkipMask(true);

			if (rotation != 0)
				sHelperMatrix = MatrixHelper.RotateZ(rotation);

			alpha *= context.alpha;
			RenderTarget renderTarget = context.renderTarget;
			if (context.blendMode != blendMode)
			{
				context.blendMode = blendMode;
				BlendModeUtils.Apply(blendMode);
			}

			for (int i = 0; i < _quadCount; i++)
			{
				Quad quad = _quads[i];
				float x, y, w, h;
				x = quad.drawRect.x * scale.x;
				y = quad.drawRect.y * scale.y;
				w = quad.drawRect.Width * scale.x;
				h = quad.drawRect.Height * scale.y;
				if (quad.drawRect.Width >= 1 && w < 1)
					w = 1;
				if (quad.drawRect.Height >= 1 && h < 1)
					h = 1;
				Vector2 uv0 = quad.uv[0];
				Vector2 uv2 = quad.uv[2];

				if (rotation != 0)
				{
					//底层选装的轴心在中间，要转换一下
					Vector3 cePivot = new Vector3(w * 0.5f, h * 0.5f, 0);
					Vector3 pos = sHelperMatrix.TransformPoint(new Vector3(x, y, 0)) + sHelperMatrix.TransformPoint(cePivot) - cePivot;
					x = pos.x;
					y = pos.y;
				}

				if (renderTarget == null)
				{
					if (pixelSnapping)
					{
						x = (float)Math.Floor(rect.x + x) * viewportReverseScale.x;
						y = (float)Math.Floor(rect.y + y) * viewportReverseScale.y;
					}
					else
					{
						x = (rect.x + x) * viewportReverseScale.x;
						y = (rect.y + y) * viewportReverseScale.y;
					}

					w *= viewportReverseScale.x;
					h *= viewportReverseScale.y;
#if CE_5_5
					IRenderAuxImage.Draw2dImage(x, y, w, h
						_texture.ID,
						uv0.x, uv0.y, uv2.x, uv2.y,
						rotation,
						quad.color.R, quad.color.G, quad.color.B, quad.color.A * alpha);
#else
					Global.gEnv.pRenderer.Draw2dImage(x, y, w, h,
						_texture.ID,
						uv0.x, uv0.y, uv2.x, uv2.y,
						rotation,
						quad.color.R, quad.color.G, quad.color.B, quad.color.A * alpha);
#endif
				}
				else
				{
#if CE_5_5

#else
					Global.gEnv.pRenderer.PushUITexture(_texture.ID, renderTarget.texture.ID,
						 (rect.x + x - renderTarget.origin.x) / renderTarget.texture.width, (rect.y + y - renderTarget.origin.y) / renderTarget.texture.height,
						 w / renderTarget.texture.width, h / renderTarget.texture.height,
						 uv0.x, uv0.y, uv2.x, uv2.y,
						 quad.color.R, quad.color.G, quad.color.B, quad.color.A * alpha);
#endif
				}
			}

			if (skipMask && UpdateContext.current.clipped)
				UpdateContext.current.SkipMask(true);
		}

		public void AddQuad(Rect drawRect, Rect uvRect, Color color)
		{
			Quad quad;
			if (_quadCount < _quads.Count)
				quad = _quads[_quadCount++];
			else
			{
				quad = new Quad();
				_quads.Add(quad);
				_quadCount++;
			}
			quad.drawRect = drawRect;
			quad.uv = new Vector2[4];
			quad.uv[0] = new Vector2(uvRect.x, uvRect.y + uvRect.Height);
			quad.uv[1] = new Vector2(uvRect.x + uvRect.Width, uvRect.y + uvRect.Height);
			quad.uv[2] = new Vector2(uvRect.x + uvRect.Width, uvRect.y);
			quad.uv[3] = new Vector2(uvRect.x, uvRect.y);
			quad.color = color;
		}

		public void AddQuad(Rect drawRect, Vector2[] uv, Color color)
		{
			Quad quad;
			if (_quadCount < _quads.Count)
				quad = _quads[_quadCount++];
			else
			{
				quad = new Quad();
				_quads.Add(quad);
				_quadCount++;
			}

			quad.drawRect = drawRect;
			quad.uv = new Vector2[4];
			Array.Copy(uv, quad.uv, 4);
			quad.color = color;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="vertRect"></param>
		/// <param name="lineSize"></param>
		/// <param name="lineColor"></param>
		/// <param name="fillColor"></param>
		/// <param name="allColors"></param>
		public void DrawRect(Rect vertRect, int lineSize, Color lineColor, Color fillColor)
		{
			Clear();

			if (lineSize == 0)
			{
				AddQuad(new Rect(0, 0, vertRect.Width, vertRect.Height), new Rect(0, 0, 1, 1), fillColor);
			}
			else
			{
				Rect rect;
				Rect uvRect = new Rect(0, 0, 1, 1);
				//left,right
				rect = new Rect(0, 0, lineSize, vertRect.Height);
				AddQuad(rect, uvRect, lineColor);
				rect = new Rect(vertRect.Width - lineSize, 0, lineSize, vertRect.Height);
				AddQuad(rect, uvRect, lineColor);

				//top, bottom
				rect = new Rect(lineSize, 0, vertRect.Width - lineSize * 2, lineSize);
				AddQuad(rect, uvRect, lineColor);
				rect = new Rect(lineSize, vertRect.Height - lineSize, vertRect.Width - lineSize * 2, lineSize);
				AddQuad(rect, uvRect, lineColor);

				//middle
				rect = new Rect(lineSize, lineSize, vertRect.Width - lineSize * 2, vertRect.Height - lineSize * 2);
				AddQuad(rect, uvRect, fillColor);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public void Clear()
		{
			_quadCount = 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		public void Tint(Color value)
		{
			for (int i = 0; i < _quadCount; i++)
			{
				Quad quad = _quads[i];
				quad.color = value;
			}
		}

		public void RotateUV(ref Rect baseUVRect)
		{
			float xMin = Math.Min(baseUVRect.x, baseUVRect.x + baseUVRect.Width);
			float yMin = baseUVRect.y;
			float yMax = baseUVRect.y + baseUVRect.Height;
			if (yMin > yMax)
			{
				yMin = yMax;
				yMax = baseUVRect.y;
			}

			for (int i = 0; i < _quadCount; i++)
			{
				Quad quad = _quads[i];
				float tmp;
				for (int j = 0; j < 4; j++)
				{
					Vector2 m = quad.uv[j];
					tmp = m.y;
					m.y = yMin + m.x - xMin;
					m.x = xMin + yMax - tmp;
					quad.uv[j] = m;
				}
			}
		}

		public static void RotateUV(Vector2[] uv, ref Rect baseUVRect)
		{
			int vertCount = uv.Length;
			float xMin = Math.Min(baseUVRect.x, baseUVRect.x + baseUVRect.Width);
			float yMin = baseUVRect.y;
			float yMax = baseUVRect.y + baseUVRect.Height;
			if (yMin > yMax)
			{
				yMin = yMax;
				yMax = baseUVRect.y;
			}

			float tmp;
			for (int i = 0; i < vertCount; i++)
			{
				Vector2 m = uv[i];
				tmp = m.y;
				m.y = yMin + m.x - xMin;
				m.x = xMin + yMax - tmp;
				uv[i] = m;
			}
		}
	}
}
