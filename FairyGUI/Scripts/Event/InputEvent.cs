using CryEngine;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class InputEvent
	{
		/// <summary>
		/// x position in stage coordinates.
		/// </summary>
		public float x { get; internal set; }

		/// <summary>
		/// y position in stage coordinates.
		/// </summary>
		public float y { get; internal set; }

		/// <summary>
		/// 
		/// </summary>
		public KeyId keyCode { get; internal set; }

		/// <summary>
		/// 
		/// </summary>
		public string KeyName { get; internal set; }

		/// <summary>
		/// 
		/// </summary>
		public InputModifierFlags modifiers { get; internal set; }

		/// <summary>
		/// 
		/// </summary>
		public int mouseWheelDelta { get; internal set; }

		/// <summary>
		/// 
		/// </summary>
		public int touchId { get; internal set; }

		/// <summary>
		/// -1-none,0-left,1-right,2-middle
		/// </summary>
		public int button { get; internal set; }

		internal int clickCount;

		public InputEvent()
		{
			touchId = -1;
			x = 0;
			y = 0;
			clickCount = 0;
			keyCode = KeyId.Unknown;
			KeyName = null;
			modifiers = 0;
			mouseWheelDelta = 0;
		}

		/// <summary>
		/// 
		/// </summary>
		public Vector2 position
		{
			get { return new Vector2(x, y); }
		}

		/// <summary>
		/// 
		/// </summary>
		public bool isDoubleClick
		{
			get { return clickCount > 1; }
		}

		/// <summary>
		/// 
		/// </summary>
		public bool ctrl
		{
			get
			{
				return ((modifiers & InputModifierFlags.LCtrl) != 0) ||
					((modifiers & InputModifierFlags.RCtrl) != 0);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public bool shift
		{
			get
			{
				return ((modifiers & InputModifierFlags.LShift) != 0) ||
					((modifiers & InputModifierFlags.RShift) != 0);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public bool alt
		{
			get
			{
				return ((modifiers & InputModifierFlags.LAlt) != 0) ||
					((modifiers & InputModifierFlags.RAlt) != 0);
			}
		}
	}
}
