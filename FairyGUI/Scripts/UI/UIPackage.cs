﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using CryEngine;
using CryEngine.Resources;
using FairyGUI.Utils;

namespace FairyGUI
{
	/// <summary>
	/// A UI Package contains a description file and some texture,sound assets.
	/// </summary>
	public class UIPackage
	{
		/// <summary>
		/// Package id. It is generated by the Editor, or set by customId.
		/// </summary>
		public string id { get; private set; }

		/// <summary>
		/// Package name.
		/// </summary>
		public string name { get; private set; }

		/// <summary>
		/// The path relative to the resources folder.
		/// </summary>
		public string assetPath { get; private set; }

		List<PackageItem> _items;
		Dictionary<string, PackageItem> _itemsById;
		Dictionary<string, PackageItem> _itemsByName;
		string _customId;

		class AtlasSprite
		{
			public PackageItem atlas;
			public Rect rect = new Rect();
			public bool rotated;
		}
		Dictionary<string, AtlasSprite> _sprites;

		static Dictionary<string, UIPackage> _packageInstById = new Dictionary<string, UIPackage>();
		static Dictionary<string, UIPackage> _packageInstByName = new Dictionary<string, UIPackage>();
		static List<UIPackage> _packageList = new List<UIPackage>();

		internal static int _constructing;

		public const string URL_PREFIX = "ui://";

		public UIPackage()
		{
			_items = new List<PackageItem>();
			_itemsById = new Dictionary<string, PackageItem>();
			_itemsByName = new Dictionary<string, PackageItem>();
			_sprites = new Dictionary<string, AtlasSprite>();
		}

		/// <summary>
		/// Return a UIPackage with a certain id.
		/// </summary>
		/// <param name="id">ID of the package.</param>
		/// <returns>UIPackage</returns>
		public static UIPackage GetById(string id)
		{
			UIPackage pkg;
			if (_packageInstById.TryGetValue(id, out pkg))
				return pkg;
			else
				return null;
		}

		/// <summary>
		/// Return a UIPackage with a certain name.
		/// </summary>
		/// <param name="name">Name of the package.</param>
		/// <returns>UIPackage</returns>
		public static UIPackage GetByName(string name)
		{
			UIPackage pkg;
			if (_packageInstByName.TryGetValue(name, out pkg))
				return pkg;
			else
				return null;
		}

		/// <summary>
		/// Add a UI package from a path relative to Unity Resources path.
		/// </summary>
		/// <param name="assetPath">Path relative to resources path.</param>
		/// <returns>UIPackage</returns>
		public static UIPackage AddPackage(string assetPath)
		{
			if (_packageInstById.ContainsKey(assetPath))
				return _packageInstById[assetPath];

			byte[] bytes = File.ReadAllBytes(Engine.DataDirectory + assetPath + ".fui");
			if (bytes == null)
				throw new Exception("FairyGUI: Cannot load ui package in '" + assetPath + "'");

			ByteBuffer buffer = new ByteBuffer(bytes);

			UIPackage pkg = new UIPackage();
			pkg.assetPath = assetPath;
			if (!pkg.LoadPackage(buffer, assetPath, Engine.DataDirectory + assetPath))
				return null;

			_packageInstById[pkg.id] = pkg;
			_packageInstByName[pkg.name] = pkg;
			_packageInstById[assetPath] = pkg;
			_packageList.Add(pkg);
			return pkg;
		}

		/// <summary>
		/// Remove a package. All resources in this package will be disposed.
		/// </summary>
		/// <param name="packageIdOrName"></param>
		/// <param name="allowDestroyingAssets"></param>
		public static void RemovePackage(string packageIdOrName)
		{
			UIPackage pkg = null;
			if (!_packageInstById.TryGetValue(packageIdOrName, out pkg))
			{
				if (!_packageInstByName.TryGetValue(packageIdOrName, out pkg))
					throw new Exception("FairyGUI: '" + packageIdOrName + "' is not a valid package id or name.");
			}
			pkg.Dispose();
			_packageInstById.Remove(pkg.id);
			if (pkg._customId != null)
				_packageInstById.Remove(pkg._customId);
			if (pkg.assetPath != null)
				_packageInstById.Remove(pkg.assetPath);
			_packageInstByName.Remove(pkg.name);
			_packageList.Remove(pkg);
		}

