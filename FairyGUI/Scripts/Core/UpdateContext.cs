using System.Collections.Generic;
using CryEngine;
using CryEngine.Common;
using FairyGUI.Utils;

namespace FairyGUI
{
	/// <summary>
	/// UpdateContext is for internal use.
	/// </summary>
	public class UpdateContext
	{
		public struct ClipInfo
		{
			public Rect rect;
			public uint clipId;
		}

		Stack<ClipInfo> _clipStack;

		public bool clipped;
		public ClipInfo clipInfo;

		public float alpha;
		public bool grayed;
		public BlendMode blendMode;

		List<RenderTarget> _renderTargets;
		int _renderTargetCount;

		public static UpdateContext current;

		public UpdateContext()
		{
			_clipStack = new Stack<ClipInfo>();
			_renderTargets = new List<RenderTarget>();
		}

		/// <summary>
		/// 
		/// </summary>
		public void Begin()
		{
			current = this;

			alpha = 1;
			grayed = false;
			blendMode = BlendMode.Normal;

			clipped = false;
			_clipStack.Clear();

			_renderTargetCount = 0;

			Stats.ObjectCount = 0;
			Stats.GraphicsCount = 0;

#if !CE_5_5
			Global.gEnv.pRenderer.SetViewport(0, 0, (int)Stage.inst.width, (int)Stage.inst.height);
#endif
		}

		/// <summary>
		/// 
		/// </summary>
		public void End()
		{
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="clipId"></param>
		/// <param name="clipRect"></param>
		/// <param name="softness"></param>
		public void EnterClipping(uint clipId, Rect clipRect)
		{
			_clipStack.Push(clipInfo);

			if (clipped)
				clipRect = ToolSet.Intersection(ref clipInfo.rect, ref clipRect);

			clipped = true;
			clipInfo.rect = clipRect;
			clipInfo.clipId = clipId;
			SetScissor(clipInfo.rect);
		}

		/// <summary>
		/// 
		/// </summary>
		public void LeaveClipping()
		{
			clipInfo = _clipStack.Pop();
			clipped = _clipStack.Count > 0;
			if (clipped)
				SetScissor(clipInfo.rect);
			else
				SetScissor(new Rect());
		}

		void SetScissor(Rect rect)
		{
#if CE_5_5

#else
			Global.gEnv.pRenderer.SetScissor((int)(rect.x), (int)(rect.y), (int)(rect.Width), (int)(rect.Height));
#endif
		}

		public void SkipMask(bool value)
		{
			if (!clipped)
				return;

			if (value)
			{
				if (_clipStack.Count == 1)
					SetScissor(new Rect());
				else if (_clipStack.Count > 0)
				{
					ClipInfo last = _clipStack.Peek();
					SetScissor(last.rect);
				}
			}
			else
			{
				SetScissor(clipInfo.rect);
			}
		}

		public void PushRenderTarget(NTexture texture, Vector2 origin)
		{
			if (_renderTargetCount >= _renderTargets.Count)
				_renderTargets.Add(new RenderTarget());

			RenderTarget rt = _renderTargets[_renderTargetCount];
			rt.texture = texture;
			rt.origin = origin;

			_renderTargetCount++;
		}

		public RenderTarget renderTarget
		{
			get
			{
				if (_renderTargetCount > 0)
					return _renderTargets[_renderTargetCount - 1];
				else
					return null;
			}
		}

		public void PopRenderTarget()
		{
			_renderTargetCount--;
		}
	}
}
