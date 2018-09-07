using System.Text;
using CryEngine;

namespace FairyGUI.Utils
{
	/// <summary>
	/// 
	/// </summary>
	public static class ToolSet
	{
		public static Color ConvertFromHtmlColor(string str)
		{
			if (str.Length < 7 || str[0] != '#')
				return Color.Black;

			if (str.Length == 9)
			{
				return new Color((float)(CharToHex(str[3]) * 16 + CharToHex(str[4])) / 255,
					(float)(CharToHex(str[5]) * 16 + CharToHex(str[6])) / 255,
					(float)(CharToHex(str[7]) * 16 + CharToHex(str[8])) / 255,
					(float)(CharToHex(str[1]) * 16 + CharToHex(str[2])) / 255);
			}
			else
			{
				return new Color((float)(CharToHex(str[1]) * 16 + CharToHex(str[2])) / 255,
					(float)(CharToHex(str[3]) * 16 + CharToHex(str[4])) / 255,
					(float)(CharToHex(str[5]) * 16 + CharToHex(str[6])) / 255,
					1);
			}
		}

		public static Color ColorFromRGB(int value)
		{
			return new Color(((value >> 16) & 0xFF) / 255f, ((value >> 8) & 0xFF) / 255f, (value & 0xFF) / 255f, 1);
		}

		public static Color ColorFromRGBA(int value)
		{
			return new Color(((value >> 16) & 0xFF) / 255f, ((value >> 8) & 0xFF) / 255f, (value & 0xFF) / 255f, ((value >> 24) & 0xFF) / 255f);
		}

		public static int CharToHex(char c)
		{
			if (c >= '0' && c <= '9')
				return (int)c - 48;
			if (c >= 'A' && c <= 'F')
				return 10 + (int)c - 65;
			else if (c >= 'a' && c <= 'f')
				return 10 + (int)c - 97;
			else
				return 0;
		}

		public static Rect Intersection(ref Rect rect1, ref Rect rect2)
		{
			if (rect1.Width == 0 || rect1.Height == 0 || rect2.Width == 0 || rect2.Height == 0)
				return new Rect(0, 0, 0, 0);

			float left = rect1.x > rect2.x ? rect1.x : rect2.x;
			float right = (rect1.x + rect1.Width) < (rect2.x + rect2.Width) ? (rect1.x + rect1.Width) : (rect2.x + rect2.Width);
			float top = rect1.y > rect2.y ? rect1.y : rect2.y;
			float bottom = (rect1.y + rect1.Height) < (rect2.y + rect2.Height) ? (rect1.y + rect1.Height) : (rect2.y + rect2.Height);

			if (left > right || top > bottom)
				return new Rect(0, 0, 0, 0);
			else
				return new Rect(left, top, right - left, bottom - top);
		}

		public static Rect Union(ref Rect rect1, ref Rect rect2)
		{
			if (rect2.Width == 0 || rect2.Height == 0)
				return rect1;

			if (rect1.Width == 0 || rect1.Height == 0)
				return rect2;

			float x = MathHelpers.Min(rect1.x, rect2.x);
			float y = MathHelpers.Min(rect1.y, rect2.y);
			return new Rect(x, y, MathHelpers.Max(rect1.x + rect1.Width, rect2.x + rect2.Width) - x,
				MathHelpers.Max(rect1.y + rect1.Height, rect2.y + rect2.Height) - y);
		}

		public static void FlipRect(ref Rect rect, FlipType flip)
		{
			if (flip == FlipType.Horizontal || flip == FlipType.Both)
			{
				float tmp = rect.x;
				rect.x = rect.x + rect.Width;
				rect.Width = tmp - rect.x;
			}
			if (flip == FlipType.Vertical || flip == FlipType.Both)
			{
				float tmp = rect.y;
				rect.y = rect.y + rect.Height;
				rect.Height = tmp - rect.y;
			}
		}

		public static void FlipInnerRect(float sourceWidth, float sourceHeight, ref Rect rect, FlipType flip)
		{
			if (flip == FlipType.Horizontal || flip == FlipType.Both)
			{
				rect.x = sourceWidth - rect.x - rect.Width;
			}

			if (flip == FlipType.Vertical || flip == FlipType.Both)
			{
				rect.y = sourceHeight - rect.y - rect.Height;
			}
		}

