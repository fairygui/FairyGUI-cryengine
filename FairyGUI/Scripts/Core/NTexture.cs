using CryEngine;
using CryEngine.Resources;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class NTexture
	{
		/// <summary>
		/// 
		/// </summary>
		public Texture nativeTexture;

		/// <summary>
		/// 
		/// </summary>
		public NTexture alphaTexture;

		/// <summary>
		/// 
		/// </summary>
		public NTexture root;

		/// <summary>
		/// 
		/// </summary>
		public Rect uvRect;

		/// <summary>
		/// 
		/// </summary>
		public bool rotated;

		/// <summary>
		/// 
		/// </summary>
		public int refCount;

		/// <summary>
		/// 
		/// </summary>
		public bool disposed;

		/// <summary>
		/// 
		/// </summary>
		public float lastActive;

		/// <summary>
		/// 
		/// </summary>
		public bool storedODisk;

		Rect? _region;

		static Texture CreateEmptyTexture()
		{
			return new Texture(1, 1, new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
				0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF });
		}

		static NTexture _empty;

		/// <summary>
		/// 
		/// </summary>
		public static NTexture Empty
		{
			get
			{
				if (_empty == null)
					_empty = new NTexture(CreateEmptyTexture());

				return _empty;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public static void DisposeEmpty()
		{
			if (_empty != null)
			{
				_empty.Dispose();
				_empty = null;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="texture"></param>
		public NTexture(Texture texture)
		{
			root = this;
			nativeTexture = texture;
			uvRect = new Rect(0, 0, 1, 1);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="texture"></param>
		/// <param name="xScale"></param>
		/// <param name="yScale"></param>
		public NTexture(Texture texture, float xScale, float yScale)
		{
			root = this;
			nativeTexture = texture;
			uvRect = new Rect(0, 0, xScale, yScale);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="texture"></param>
		/// <param name="region"></param>
		public NTexture(Texture texture, Rect region)
		{
			root = this;
			nativeTexture = texture;
			_region = region;
			uvRect = new Rect(region.x / nativeTexture.Width, 1 - (region.y + region.Height) / nativeTexture.Height,
				region.Width / nativeTexture.Width, region.Height / nativeTexture.Height);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="root"></param>
		/// <param name="region"></param>
		public NTexture(NTexture root, Rect region, bool rotated)
		{
			this.root = root;
			nativeTexture = root.nativeTexture;
			this.rotated = rotated;
			if (root._region != null)
			{
				region.x += ((Rect)root._region).x;
				region.y += ((Rect)root._region).y;
			}
			uvRect = new Rect(region.x * root.uvRect.Width / nativeTexture.Width, 1 - (region.y + region.Height) * root.uvRect.Height / nativeTexture.Height,
				region.Width * root.uvRect.Width / nativeTexture.Width, region.Height * root.uvRect.Height / nativeTexture.Height);
			if (rotated)
			{
				float tmp = region.Width;
				region.Width = region.Height;
				region.Height = tmp;

				tmp = uvRect.Width;
				uvRect.Width = uvRect.Height;
				uvRect.Height = tmp;
			}

			_region = region;
		}

		/// <summary>
		/// 
		/// </summary>
		public int width
		{
			get
			{
				if (_region != null)
					return (int)((Rect)_region).Width;
				else
					return nativeTexture.Width;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public int height
		{
			get
			{
				if (_region != null)
					return (int)((Rect)_region).Height;
				else
					return nativeTexture.Height;
			}
		}

		public int ID
		{
			get { return nativeTexture.ID; }
		}

		public void Dispose()
		{
			if (!disposed)
			{
				disposed = true;

				if (root == this && nativeTexture != null)
				{
					nativeTexture.Destroy();
				}
				nativeTexture = null;
				root = null;
			}
		}
	}
}
