using System;
using CryEngine;
using CryEngine.Common;
using CryEngine.Resources;
using FairyGUI.Utils;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class DisplayObject : EventDispatcher
	{
		/// <summary>
		/// 
		/// </summary>
		public string name;

		/// <summary>
		/// 
		/// </summary>
		public Container parent { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public NGraphics graphics { get; protected set; }

		/// <summary>
		/// 
		/// </summary>
		public NGraphics paintingGraphics { get; protected set; }

		/// <summary>
		/// 
		/// </summary>
		public GObject gOwner;

		/// <summary>
		/// 
		/// </summary>
		public uint id;

		/// <summary>
		/// 
		/// </summary>
		public EventListener onClick { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public EventListener onRightClick { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public EventListener onTouchBegin { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public EventListener onTouchMove { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public EventListener onTouchEnd { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public EventListener onRollOver { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public EventListener onRollOut { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public EventListener onMouseWheel { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public EventListener onAddedToStage { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public EventListener onRemovedFromStage { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public EventListener onKeyDown { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public EventListener onClickLink { get; private set; }

		protected Vector3 _rotation;
		protected Vector3 _position;
		protected Vector2 _scale;

		bool _visible;
		bool _touchable;
		Vector2 _pivot;
		Vector3 _pivotOffset;
		float _alpha;
		bool _grayed;
		IFilter _filter;
		BlendMode _blendMode;

		protected int _paintingMode; //1-滤镜，2-blendMode，4-transformMatrix, 8-cacheAsBitmap
		protected Margin _paintingMargin;
		protected int _paintingFlag;
		protected IMaterial _paintingMaterial;
		protected bool _cacheAsBitmap;

		protected Rect _contentRect;
		protected bool _requireUpdateMesh;
		protected bool _outlineChanged;

		protected Matrix4x4 _localToWorldMatrix;
		protected Rect _renderRect;
		protected Vector2 _renderScale;
		protected float _renderRotation;

		private uint _matrixVersion;
		private uint _parentMatrixVersion;

		internal bool _disposed;
		internal protected bool _touchDisabled;

		internal static uint _gInstanceCounter;

		public DisplayObject()
		{
			_alpha = 1;
			_visible = true;
			_touchable = true;
			id = _gInstanceCounter++;
			_scale = new Vector2(1, 1);
			_blendMode = BlendMode.Normal;

			_renderScale = new Vector2(1, 1);
			_matrixVersion = _parentMatrixVersion = 0;
			_localToWorldMatrix.SetIdentity();
			_outlineChanged = true;

			onClick = new EventListener(this, "onClick");
			onRightClick = new EventListener(this, "onRightClick");
			onTouchBegin = new EventListener(this, "onTouchBegin");
			onTouchMove = new EventListener(this, "onTouchMove");
			onTouchEnd = new EventListener(this, "onTouchEnd");
			onRollOver = new EventListener(this, "onRollOver");
			onRollOut = new EventListener(this, "onRollOut");
			onMouseWheel = new EventListener(this, "onMouseWheel");
			onAddedToStage = new EventListener(this, "onAddedToStage");
			onRemovedFromStage = new EventListener(this, "onRemovedFromStage");
			onKeyDown = new EventListener(this, "onKeyDown");
			onClickLink = new EventListener(this, "onClickLink");
		}

		/// <summary>
		/// 
		/// </summary>
		public float alpha
		{
			get { return _alpha; }
			set { _alpha = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public bool grayed
		{
			get { return _grayed; }
			set { _grayed = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public bool visible
		{
			get { return _visible; }
			set { _visible = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public float x
		{
			get { return _position.x; }
			set
			{
				_position.x = value;
				_outlineChanged = true;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public float y
		{
			get { return _position.y; }
			set
			{
				_position.y = value;
				_outlineChanged = true;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public float z
		{
			get { return _position.z; }
			set
			{
				_position.z = value;
				_outlineChanged = true;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public Vector3 position
		{
			get { return _position; }
			set { SetPosition(value.x, value.y, value.z); }
		}

		/// <summary>
		/// 
		/// </summary>
		public Vector2 xy
		{
			get { return new Vector2(_position.x, _position.y); }
			set { SetPosition(value.x, value.y); }
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="xv"></param>
		/// <param name="yv"></param>
		/// <param name="zv"></param>
		public void SetPosition(float xv, float yv, float zv)
		{
			_position.x = xv;
			_position.y = yv;
			_position.z = zv;
			_outlineChanged = true;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="xv"></param>
		/// <param name="yv"></param>
		public void SetPosition(float xv, float yv)
		{
			_position.x = xv;
			_position.y = yv;
			_outlineChanged = true;
		}

		/// <summary>
		/// 
		/// </summary>
		public float width
		{
			get
			{
				EnsureSizeCorrect();
				return _contentRect.Width;
			}
			set
			{
				if (!MathHelpers.Approximately(value, _contentRect.Width))
				{
					_contentRect.Width = value;
					OnSizeChanged(true, false);
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public float height
		{
			get
			{
				EnsureSizeCorrect();
				return _contentRect.Height;
			}
			set
			{
				if (!MathHelpers.Approximately(value, _contentRect.Height))
				{
					_contentRect.Height = value;
					OnSizeChanged(false, true);
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public Vector2 size
		{
			get
			{
				EnsureSizeCorrect();
				return new Vector2(_contentRect.Width, _contentRect.Height);
			}
			set
			{
				SetSize(value.x, value.y);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="wv"></param>
		/// <param name="hv"></param>
		public void SetSize(float wv, float hv)
		{
			bool wc = !MathHelpers.Approximately(wv, _contentRect.Width);
			bool hc = !MathHelpers.Approximately(hv, _contentRect.Height);

			if (wc || hc)
			{
				_contentRect.Width = wv;
				_contentRect.Height = hv;
				OnSizeChanged(wc, hc);
			}
		}

		virtual public void EnsureSizeCorrect()
		{
		}

		virtual protected void OnSizeChanged(bool widthChanged, bool heightChanged)
		{
			ApplyPivot();
			_paintingFlag = 1;
			if (graphics != null)
				_requireUpdateMesh = true;
			_outlineChanged = true;
		}

		/// <summary>
		/// 
		/// </summary>
		public float scaleX
		{
			get { return _scale.x; }
			set
			{
				_scale.x = ValidateScale(value);
				_outlineChanged = true;
				ApplyPivot();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public float scaleY
		{
			get { return _scale.y; }
			set
			{
				_scale.y = ValidateScale(value);
				_outlineChanged = true;
				ApplyPivot();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="xv"></param>
		/// <param name="yv"></param>
		public void SetScale(float xv, float yv)
		{
			_scale.x = ValidateScale(xv);
			_scale.y = ValidateScale(yv);
			_outlineChanged = true;
			ApplyPivot();
		}

		/// <summary>
		/// 在scale过小情况（极端情况=0），当使用Transform的坐标变换时，变换到世界，再从世界变换到本地，会由于精度问题造成结果错误。
		/// 这种错误会导致Batching错误，因为Batching会使用缓存的outline。
		/// 这里限制一下scale的最小值作为当前解决方案。
		/// 这个方案并不完美，因为限制了本地scale值并不能保证对世界scale不会过小。
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		private float ValidateScale(float value)
		{
			if (value >= 0 && value < 0.001f)
				value = 0.001f;
			else if (value < 0 && value > -0.001f)
				value = -0.001f;
			return value;
		}

		/// <summary>
		/// 
		/// </summary>
		public Vector2 scale
		{
			get { return _scale; }
			set
			{
				SetScale(value.x, value.y);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public float rotation
		{
			get
			{
				return _rotation.z;
			}
			set
			{
				_rotation.z = value;
				_outlineChanged = true;
				ApplyPivot();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public float rotationX
		{
			get
			{
				return _rotation.x;
			}
			set
			{
				_rotation.x = value;
				_outlineChanged = true;
				ApplyPivot();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public float rotationY
		{
			get
			{
				return _rotation.y;
			}
			set
			{
				_rotation.y = value;
				_outlineChanged = true;
				ApplyPivot();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public Vector2 pivot
		{
			get { return _pivot; }
			set
			{
				Vector3 deltaPivot = new Vector3((value.x - _pivot.x) * _contentRect.Width, (value.y - _pivot.y) * _contentRect.Height, 0);
				Vector3 oldOffset = _pivotOffset;

				_pivot = value;
				UpdatePivotOffset();
				_position += oldOffset - _pivotOffset + deltaPivot;
				_outlineChanged = true;
			}
		}

		void UpdatePivotOffset()
		{
			float px = _pivot.x * _contentRect.Width;
			float py = _pivot.y * _contentRect.Height;

			Matrix4x4 matrix = MatrixHelper.TRS(Vector3.Zero, _rotation, new Vector3(_scale.x, _scale.y, 1));
			_pivotOffset = matrix.TransformPoint(new Vector3(px, py, 0));
		}

		void ApplyPivot()
		{
			if (_pivot.x != 0 || _pivot.y != 0)
			{
				Vector3 oldOffset = _pivotOffset;

				UpdatePivotOffset();
				_position += oldOffset - _pivotOffset;
				_outlineChanged = true;
			}
		}

		/// <summary>
		/// This is the pivot position
		/// </summary>
		public Vector3 location
		{
			get
			{
				Vector3 pos = _position;
				pos.x += _pivotOffset.x;
				pos.y += _pivotOffset.y;
				pos.z += _pivotOffset.z;
				return pos;
			}

			set
			{
				this.SetPosition(value.x - _pivotOffset.x, value.y - _pivotOffset.y, value.z - _pivotOffset.z);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public bool isDisposed
		{
			get { return _disposed; }
		}

		internal void InternalSetParent(Container value)
		{
			if (parent != value)
			{
				parent = value;
				_parentMatrixVersion = 0;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public Container topmost
		{
			get
			{
				DisplayObject currentObject = this;
				while (currentObject.parent != null)
					currentObject = currentObject.parent;
				return currentObject as Container;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public Stage stage
		{
			get
			{
				return topmost as Stage;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		virtual public bool touchable
		{
			get { return _touchable; }
			set { _touchable = value; }
		}

		void ValidateMatrix(bool checkParent)
		{
			if (parent != null)
			{
				if (checkParent)
					parent.ValidateMatrix(checkParent);
				if (_parentMatrixVersion != parent._matrixVersion)
				{
					_outlineChanged = true;
					_parentMatrixVersion = parent._matrixVersion;
				}
			}

			if (_outlineChanged)
			{
				_outlineChanged = false;
				_matrixVersion++;

				Matrix4x4 mat = MatrixHelper.TRS(_position, _rotation, new Vector3(_scale.x, _scale.y, 1));
				if (parent != null)
				{
					_localToWorldMatrix = parent._localToWorldMatrix * mat;
					_renderRotation = _rotation.z + parent._renderRotation;
				}
				else
				{
					_localToWorldMatrix = mat;
					_renderRotation = _rotation.z;
				}

				if (graphics != null || paintingGraphics != null)
				{
					Vector2 tl = (Vector2)_localToWorldMatrix.TransformPoint(Vector3.Zero);
					Vector2 br = (Vector2)_localToWorldMatrix.TransformPoint(new Vector3(_contentRect.Width, _contentRect.Height, 0));
					if (_renderRotation == 0)
					{
						_renderScale.x = Math.Abs(br.x - tl.x) / _contentRect.Width;
						_renderScale.y = Math.Abs(br.y - tl.y) / _contentRect.Height;
					}
					else
					{
						Vector2 tr = (Vector2)_localToWorldMatrix.TransformPoint(new Vector3(_contentRect.Width, 0, 0));
						Vector2 bl = (Vector2)_localToWorldMatrix.TransformPoint(new Vector3(0, _contentRect.Height, 0));

						_renderScale.x = (float)Math.Sqrt(Math.Pow(tr.x - tl.x, 2) + Math.Pow(tr.y - tl.y, 2)) / _contentRect.Width;
						_renderScale.y = (float)Math.Sqrt(Math.Pow(bl.x - tl.x, 2) + Math.Pow(bl.y - tl.y, 2)) / _contentRect.Height;
					}

					_renderRect = new Rect(tl.x, tl.y, _contentRect.Width, _contentRect.Height);
				}
			}
		}

		public Matrix4x4 transformMatrix
		{
			get
			{
				ValidateMatrix(true);
				return _localToWorldMatrix;
			}
		}

		/// <summary>
		/// 进入绘画模式，整个对象将画到一张RenderTexture上，然后这种贴图将代替原有的显示内容。
		/// 可以在onPaint回调里对这张纹理进行进一步操作，实现特殊效果。
		/// 可能有多个地方要求进入绘画模式，这里用requestorId加以区别，取值是1、2、4、8、16以此类推。1024内内部保留。用户自定义的id从1024开始。
		/// </summary>
		/// <param name="requestId">请求者id</param>
		/// <param name="margin">纹理四周的留空。如果特殊处理后的内容大于原内容，那么这里的设置可以使纹理扩大。</param>
		public void EnterPaintingMode(int requestorId, Margin? margin)
		{
			bool first = _paintingMode == 0;
			_paintingMode |= requestorId;
			if (first)
			{
				if (paintingGraphics == null)
					paintingGraphics = new NGraphics();
				else
					paintingGraphics.enabled = true;

				_paintingMargin = new Margin();
				_outlineChanged = true;
			}
			if (margin != null)
				_paintingMargin = (Margin)margin;
			_paintingFlag = 1;
		}

		/// <summary>
		/// 离开绘画模式
		/// </summary>
		/// <param name="requestId"></param>
		public void LeavePaintingMode(int requestorId)
		{
			if (_paintingMode == 0 || _disposed)
				return;

			_paintingMode ^= requestorId;
			if (_paintingMode == 0)
			{
				paintingGraphics.Clear();
				paintingGraphics.enabled = false;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public bool paintingMode
		{
			get { return _paintingMode > 0; }
		}

		/// <summary>
		/// 将整个显示对象（如果是容器，则容器包含的整个显示列表）静态化，所有内容被缓冲到一张纹理上。
		/// DC将保持为1。CPU消耗将降到最低。但对象的任何变化不会更新。
		/// 当cacheAsBitmap已经为true时，再次调用cacheAsBitmap=true将会刷新一次。
		/// </summary>
		public bool cacheAsBitmap
		{
			get { return _cacheAsBitmap; }
			set
			{
				_cacheAsBitmap = value;
				if (value)
					EnterPaintingMode(8, null);
				else
					LeavePaintingMode(8);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public IFilter filter
		{
			get
			{
				return _filter;
			}

			set
			{
				if (value == _filter)
					return;

				if (_filter != null)
					_filter.Dispose();

				if (value != null && value.target != null)
					value.target.filter = null;

				_filter = value;
				if (_filter != null)
					_filter.target = this;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public BlendMode blendMode
		{
			get { return _blendMode; }
			set
			{
				_blendMode = value;

				if (this is Container)
				{
					if (_blendMode != BlendMode.Normal)
					{
						EnterPaintingMode(2, null);
						paintingGraphics.blendMode = _blendMode;
					}
					else
						LeavePaintingMode(2);
				}
				else if (graphics != null)
					graphics.blendMode = _blendMode;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="targetSpace"></param>
		/// <returns></returns>
		virtual public Rect GetBounds(DisplayObject targetSpace)
		{
			EnsureSizeCorrect();

			if (targetSpace == this || _contentRect.Width == 0 || _contentRect.Height == 0) // optimization
			{
				return _contentRect;
			}
			else if (targetSpace == parent && _rotation.z == 0)
			{
				float sx = this.scaleX;
				float sy = this.scaleY;
				return new Rect(this.x, this.y, _contentRect.Width * sx, _contentRect.Height * sy);
			}
			else
				return TransformRect(_contentRect, targetSpace);
		}

		protected internal DisplayObject InternalHitTest()
		{
			if (!_visible || (HitTestContext.forTouch && (!_touchable || _touchDisabled)))
				return null;

			return HitTest();
		}

		protected internal DisplayObject InternalHitTestMask()
		{
			if (_visible)
				return HitTest();
			else
				return null;
		}

		virtual protected DisplayObject HitTest()
		{
			Rect rect = GetBounds(this);
			if (rect.Width == 0 || rect.Height == 0)
				return null;

			Vector2 localPoint = GlobalToLocal(HitTestContext.screenPoint);
			if (rect.Contains(localPoint.x, localPoint.y))
				return this;
			else
				return null;
		}

		/// <summary>
		/// 将舞台坐标转换为本地坐标
		/// </summary>
		/// <param name="point"></param>
		/// <returns></returns>
		public Vector2 GlobalToLocal(Vector2 point)
		{
			Matrix4x4 mat = transformMatrix.GetInverted();
			return (Vector2)mat.TransformPoint((Vector3)point);
		}

		/// <summary>
		/// 将本地坐标转换为舞台坐标
		/// </summary>
		/// <param name="point"></param>
		/// <returns></returns>
		public Vector2 LocalToGlobal(Vector2 point)
		{
			return (Vector2)transformMatrix.TransformPoint((Vector3)point);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="point"></param>
		/// <param name="targetSpace">null if to world space</param>
		/// <returns></returns>
		public Vector2 TransformPoint(Vector2 point, DisplayObject targetSpace)
		{
			if (targetSpace == null)
				targetSpace = Stage.inst;

			if (targetSpace == this)
				return point;

			Vector3 vec3 = transformMatrix.TransformPoint((Vector3)point);
			return (Vector2)targetSpace.transformMatrix.GetInverted().TransformPoint(vec3);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="rect"></param>
		/// <param name="targetSpace">null if to world space</param>
		/// <returns></returns>
		public Rect TransformRect(Rect rect, DisplayObject targetSpace)
		{
			if (targetSpace == null)
				targetSpace = Stage.inst;

			if (targetSpace == this)
				return rect;

			if (targetSpace == parent && _rotation.z == 0) // optimization
			{
				return new Rect((this.x + rect.x) * _scale.x, (this.y + rect.y) * _scale.y,
					rect.Width * _scale.x, rect.Height * _scale.y);
			}
			else
			{
				ValidateMatrix(true);

				Matrix4x4 mat = targetSpace.transformMatrix.GetInverted();
				ToolSet.TransformRect(ref rect, ref _localToWorldMatrix, ref mat);

				return rect;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public void RemoveFromParent()
		{
			if (parent != null)
				parent.RemoveChild(this);
		}

		virtual public void Visit()
		{
			ValidateMatrix(false);

			if (_paintingMode != 0)
			{
				NTexture paintingTexture = paintingGraphics.texture;
				if (paintingTexture != null && paintingTexture.disposed) //Texture可能已被Stage.MonitorTexture销毁
				{
					paintingTexture = null;
					_paintingFlag = 1;
				}
				if (_paintingFlag == 1)
				{
					_paintingFlag = 0;

					//从优化考虑，决定使用绘画模式的容器都需要明确指定大小，而不是自动计算包围。这在UI使用上并没有问题，因为组件总是有固定大小的
					int textureWidth = (int)Math.Round(_contentRect.Width + _paintingMargin.left + _paintingMargin.right);
					int textureHeight = (int)Math.Round(_contentRect.Height + _paintingMargin.top + _paintingMargin.bottom);
					if (paintingTexture == null || paintingTexture.width != textureWidth || paintingTexture.height != textureHeight)
					{
						if (paintingTexture != null)
							paintingTexture.Dispose();
						if (textureWidth > 0 && textureHeight > 0)
						{
							paintingTexture = new NTexture(new Texture(textureWidth, textureHeight, new byte[textureWidth * textureHeight * 4], true, true));
							Stage.inst.MonitorTexture(paintingTexture);
						}
						else
							paintingTexture = null;
						paintingGraphics.texture = paintingTexture;
					}

					if (paintingTexture != null)
					{
						paintingGraphics.Clear();
						paintingGraphics.AddQuad(new Rect(-_paintingMargin.left, -_paintingMargin.top, paintingTexture.width, paintingTexture.height),
							new Rect(0, 0, 1, 1), Color.White);
					}
					else
						paintingGraphics.Clear();
				}

				if (paintingTexture != null)
					paintingTexture.lastActive = Engine.Timer.GetCurrTime();
			}

			Stats.ObjectCount++;
		}

		virtual public void Update(UpdateContext context)
		{
			ValidateMatrix(false);

			if (_paintingMode != 0)
			{
				if (graphics != null && paintingGraphics.texture != null)
				{
					context.PushRenderTarget(paintingGraphics.texture, new Vector2(_renderRect.x, _renderRect.y));
					graphics.Render(_renderRect, _renderScale, _renderRotation, _alpha, context);
					context.PopRenderTarget();
				}

				if (!(this is Container))
					paintingGraphics.Render(_renderRect, _renderScale, _renderRotation, _alpha, context);
			}
			else
			{
				if (graphics != null)
					graphics.Render(_renderRect, _renderScale, _renderRotation, _alpha, context);
			}

			if (_filter != null)
				_filter.Update();
		}

		virtual public void Dispose()
		{
			if (_disposed)
				return;

			_disposed = true;
			RemoveFromParent();
			RemoveEventListeners();
			if (_filter != null)
				_filter.Dispose();
		}
	}
}