		static Vector2[] sHelperPoints = new Vector2[4];
		public static void TransformRect(ref Rect rect, ref Matrix4x4 localToWorld, ref Matrix4x4 worldToLocal)
		{
			sHelperPoints[0] = new Vector2(rect.x, rect.y);
			sHelperPoints[1] = new Vector2(rect.x + rect.Width, rect.y);
			sHelperPoints[2] = new Vector2(rect.x, rect.y + rect.Height);
			sHelperPoints[3] = new Vector2(rect.x + rect.Width, rect.y + rect.Height);

			rect.x = float.MaxValue;
			rect.y = float.MaxValue;
			float maxX = float.MinValue;
			float maxY = float.MinValue;

			for (int i = 0; i < 4; i++)
			{
				Vector3 v = localToWorld.TransformPoint(new Vector3(sHelperPoints[i].x, sHelperPoints[i].y, 0));
				v = worldToLocal.TransformPoint(v);

				if (v.x < rect.x) rect.x = v.x;
				if (v.x > maxX) maxX = v.x;
				if (v.y < rect.y) rect.y = v.y;
				if (v.y > maxY) maxY = v.y;
			}

			rect.Width = maxX - rect.x;
			rect.Height = maxY - rect.y;
		}

		public static void uvLerp(Vector2[] uvSrc, Vector2[] uvDest, float min, float max)
		{
			float uMin = float.MaxValue;
			float uMax = float.MinValue;
			float vMin = float.MaxValue;
			float vMax = float.MinValue;
			int len = uvSrc.Length;
			for (int i = 0; i < len; i++)
			{
				Vector2 v = uvSrc[i];
				if (v.x < uMin)
					uMin = v.x;
				if (v.x > uMax)
					uMax = v.x;
				if (v.y < vMin)
					vMin = v.y;
				if (v.y > vMax)
					vMax = v.y;
			}
			float uLen = uMax - uMin;
			float vLen = vMax - vMin;
			for (int i = 0; i < len; i++)
			{
				Vector2 v = uvSrc[i];
				v.x = (v.x - uMin) / uLen;
				v.y = (v.y - vMin) / vLen;
				uvDest[i] = v;
			}
		}

		//格式化回车符，使只出现\n
		public static string FormatCRLF(string source)
		{
			int pos = source.IndexOf("\r");
			if (pos != -1)
			{
				int len = source.Length;
				StringBuilder buffer = new StringBuilder();
				int lastPos = 0;
				while (pos != -1)
				{
					buffer.Append(source, lastPos, pos);
					if (pos == len - 1 || source[pos + 1] != '\n')
						buffer.Append('\n');

					lastPos = pos + 1;
					if (lastPos >= len)
						break;

					pos = source.IndexOf("\r", lastPos);
				}
				if (lastPos < len)
					buffer.Append(source, lastPos, len - lastPos);

				source = buffer.ToString();
			}

			return source;
		}

		//From Starling
		public static bool IsPointInTriangle(ref Vector2 p, ref Vector2 a, ref Vector2 b, ref Vector2 c)
		{
			// This algorithm is described well in this article:
			// http://www.blackpawn.com/texts/pointinpoly/default.html

			float v0x = c.x - a.x;
			float v0y = c.y - a.y;
			float v1x = b.x - a.x;
			float v1y = b.y - a.y;
			float v2x = p.x - a.x;
			float v2y = p.y - a.y;

			float dot00 = v0x * v0x + v0y * v0y;
			float dot01 = v0x * v1x + v0y * v1y;
			float dot02 = v0x * v2x + v0y * v2y;
			float dot11 = v1x * v1x + v1y * v1y;
			float dot12 = v1x * v2x + v1y * v2y;

			float invDen = 1.0f / (dot00 * dot11 - dot01 * dot01);
			float u = (dot11 * dot02 - dot01 * dot12) * invDen;
			float v = (dot00 * dot12 - dot01 * dot02) * invDen;

			return (u >= 0) && (v >= 0) && (u + v < 1);
		}

		public static bool EqualColor(ref Color c1, ref Color c2)
		{
			return c1.A == c2.A
				&& c1.R == c2.R
				&& c1.G == c2.G
				&& c1.B == c2.B;
		}	}
}
