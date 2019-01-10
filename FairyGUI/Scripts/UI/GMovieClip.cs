﻿using CryEngine;
using FairyGUI.Utils;

namespace FairyGUI
{
	/// <summary>
	/// GMovieClip class.
	/// </summary>
	public class GMovieClip : GObject, IAnimationGear, IColorGear
	{
		MovieClip _content;
		EventListener _onPlayEnd;

		public GMovieClip()
		{
			_sizeImplType = 1;
		}

		/// <summary>
		/// 
		/// </summary>
		public EventListener onPlayEnd
		{
			get { return _onPlayEnd ?? (_onPlayEnd = new EventListener(this, "onPlayEnd")); }
		}

		override protected void CreateDisplayObject()
		{
			_content = new MovieClip();
			_content.gOwner = this;
			_content.ignoreEngineTimeScale = true;
			displayObject = _content;
		}

		/// <summary>
		/// 
		/// </summary>
		public bool playing
		{
			get { return _content.playing; }
			set
			{
				_content.playing = value;
				UpdateGear(5);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public int frame
		{
			get { return _content.frame; }
			set
			{
				_content.frame = value;
				UpdateGear(5);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public Color color
		{
			get { return _content.color; }
			set
			{
				_content.color = value;
				UpdateGear(4);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public FlipType flip
		{
			get { return _content.flip; }
			set { _content.flip = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public float timeScale
		{
			get { return _content.timeScale; }
			set { _content.timeScale = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public bool ignoreEngineTimeScale
		{
			get { return _content.ignoreEngineTimeScale; }
			set { _content.ignoreEngineTimeScale = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public void Rewind()
		{
			_content.Rewind();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="anotherMc"></param>
		public void SyncStatus(GMovieClip anotherMc)
		{
			_content.SyncStatus(anotherMc._content);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="time"></param>
		public void Advance(float time)
		{
			_content.Advance(time);
		}

		/// <summary>
		/// Play from the start to end, repeat times, set to endAt on complete.
		/// 从start帧开始，播放到end帧（-1表示结尾），重复times次（0表示无限循环），循环结束后，停止在endAt帧（-1表示参数end）
		/// </summary>
		/// <param name="start">Start frame</param>
		/// <param name="end">End frame. -1 indicates the last frame.</param>
		/// <param name="times">Repeat times. 0 indicates infinite loop.</param>
		/// <param name="endAt">Stop frame. -1 indicates to equal to the end parameter.</param>
		public void SetPlaySettings(int start, int end, int times, int endAt)
		{
			((MovieClip)displayObject).SetPlaySettings(start, end, times, endAt);
		}

		override public void ConstructFromResource()
		{
			packageItem.Load();

			sourceWidth = packageItem.width;
			sourceHeight = packageItem.height;
			initWidth = sourceWidth;
			initHeight = sourceHeight;

			_content.interval = packageItem.interval;
			_content.swing = packageItem.swing;
			_content.repeatDelay = packageItem.repeatDelay;
			_content.SetData(packageItem.texture, packageItem.frames, new Rect(0, 0, sourceWidth, sourceHeight));

			SetSize(sourceWidth, sourceHeight);
		}

		override public void Setup_BeforeAdd(ByteBuffer buffer, int beginPos)
		{
			base.Setup_BeforeAdd(buffer, beginPos);

			buffer.Seek(beginPos, 5);

			if (buffer.ReadBool())
				_content.color = buffer.ReadColor();
			_content.flip = (FlipType)buffer.ReadByte();
			_content.frame = buffer.ReadInt();
			_content.playing = buffer.ReadBool();
		}
	}
}
