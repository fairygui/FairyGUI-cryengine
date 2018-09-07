using CryEngine;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public enum FillMethod
	{
		None = 0,

		/// <summary>
		/// The Image will be filled Horizontally
		/// </summary>
		Horizontal = 1,

		/// <summary>
		/// The Image will be filled Vertically.
		/// </summary>
		Vertical = 2,

		/// <summary>
		/// The Image will be filled Radially with the radial center in one of the corners.
		/// </summary>
		Radial90 = 3,

		/// <summary>
		/// The Image will be filled Radially with the radial center in one of the edges.
		/// </summary>
		Radial180 = 4,

		/// <summary>
		/// The Image will be filled Radially with the radial center at the center.
		/// </summary>
		Radial360 = 5,
	}

	/// <summary>
	/// 
	/// </summary>
	public enum OriginHorizontal
	{
		Left,
		Right,
	}

	/// <summary>
	/// 
	/// </summary>
	public enum OriginVertical
	{
		Top,
		Bottom
	}

	/// <summary>
	/// 
	/// </summary>
	public enum Origin90
	{
		TopLeft,
		TopRight,
		BottomLeft,
		BottomRight
	}

	/// <summary>
	/// 
	/// </summary>
	public enum Origin180
	{
		Top,
		Bottom,
		Left,
		Right
	}

	/// <summary>
	/// 
	/// </summary>
	public enum Origin360
	{
		Top,
		Bottom,
		Left,
		Right
	}
}
