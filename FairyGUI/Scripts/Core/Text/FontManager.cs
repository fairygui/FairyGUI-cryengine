using System.Collections.Generic;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class FontManager
	{
		static Dictionary<string, BaseFont> sFontFactory = new Dictionary<string, BaseFont>();

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		static public BaseFont GetFont(string name)
		{
			if (name.StartsWith("ui://"))
			{
				BitmapFont font = (BitmapFont)UIPackage.GetItemAssetByURL(name);
				if (font != null)
					return font;
			}

			BaseFont ret;
			if (!sFontFactory.TryGetValue(name, out ret))
			{
				ret = new DynamicFont(name);
				sFontFactory.Add(name, ret);
			}

			return ret;
		}
	}
}
