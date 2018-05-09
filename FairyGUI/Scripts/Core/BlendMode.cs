using System;
using System.Collections.Generic;
using CryEngine;
using CryEngine.Common;

namespace FairyGUI
{
	/*关于BlendMode.Off, 这种模式相当于Blend Off指令的效果。当然，在着色器里使用Blend Off指令可以获得更高的效率，
		但因为Image着色器本身就有多个关键字，复制一个这样的着色器代价太大，所有为了节省Shader数量便增加了这样一种模式，也是可以接受的。
	*/

	/// <summary>
	/// 
	/// </summary>
	public enum BlendMode
	{
		Normal,
		None,
		Add,
		Multiply,
		Screen,
		Erase,
		Mask,
		Below,
		Off,
		Custom1,
		Custom2,
		Custom3
	}

	/// <summary>
	/// 
	/// </summary>
	public class BlendModeUtils
	{
		// Render State flags (uint32)
		public const int GS_BLSRC_ZERO = 0x01;
		public const int GS_BLSRC_ONE = 0x02;
		public const int GS_BLSRC_DSTCOL = 0x03;
		public const int GS_BLSRC_ONEMINUSDSTCOL = 0x04;
		public const int GS_BLSRC_SRCALPHA = 0x05;
		public const int GS_BLSRC_ONEMINUSSRCALPHA = 0x06;
		public const int GS_BLSRC_DSTALPHA = 0x07;
		public const int GS_BLSRC_ONEMINUSDSTALPHA = 0x08;
		public const int GS_BLSRC_ALPHASATURATE = 0x09;
		public const int GS_BLSRC_SRCALPHA_A_ZERO = 0x0A;  // separate alpha blend state
		public const int GS_BLSRC_SRC1ALPHA = 0x0B;  // dual source blending
		public const int GS_BLSRC_MASK = 0x0F;
		public const int GS_BLSRC_SHIFT = 0;

		public const int GS_BLDST_ZERO = 0x10;
		public const int GS_BLDST_ONE = 0x20;
		public const int GS_BLDST_SRCCOL = 0x30;
		public const int GS_BLDST_ONEMINUSSRCCOL = 0x40;
		public const int GS_BLDST_SRCALPHA = 0x50;
		public const int GS_BLDST_ONEMINUSSRCALPHA = 0x60;
		public const int GS_BLDST_DSTALPHA = 0x70;
		public const int GS_BLDST_ONEMINUSDSTALPHA = 0x80;
		public const int GS_BLDST_ONE_A_ZERO = 0x90; // separate alpha blend state
		public const int GS_BLDST_ONEMINUSSRC1ALPHA = 0xA0; // dual source blending
		public const int GS_BLDST_MASK = 0xF0;
		public const int GS_BLDST_SHIFT = 4;

		//Source指的是被计算的颜色，Destination是已经在屏幕上的颜色。
		//混合结果=Source * factor1 + Destination * factor2
		static int[] Factors = new int[] {
			//Normal
			GS_BLSRC_SRCALPHA,
			GS_BLDST_ONEMINUSSRCALPHA,

			//None
			GS_BLSRC_ONE,
			GS_BLDST_ONE,

			//Add
			GS_BLSRC_SRCALPHA,
			GS_BLDST_ONE,

			//Multiply
			GS_BLSRC_DSTCOL,
			GS_BLDST_ONEMINUSSRCALPHA,

			//Screen
			GS_BLSRC_ONE,
			GS_BLDST_ONEMINUSSRCCOL,

			//Erase
			GS_BLSRC_ZERO,
			GS_BLDST_ONEMINUSSRCALPHA,

			//Mask
			GS_BLSRC_ZERO,
			GS_BLDST_SRCALPHA,

			//Below
			GS_BLSRC_ONEMINUSDSTALPHA,
			GS_BLDST_DSTALPHA,

			//Off
			GS_BLSRC_ONE,
			GS_BLDST_ZERO,

			//Custom1
			GS_BLSRC_SRCALPHA,
			GS_BLDST_ONEMINUSSRCALPHA,

			//Custom2
			GS_BLSRC_SRCALPHA,
			GS_BLDST_ONEMINUSSRCALPHA,

			//Custom3
			GS_BLSRC_SRCALPHA,
			GS_BLDST_ONEMINUSSRCALPHA
		};

		/// <summary>
		/// 
		/// </summary>
		/// <param name="mat"></param>
		/// <param name="blendMode"></param>
		public static void Apply(BlendMode blendMode)
		{
			int index = (int)blendMode * 2;
#if !CE_5_5
			Global.gEnv.pRenderer.SetState(/*GS_NODEPTHTEST*/0x00000200 | Factors[index] | Factors[index + 1]);
#endif
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="blendMode"></param>
		/// <param name="srcFactor"></param>
		/// <param name="dstFactor"></param>
		public static void Override(BlendMode blendMode, int srcFactor, int dstFactor)
		{
			int index = (int)blendMode * 2;
			Factors[index] = srcFactor;
			Factors[index + 1] = dstFactor;
		}
	}
}
