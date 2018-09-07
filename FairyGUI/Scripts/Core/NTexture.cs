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
		public float lastActive;

		Texture _nativeTexture;
		Texture _alphaTexture;
		Rect _region;
		NTexture _root;

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
				NTexture tmp = _empty;
				_empty = null;
				tmp.Dispose();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="texture"></param>
		public NTexture(Texture texture)
		{
			_root = this;
			_nativeTexture = texture;
			uvRect = new Rect(0, 0, 1, 1);
			_region = new Rect(0, 0, texture.Width, texture.Height);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="texture"></param>
		/// /// <param name="alphaTexture"></param>
		/// <param name="xScale"></param>
		/// <param name="yScale"></param>
		public NTexture(Texture texture, Texture alphaTexture, float xScale, float yScale)
		{
			_root = this;
			_nativeTexture = texture;
			_alphaTexture = alphaTexture;
			uvRect = new Rect(0, 0, xScale, yScale);
			_region = new Rect(0, 0, texture.Width, texture.Height);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="texture"></param>
		/// <param name="region"></param>
		public NTexture(Texture texture, Rect region)
		{
			_root = this;
			_nativeTexture = texture;
			_region = region;
			uvRect = new Rect(region.x / _nativeTexture.Width, 1 - (region.y + region.Height) / _nativeTexture.Height,
				region.Width / _nativeTexture.Width, region.Height / _nativeTexture.Height);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="root"></param>
		/// <param name="region"></param>
		public NTexture(NTexture root, Rect region, bool rotated)
		{
			_root = root;
			this.rotated = rotated;
			region.x += root._region.x;
			region.y += root._region.y;
			uvRect = new Rect(region.x * root.uvRect.Width / root.width, 1 - (region.y + region.Height) * root.uvRect.Height / root.height,
				region.Width * root.uvRect.Width / root.width, region.Height * root.uvRect.Height / root.height);
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
			get { return (int)_region.Width; }
		}

		/// <summary>
		/// 
		/// </summary>
		public int height
		{
			get { return (int)_region.Height; }
		}

		/// <summary>
		/// 
		/// </summary>
		public NTexture root
		{
			get { return _root; }
		}

		/// <summary>
		/// 
		/// </summary>
		public bool disposed
		{
			get { return _root == null; }
		}

		/// <summary>
		/// 
		/// </summary>
		public Texture nativeTexture
		{
			get { return _root != null ? _root._nativeTexture : null; }
		}

		/// <summary>
		/// 
		/// </summary>
		public Texture alphaTexture
		{
			get { return _root != null ? _root._alphaTexture : null; }
		}

		public int ID
		{
			get { return _root != null ? _root._nativeTexture.ID : -1; }
		}
		/// <summary>
		/// 
		/// </summary>
		public void Unload()
		{
			if (this == _empty)
				return;

			if (_root != this)
				throw new System.Exception("Unload is not allow to call on none root NTexture.");

			if (_nativeTexture != null)
			{
				_nativeTexture.Destroy();
				_nativeTexture = null;
			}

			if(_alphaTexture!=null)
			{
				_alphaTexture.Destroy();
				_alphaTexture = null;
			}
		}

		public void Dispose()
		{
			if (this == _empty)
				return;

			if (_root == this)
				Unload();
			_root = null;
		}
	}
}