		/// <summary>
		/// 
		/// </summary>
		public static void RemoveAllPackages()
		{
			if (_packageInstById.Count > 0)
			{
				UIPackage[] pkgs = _packageList.ToArray();

				foreach (UIPackage pkg in pkgs)
				{
					pkg.Dispose();
				}
			}
			_packageList.Clear();
			_packageInstById.Clear();
			_packageInstByName.Clear();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public static List<UIPackage> GetPackages()
		{
			return _packageList;
		}

		/// <summary>
		/// Create a UI object.
		/// </summary>
		/// <param name="pkgName">Package name.</param>
		/// <param name="resName">Resource name.</param>
		/// <returns>A UI object.</returns>
		public static GObject CreateObject(string pkgName, string resName)
		{
			UIPackage pkg = GetByName(pkgName);
			if (pkg != null)
				return pkg.CreateObject(resName);
			else
				return null;
		}

		/// <summary>
		///  Create a UI object.
		/// </summary>
		/// <param name="pkgName">Package name.</param>
		/// <param name="resName">Resource name.</param>
		/// <param name="userClass">Custom implementation of this object.</param>
		/// <returns>A UI object.</returns>
		public static GObject CreateObject(string pkgName, string resName, System.Type userClass)
		{
			UIPackage pkg = GetByName(pkgName);
			if (pkg != null)
				return pkg.CreateObject(resName, userClass);
			else
				return null;
		}

		/// <summary>
		/// Create a UI object.
		/// </summary>
		/// <param name="url">Resource url.</param>
		/// <returns>A UI object.</returns>
		public static GObject CreateObjectFromURL(string url)
		{
			PackageItem pi = GetItemByURL(url);
			if (pi != null)
				return pi.owner.CreateObject(pi, null);
			else
				return null;
		}

		/// <summary>
		/// Create a UI object.
		/// </summary>
		/// <param name="url">Resource url.</param>
		/// <param name="userClass">Custom implementation of this object.</param>
		/// <returns>A UI object.</returns>
		public static GObject CreateObjectFromURL(string url, System.Type userClass)
		{
			PackageItem pi = GetItemByURL(url);
			if (pi != null)
				return pi.owner.CreateObject(pi, userClass);
			else
				return null;
		}

		/// <summary>
		/// Get a asset with a certain name.
		/// </summary>
		/// <param name="pkgName">Package name.</param>
		/// <param name="resName">Resource name.</param>
		/// <returns>If resource is atlas, returns NTexture; If resource is sound, returns AudioClip.</returns>
		public static object GetItemAsset(string pkgName, string resName)
		{
			UIPackage pkg = GetByName(pkgName);
			if (pkg != null)
				return pkg.GetItemAsset(resName);
			else
				return null;
		}

		/// <summary>
		/// Get a asset with a certain name.
		/// </summary>
		/// <param name="url">Resource url.</param>
		/// <returns>If resource is atlas, returns NTexture; If resource is sound, returns AudioClip.</returns>
		public static object GetItemAssetByURL(string url)
		{
			PackageItem item = GetItemByURL(url);
			if (item == null)
				return null;

			return item.owner.GetItemAsset(item);
		}

		/// <summary>
		/// Get url of an item in package.
		/// </summary>
		/// <param name="pkgName">Package name.</param>
		/// <param name="resName">Resource name.</param>
		/// <returns>Url.</returns>
		public static string GetItemURL(string pkgName, string resName)
		{
			UIPackage pkg = GetByName(pkgName);
			if (pkg == null)
				return null;

			PackageItem pi;
			if (!pkg._itemsByName.TryGetValue(resName, out pi))
				return null;

			return URL_PREFIX + pkg.id + pi.id;
		}

		public static PackageItem GetItemByURL(string url)
		{
			if (url == null)
				return null;

			int pos1 = url.IndexOf("//");
			if (pos1 == -1)
				return null;

			int pos2 = url.IndexOf('/', pos1 + 2);
			if (pos2 == -1)
			{
				if (url.Length > 13)
				{
					string pkgId = url.Substring(5, 8);
					UIPackage pkg = GetById(pkgId);
					if (pkg != null)
					{
						string srcId = url.Substring(13);
						return pkg.GetItem(srcId);
					}
				}
			}
			else
			{
				string pkgName = url.Substring(pos1 + 2, pos2 - pos1 - 2);
				UIPackage pkg = GetByName(pkgName);
				if (pkg != null)
				{
					string srcName = url.Substring(pos2 + 1);
					return pkg.GetItemByName(srcName);
				}
			}

			return null;
		}

		/// <summary>
		/// 将'ui://包名/组件名'转换为以内部id表达的url格式。如果传入的url本身就是内部id格式，则直接返回。
		/// 同时这个方法还带格式检测，如果传入不正确的url，会返回null。
		/// </summary>
		/// <param name="url"></param>
		/// <returns></returns>
		public static string NormalizeURL(string url)
		{
			if (url == null)
				return null;

			int pos1 = url.IndexOf("//");
			if (pos1 == -1)
				return null;

			int pos2 = url.IndexOf('/', pos1 + 2);
			if (pos2 == -1)
				return url;
			else
			{
				string pkgName = url.Substring(pos1 + 2, pos2 - pos1 - 2);
				string srcName = url.Substring(pos2 + 1);
				return GetItemURL(pkgName, srcName);
			}
		}

		/// <summary>
		/// Set strings source.
		/// </summary>
		/// <param name="source"></param>
		public static void SetStringsSource(XML source)
		{
			TranslationHelper.LoadFromXML(source);
		}

		/// <summary>
		/// Set a custom id for package, then you can use it in GetById.
		/// </summary>
		public string customId
		{
			get { return _customId; }
			set
			{
				if (_customId != null)
					_packageInstById.Remove(_customId);
				_customId = value;
				if (_customId != null)
					_packageInstById[_customId] = this;
			}
		}


		bool LoadPackage(ByteBuffer buffer, string packageSource, string assetNamePrefix)
		{
			if (buffer.ReadUint() != 0x46475549)
				throw new Exception("FairyGUI: old package format found in '" + packageSource + "'");

			buffer.version = buffer.ReadInt();
			buffer.ReadBool(); //compressed
			id = buffer.ReadString();
			name = buffer.ReadString();
			if (_packageInstById.ContainsKey(id) && name != _packageInstById[id].name)
			{
				Log.Warning("FairyGUI: Package id conflicts, '" + name + "' and '" + _packageInstById[id].name + "'");
				return false;
			}
			buffer.Skip(20);
			int indexTablePos = buffer.position;
			int cnt;

			buffer.Seek(indexTablePos, 4);

			cnt = buffer.ReadInt();
			string[] stringTable = new string[cnt];
			for (int i = 0; i < cnt; i++)
				stringTable[i] = buffer.ReadString();
			buffer.stringTable = stringTable;

			buffer.Seek(indexTablePos, 1);

			PackageItem pi;

			if (assetNamePrefix == null)
				assetNamePrefix = string.Empty;
			else if (assetNamePrefix.Length > 0)
				assetNamePrefix = assetNamePrefix + "_";

			cnt = buffer.ReadShort();
			for (int i = 0; i < cnt; i++)
			{
				int nextPos = buffer.ReadInt();
				nextPos += buffer.position;

				pi = new PackageItem();
				pi.owner = this;
				pi.type = (PackageItemType)buffer.ReadByte();
				pi.id = buffer.ReadS();
				pi.name = buffer.ReadS();
				buffer.ReadS(); //path
				pi.file = buffer.ReadS();
				pi.exported = buffer.ReadBool();
				pi.width = buffer.ReadInt();
				pi.height = buffer.ReadInt();

				switch (pi.type)
				{
					case PackageItemType.Image:
						{
							pi.objectType = ObjectType.Image;
							int scaleOption = buffer.ReadByte();
							if (scaleOption == 1)
							{
								Rect rect = new Rect();
								rect.x = buffer.ReadInt();
								rect.y = buffer.ReadInt();
								rect.Width = buffer.ReadInt();
								rect.Height = buffer.ReadInt();
								pi.scale9Grid = rect;

								pi.tileGridIndice = buffer.ReadInt();
							}
							else if (scaleOption == 2)
								pi.scaleByTile = true;

							buffer.ReadBool(); //smoothing
							break;
						}

					case PackageItemType.MovieClip:
						{
							buffer.ReadBool(); //smoothing
							pi.objectType = ObjectType.MovieClip;
							pi.rawData = buffer.ReadBuffer();
							break;
						}

					case PackageItemType.Font:
						{
							pi.rawData = buffer.ReadBuffer();
							break;
						}

					case PackageItemType.Component:
						{
							int extension = buffer.ReadByte();
							if (extension > 0)
								pi.objectType = (ObjectType)extension;
							else
								pi.objectType = ObjectType.Component;
							pi.rawData = buffer.ReadBuffer();

							UIObjectFactory.ResolvePackageItemExtension(pi);
							break;
						}

					case PackageItemType.Atlas:
					case PackageItemType.Sound:
					case PackageItemType.Misc:
						{
							pi.file = assetNamePrefix + pi.file;
							break;
						}
				}
				_items.Add(pi);
				_itemsById[pi.id] = pi;
				if (pi.name != null)
					_itemsByName[pi.name] = pi;

				buffer.position = nextPos;
			}

			buffer.Seek(indexTablePos, 2);

			cnt = buffer.ReadShort();
			for (int i = 0; i < cnt; i++)
			{
				int nextPos = buffer.ReadShort();
				nextPos += buffer.position;

				string itemId = buffer.ReadS();
				pi = _itemsById[buffer.ReadS()];

				AtlasSprite sprite = new AtlasSprite();
				sprite.atlas = pi;
				sprite.rect.x = buffer.ReadInt();
				sprite.rect.y = buffer.ReadInt();
				sprite.rect.Width = buffer.ReadInt();
				sprite.rect.Height = buffer.ReadInt();
				sprite.rotated = buffer.ReadBool();
				_sprites[itemId] = sprite;

				buffer.position = nextPos;
			}

			if (buffer.Seek(indexTablePos, 3))
			{
				cnt = buffer.ReadShort();
				for (int i = 0; i < cnt; i++)
				{
					int nextPos = buffer.ReadInt();
					nextPos += buffer.position;

					if (_itemsById.TryGetValue(buffer.ReadS(), out pi))
					{
						if (pi.type == PackageItemType.Image)
						{
							pi.pixelHitTestData = new PixelHitTestData();
							pi.pixelHitTestData.Load(buffer);
						}
					}

					buffer.position = nextPos;
				}
			}

			return true;
		}

		static int ComparePackageItem(PackageItem p1, PackageItem p2)
		{
			if (p1.name != null && p2.name != null)
				return p1.name.CompareTo(p2.name);
			else
				return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		public void LoadAllAssets()
		{
			int cnt = _items.Count;
			for (int i = 0; i < cnt; i++)
				GetItemAsset(_items[i]);
		}

		void Dispose()
		{
			int cnt = _items.Count;
			for (int i = 0; i < cnt; i++)
			{
				PackageItem pi = _items[i];
				if (pi.type == PackageItemType.Atlas)
				{
					if (pi.texture != null)
					{
						pi.texture.Dispose();
						pi.texture = null;
					}
				}
				else if (pi.type == PackageItemType.Sound)
				{
					if (pi.audioClip != null)
					{
						pi.audioClip.Dispose();
						pi.audioClip = null;
					}
				}
			}
			_items.Clear();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="resName"></param>
		/// <returns></returns>
		public GObject CreateObject(string resName)
		{
			PackageItem pi;
			if (!_itemsByName.TryGetValue(resName, out pi))
				return null;

			return CreateObject(pi, null);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="resName"></param>
		/// <param name="userClass"></param>
		/// <returns></returns>
		public GObject CreateObject(string resName, System.Type userClass)
		{
			PackageItem pi;
			if (!_itemsByName.TryGetValue(resName, out pi))
				return null;

			return CreateObject(pi, userClass);
		}

		GObject CreateObject(PackageItem item, System.Type userClass)
		{
			Stats.LatestObjectCreation = 0;
			Stats.LatestGraphicsCreation = 0;

			GetItemAsset(item);

			GObject g = null;
			if (item.type == PackageItemType.Component)
			{
				if (userClass != null)
					g = (GComponent)Activator.CreateInstance(userClass);
				else
					g = UIObjectFactory.NewObject(item);
			}
			else
				g = UIObjectFactory.NewObject(item);

			if (g == null)
				return null;

			_constructing++;
			g.packageItem = item;
			g.ConstructFromResource();
			_constructing--;
			return g;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="resName"></param>
		/// <returns></returns>
		public object GetItemAsset(string resName)
		{
			PackageItem pi;
			if (!_itemsByName.TryGetValue(resName, out pi))
				return null;

			return GetItemAsset(pi);
		}

		public List<PackageItem> GetItems()
		{
			return _items;
		}

		public PackageItem GetItem(string itemId)
		{
			PackageItem pi;
			if (_itemsById.TryGetValue(itemId, out pi))
				return pi;
			else
				return null;
		}

		public PackageItem GetItemByName(string itemName)
		{
			PackageItem pi;
			if (_itemsByName.TryGetValue(itemName, out pi))
				return pi;
			else
				return null;
		}

		public object GetItemAsset(PackageItem item)
		{
			switch (item.type)
			{
				case PackageItemType.Image:
					if (item.texture == null)
						LoadImage(item);
					return item.texture;

				case PackageItemType.Atlas:
					if (item.texture == null)
						LoadAtlas(item);
					return item.texture;

				case PackageItemType.Sound:
					if (item.audioClip == null)
						LoadSound(item);
					return item.audioClip;

				case PackageItemType.Font:
					if (item.bitmapFont == null)
						LoadFont(item);

					return item.bitmapFont;

				case PackageItemType.MovieClip:
					if (item.frames == null)
						LoadMovieClip(item);

					return item.frames;

				case PackageItemType.Component:
					return item.rawData;

				case PackageItemType.Misc:
					return LoadBinary(item);

				default:
					return null;
			}
		}

		void LoadAtlas(PackageItem item)
		{
			try
			{
				Bitmap bm = new Bitmap(item.file);
				Texture tex = new Texture(item.width, item.height, bm.GetPixels());
				bm.Dispose();
				item.texture = new NTexture(tex, null, (float)tex.Width / item.width, (float)tex.Height / item.height);
			}
			catch (Exception)
			{
				Log.Warning("FairyGUI: texture '" + item.file + "' not found in " + this.name);
				item.texture = NTexture.Empty;
			}
		}

		void LoadImage(PackageItem item)
		{
			AtlasSprite sprite;
			if (_sprites.TryGetValue(item.id, out sprite))
				item.texture = new NTexture((NTexture)GetItemAsset(sprite.atlas), sprite.rect, sprite.rotated);
			else
				item.texture = NTexture.Empty;
		}

		void LoadSound(PackageItem item)
		{
			string ext = Path.GetExtension(item.file);
			string fileName = item.file.Substring(0, item.file.Length - ext.Length);

			item.audioClip = new AudioFile(fileName);
		}

		byte[] LoadBinary(PackageItem item)
		{
			return File.ReadAllBytes(item.file);
		}

		void LoadMovieClip(PackageItem item)
		{
			ByteBuffer buffer = item.rawData;

			buffer.Seek(0, 0);

			item.interval = buffer.ReadInt() / 1000f;
			item.swing = buffer.ReadBool();
			item.repeatDelay = buffer.ReadInt() / 1000f;

			buffer.Seek(0, 1);

			int frameCount = buffer.ReadShort();
			item.frames = new MovieClip.Frame[frameCount];

			string spriteId;
			MovieClip.Frame frame;
			AtlasSprite sprite;

			for (int i = 0; i < frameCount; i++)
			{
				int nextPos = buffer.ReadShort();
				nextPos += buffer.position;

				frame = new MovieClip.Frame();
				frame.rect.x = buffer.ReadInt();
				frame.rect.y = buffer.ReadInt();
				frame.rect.Width = buffer.ReadInt();
				frame.rect.Height = buffer.ReadInt();
				frame.addDelay = buffer.ReadInt() / 1000f;
				spriteId = buffer.ReadS();

				if (spriteId != null && _sprites.TryGetValue(spriteId, out sprite))
				{
					if (item.texture == null)
						item.texture = (NTexture)GetItemAsset(sprite.atlas);
					frame.uvRect = new Rect(sprite.rect.x / item.texture.width * item.texture.uvRect.Width,
						1 - (sprite.rect.y + sprite.rect.Height) * item.texture.uvRect.Height / item.texture.height,
						sprite.rect.Width * item.texture.uvRect.Width / item.texture.width,
						sprite.rect.Height * item.texture.uvRect.Height / item.texture.height);
					frame.rotated = sprite.rotated;
					if (frame.rotated)
					{
						float tmp = frame.uvRect.Width;
						frame.uvRect.Width = frame.uvRect.Height;
						frame.uvRect.Height = tmp;
					}
				}
				item.frames[i] = frame;

				buffer.position = nextPos;
			}
		}

		void LoadFont(PackageItem item)
		{
			BitmapFont font = new BitmapFont(item);
			item.bitmapFont = font;
			ByteBuffer buffer = item.rawData;

			buffer.Seek(0, 0);

			bool ttf = buffer.ReadBool();
			font.colorEnabled = buffer.ReadBool();
			font.scaleEnabled = buffer.ReadBool();
			buffer.ReadBool();//hasChannel
			int fontSize = buffer.ReadInt();
			int xadvance = buffer.ReadInt();
			int lineHeight = buffer.ReadInt();

			float texScaleX = 1;
			float texScaleY = 1;
			NTexture mainTexture = null;
			AtlasSprite mainSprite = null;
			if (ttf && _sprites.TryGetValue(item.id, out mainSprite))
			{
				mainTexture = (NTexture)GetItemAsset(mainSprite.atlas);
				texScaleX = mainTexture.root.uvRect.Width / mainTexture.width;
				texScaleY = mainTexture.root.uvRect.Height / mainTexture.height;
			}

			buffer.Seek(0, 1);

			BitmapFont.BMGlyph bg;
			int cnt = buffer.ReadInt();
			for (int i = 0; i < cnt; i++)
			{
				int nextPos = buffer.ReadShort();
				nextPos += buffer.position;

				bg = new BitmapFont.BMGlyph();
				char ch = buffer.ReadChar();
				font.AddChar(ch, bg);

				string img = buffer.ReadS();
				int bx = buffer.ReadInt();
				int by = buffer.ReadInt();
				bg.offsetX = buffer.ReadInt();
				bg.offsetY = buffer.ReadInt();
				bg.width = buffer.ReadInt();
				bg.height = buffer.ReadInt();
				bg.advance = buffer.ReadInt();
				bg.channel = buffer.ReadByte();
				if (bg.channel == 1)
					bg.channel = 3;
				else if (bg.channel == 2)
					bg.channel = 2;
				else if (bg.channel == 3)
					bg.channel = 1;

				if (ttf)
				{
					if (mainSprite.rotated)
					{
						float texBaseY = 1 - (float)(mainSprite.rect.y + mainSprite.rect.Height - bx) * texScaleY;
						bg.uv[0] = new Vector2((float)(by + bg.height + mainSprite.rect.x) * texScaleX,
							texBaseY);
						bg.uv[1] = new Vector2(bg.uv[0].x - (float)bg.height * texScaleX, texBaseY);
						bg.uv[2] = new Vector2(bg.uv[1].x, texBaseY + (float)bg.width * texScaleY);
						bg.uv[3] = new Vector2(bg.uv[0].x, texBaseY + (float)bg.width * texScaleY);
					}
					else
					{
						float texBaseY = 1 - (float)(by + bg.height + mainSprite.rect.y) * texScaleY;
						bg.uv[0] = new Vector2((float)(bx + mainSprite.rect.x) * texScaleX,
							texBaseY + (float)bg.height * texScaleY);
						bg.uv[1] = new Vector2(bg.uv[0].x, texBaseY + (float)bg.height * texScaleY);
						bg.uv[2] = new Vector2(bg.uv[0].x + (float)bg.width * texScaleX, texBaseY);
						bg.uv[3] = new Vector2(bg.uv[2].x, texBaseY);
					}

					bg.lineHeight = lineHeight;
				}
				else
				{
					PackageItem charImg;
					if (_itemsById.TryGetValue(img, out charImg))
					{
						GetItemAsset(charImg);
						Rect uvRect = charImg.texture.uvRect;
						bg.uv[0] = new Vector2(uvRect.x, uvRect.y + uvRect.Height);
						bg.uv[1] = new Vector2(uvRect.x + uvRect.Width, uvRect.y + uvRect.Height);
						bg.uv[2] = new Vector2(uvRect.x + uvRect.Width, uvRect.y);
						bg.uv[3] = new Vector2(uvRect.x, uvRect.y);
						if (charImg.texture.rotated)
							NGraphics.RotateUV(bg.uv, ref uvRect);
						bg.width = charImg.texture.width;
						bg.height = charImg.texture.height;

						if (mainTexture == null)
							mainTexture = charImg.texture.root;
					}

					if (fontSize == 0)
						fontSize = bg.height;

					if (bg.advance == 0)
					{
						if (xadvance == 0)
							bg.advance = bg.offsetX + bg.width;
						else
							bg.advance = xadvance;
					}

					bg.lineHeight = bg.offsetY < 0 ? bg.height : (bg.offsetY + bg.height);
					if (bg.lineHeight < font.size)
						bg.lineHeight = font.size;
				}

				buffer.position = nextPos;
			}

			font.size = fontSize;
			font.mainTexture = mainTexture;
		}
	}
}
