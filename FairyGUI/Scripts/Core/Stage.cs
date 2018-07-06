using System;
using System.Collections.Generic;
using CryEngine;
using CryEngine.Common;
using CryEngine.Rendering;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class Stage : Container, IGameUpdateReceiver, IGameRenderReceiver
	{
		/// <summary>
		/// 
		/// </summary>
		public float soundVolume { get; set; }

		DisplayObject _touchTarget;
		DisplayObject _focused;
		InputTextField _lastInput;
		UpdateContext _updateContext;
		List<DisplayObject> _rollOutChain;
		List<DisplayObject> _rollOverChain;
		TouchInfo _touchInfo;
		Vector2 _touchPosition;
		EventCallback1 _focusRemovedDelegate;
		List<NTexture> _toCollectTextures = new List<NTexture>();
		bool _soundEnabled;
		MouseEventListener _mouseEventListener;
		float _mousePositionX;
		float _mousePositionY;
		Dictionary<KeyId, float> _lastKeyDownTime;

		public static EventCallback0 beforeVisit;
		public static EventCallback0 afterVisit;

		static Stage _inst;
		/// <summary>
		/// 
		/// </summary>
		public static Stage inst
		{
			get
			{
				if (_inst == null)
					Instantiate();

				return _inst;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public static void Instantiate()
		{
			if (_inst == null)
			{
				_inst = new Stage();
				GRoot._inst = new GRoot();
				GRoot._inst.ApplyContentScaleFactor();
				_inst.AddChild(GRoot._inst.displayObject);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public static bool isTouchOnUI
		{
			get
			{
				return _inst != null && _inst.touchTarget != null;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public Stage()
			: base()
		{
			_inst = this;
			soundVolume = 1;

			_updateContext = new UpdateContext();
			_soundEnabled = true;

			_touchInfo = new TouchInfo();
			_touchInfo.touchId = 0;
			_lastKeyDownTime = new Dictionary<KeyId, float>();

			_rollOutChain = new List<DisplayObject>();
			_rollOverChain = new List<DisplayObject>();

			Timers.inst.Add(5, 0, RunTextureCollector);
			GameFramework.RegisterForUpdate(this);
			GameFramework.RegisterForRender(this);

			_focusRemovedDelegate = OnFocusRemoved;

			Renderer.ResolutionChanged += OnResolutionChanged;
			OnResolutionChanged(0, 0);

			_mouseEventListener = new MouseEventListener();
			Global.gEnv.pHardwareMouse.AddListener(_mouseEventListener);

			Input.OnKey += OnKeyEvents;
		}

		public override void Dispose()
		{
			base.Dispose();

			GameFramework.UnregisterFromUpdate(this);
		}

		public static bool touchScreen
		{
			get { return false; }
		}

		/// <summary>
		/// 
		/// </summary>
		public DisplayObject touchTarget
		{
			get
			{
				if (_touchTarget == this)
					return null;
				else
					return _touchTarget;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public DisplayObject focus
		{
			get
			{
				if (_focused != null && _focused.isDisposed)
					_focused = null;
				return _focused;
			}
			set
			{
				if (_focused == value)
					return;

				DisplayObject oldFocus = _focused;
				_focused = value;
				if (_focused == this)
					_focused = null;

				if (oldFocus != null)
				{
					if (oldFocus is InputTextField)
						((InputTextField)oldFocus).onFocusOut.Call();

					oldFocus.onRemovedFromStage.RemoveCapture(_focusRemovedDelegate);
				}

				if (_focused != null)
				{
					if (_focused is InputTextField)
					{
						_lastInput = (InputTextField)_focused;
						_lastInput.onFocusIn.Call();
					}

					_focused.onRemovedFromStage.AddCapture(_focusRemovedDelegate);
				}
			}
		}

		void OnFocusRemoved(EventContext context)
		{
			if (context.sender == _focused)
			{
				if (_focused is InputTextField)
					_lastInput = null;
				this.focus = null;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public Vector2 touchPosition
		{
			get
			{
				return _touchPosition;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="touchId"></param>
		/// <returns></returns>
		public Vector2 GetTouchPosition(int touchId)
		{
			return _touchPosition;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="touchId"></param>
		public void CancelClick(int touchId)
		{
			_touchInfo.clickCancelled = true;
		}

		/// <summary>
		/// 
		/// </summary>
		public void EnableSound()
		{
			_soundEnabled = true;
		}

		/// <summary>
		/// 
		/// </summary>
		public void DisableSound()
		{
			_soundEnabled = false;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="clip"></param>
		/// <param name="volumeScale"></param>
		public void PlayOneShotSound(AudioFile clip, float volumeScale)
		{
			if (_soundEnabled)
				clip.Play();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="clip"></param>
		public void PlayOneShotSound(AudioFile clip)
		{
			if (_soundEnabled)
				clip.Play();
		}

		static EventCallback0 _tempDelegate;

		public void OnRender()
		{
			Global.gEnv.pHardwareMouse.GetHardwareMouseClientPosition(ref _mousePositionX, ref _mousePositionY);
			if ((_mousePositionX < 0 || _mousePositionY < 0 || _mousePositionX >= this.width || _mousePositionY >= this.height)
				&& _touchInfo.began)
				HandleMouseEvents((int)_mousePositionX, (int)_mousePositionY, EHARDWAREMOUSEEVENT.HARDWAREMOUSEEVENT_LBUTTONUP, 0);
			if (_focused is InputTextField)
				HandleTextInput();


			_tempDelegate = beforeVisit;
			beforeVisit = null;

			//允许beforeVisit里再次Add，这里没有做死锁检查
			while (_tempDelegate != null)
			{
				_tempDelegate.Invoke();
				_tempDelegate = beforeVisit;
				beforeVisit = null;
			}
			_tempDelegate = null;

			Visit();

			if (afterVisit != null)
				afterVisit.Invoke();

			afterVisit = null;
		}

		public void OnUpdate()
		{
			_updateContext.Begin();
			Update(_updateContext);
			_updateContext.End();
		}

		void OnResolutionChanged(int lw, int lh)
		{
			SetSize(Renderer.ScreenWidth, Renderer.ScreenHeight);
#if CE_5_5
			NGraphics.viewportReverseScale = new Vector2(1, 1);
#else
			NGraphics.viewportReverseScale = new Vector2(800.0f / this.width, 600.0f / this.height);
#endif

			UIContentScaler.ApplyChange();
		}

		internal void HandleMouseEvents(int iX, int iY, EHARDWAREMOUSEEVENT eHardwareMouseEvent, int wheelDelta)
		{
			if (wheelDelta != 0)
			{
				if (_touchTarget != null)
				{
					_touchInfo.mouseWheelDelta = -wheelDelta;
					_touchInfo.UpdateEvent();
					_touchTarget.onMouseWheel.BubbleCall(_touchInfo.evt);
					_touchInfo.mouseWheelDelta = 0;
				}
				return;
			}

			_touchPosition = new Vector2(iX, iY);
			_touchTarget = HitTest(_touchPosition, true);
			_touchInfo.target = _touchTarget;

			if (_touchInfo.x != _touchPosition.x || _touchInfo.y != _touchPosition.y)
			{
				_touchInfo.x = _touchPosition.x;
				_touchInfo.y = _touchPosition.y;
				_touchInfo.Move();
			}

			if (_touchInfo.lastRollOver != _touchInfo.target)
				HandleRollOver(_touchInfo);

			if (eHardwareMouseEvent == EHARDWAREMOUSEEVENT.HARDWAREMOUSEEVENT_LBUTTONDOWN ||
				eHardwareMouseEvent == EHARDWAREMOUSEEVENT.HARDWAREMOUSEEVENT_RBUTTONDOWN ||
				eHardwareMouseEvent == EHARDWAREMOUSEEVENT.HARDWAREMOUSEEVENT_MBUTTONDOWN ||
				eHardwareMouseEvent == EHARDWAREMOUSEEVENT.HARDWAREMOUSEEVENT_LBUTTONDOUBLECLICK)
			{
				if (!_touchInfo.began)
				{
					_touchInfo.Begin();
					_touchInfo.button = (eHardwareMouseEvent == EHARDWAREMOUSEEVENT.HARDWAREMOUSEEVENT_LBUTTONDOWN || eHardwareMouseEvent == EHARDWAREMOUSEEVENT.HARDWAREMOUSEEVENT_LBUTTONDOUBLECLICK) ? 0 : (eHardwareMouseEvent == EHARDWAREMOUSEEVENT.HARDWAREMOUSEEVENT_RBUTTONDOWN ? 1 : 2);
					this.focus = _touchInfo.target;

					_touchInfo.UpdateEvent();
					_touchInfo.target.onTouchBegin.BubbleCall(_touchInfo.evt);
				}
			}
			if (eHardwareMouseEvent == EHARDWAREMOUSEEVENT.HARDWAREMOUSEEVENT_LBUTTONUP ||
				eHardwareMouseEvent == EHARDWAREMOUSEEVENT.HARDWAREMOUSEEVENT_RBUTTONUP ||
				eHardwareMouseEvent == EHARDWAREMOUSEEVENT.HARDWAREMOUSEEVENT_MBUTTONUP)
			{
				if (_touchInfo.began)
				{
					_touchInfo.button = eHardwareMouseEvent == EHARDWAREMOUSEEVENT.HARDWAREMOUSEEVENT_LBUTTONUP ? 0 : (eHardwareMouseEvent == EHARDWAREMOUSEEVENT.HARDWAREMOUSEEVENT_RBUTTONUP ? 1 : 2);
					_touchInfo.End();

					DisplayObject clickTarget = _touchInfo.ClickTest();
					if (clickTarget != null)
					{
						_touchInfo.UpdateEvent();

						if (eHardwareMouseEvent == EHARDWAREMOUSEEVENT.HARDWAREMOUSEEVENT_RBUTTONUP)
							clickTarget.onRightClick.BubbleCall(_touchInfo.evt);
						else
							clickTarget.onClick.BubbleCall(_touchInfo.evt);
					}

					_touchInfo.button = -1;
				}
			}
		}

		private static Dictionary<KeyId, string> _charByScanCode1 = new Dictionary<KeyId, string>
		{
			{ KeyId.Alpha1, "1"},{ KeyId.Alpha2, "2"},{ KeyId.Alpha3, "3"},{ KeyId.Alpha4, "4"},{ KeyId.Alpha5, "5"},
			{ KeyId.Alpha6, "6"},{ KeyId.Alpha7, "7"},{ KeyId.Alpha8, "8"},{ KeyId.Alpha9, "9"},{ KeyId.Alpha0, "0"},
			{ KeyId.Minus, "-"},{ KeyId.Equals, "="},{ KeyId.LBracket, "["},{ KeyId.RBracket, "]"},{ KeyId.Backslash, "\\"},
			{ KeyId.Semicolon, ";"},{ KeyId.Apostrophe, "'"},{ KeyId.Comma, ","},{ KeyId.Period, "."},{ KeyId.Slash, "/"},
			{ KeyId.Enter, "\n" }, {KeyId.Tab, "\t" }, {KeyId.Space, " " }, {KeyId.Tilde, "`" }
		};

		private static Dictionary<KeyId, string> _charByScanCode2 = new Dictionary<KeyId, string>
		{
			{ KeyId.Alpha1, "!"},{ KeyId.Alpha2, "@"},{ KeyId.Alpha3, "#"},{ KeyId.Alpha4, "$"},{ KeyId.Alpha5, "%"},
			{ KeyId.Alpha6, "^"},{ KeyId.Alpha7, "&"},{ KeyId.Alpha8, "*"},{ KeyId.Alpha9, "("},{ KeyId.Alpha0, ")"},
			{ KeyId.Minus, "_"},{ KeyId.Equals, "+"},{ KeyId.LBracket, "{"},{ KeyId.RBracket, "}"},{ KeyId.Backslash, "|"},
			{ KeyId.Semicolon, ":"},{ KeyId.Apostrophe, "\""},{ KeyId.Comma, "<"},{ KeyId.Period, ">"},{ KeyId.Slash, "?"},
			{KeyId.Tilde, "~" }
		};

		private static Dictionary<KeyId, string> _charByScanCode3 = new Dictionary<KeyId, string>
		{
			{ KeyId.NP_1, "1"},{ KeyId.NP_2, "2"},{ KeyId.NP_3, "3"},{ KeyId.NP_4, "4"},{ KeyId.NP_5, "5"},
			{ KeyId.NP_6, "6"},{ KeyId.NP_7, "7"},{ KeyId.NP_8, "8"},{ KeyId.NP_9, "9"},{ KeyId.NP_0, "0"},
			{ KeyId.NP_Add, "+"},{ KeyId.NP_Divide, "/"},{ KeyId.NP_Enter, "\n"},{ KeyId.NP_Multiply, "*"},{ KeyId.NP_Period, "."},
			{ KeyId.NP_Substract, "-"}
		};

		void OnKeyEvents(CryEngine.InputEvent evt)
		{
			//这里主要处理连按的逻辑，第一次收到按下通知后立刻发出键盘事件，然后延迟一段时间，如果后续还有down通知，按照一定频率触发键盘事件。
			float currTime = Engine.Timer.GetCurrTime();
			if (evt.State == InputState.Down)
			{
				float lastDownTime;
				if (_lastKeyDownTime.TryGetValue(evt.KeyId, out lastDownTime))
				{
					if (currTime - lastDownTime < 0)
						return;
				}

				_lastKeyDownTime[evt.KeyId] = currTime + 0.05f;
			}
			else if (evt.State == InputState.Released)
			{
				_lastKeyDownTime.Remove(evt.KeyId);
				return;
			}
			else if (evt.State == InputState.Pressed)
			{
				_lastKeyDownTime[evt.KeyId] = currTime + 0.5f;
			}
			else
				return;

			_touchInfo.keyCode = evt.KeyId;
			_touchInfo.keyName = null;
			_touchInfo.modifiers = evt.InputModifiers;

			bool shift = (evt.InputModifiers & InputModifierFlags.Shift) != 0;
			//evt.keyName is not reliable, I parse it myself.
			if (evt.KeyId >= KeyId.Q && evt.KeyId <= KeyId.P
				|| evt.KeyId >= KeyId.A && evt.KeyId <= KeyId.L
				|| evt.KeyId >= KeyId.Z && evt.KeyId <= KeyId.M)
			{
				bool capsLock = (evt.InputModifiers & InputModifierFlags.CapsLock) != 0;
				if (shift)
					capsLock = !capsLock;
				if (capsLock)
					_touchInfo.keyName = evt.KeyName.ToUpper();
				else
					_touchInfo.keyName = evt.KeyName;
			}
			else
			{
				if (_charByScanCode3.TryGetValue(evt.KeyId, out _touchInfo.keyName))
				{
					if ((evt.InputModifiers & InputModifierFlags.NumLock) == 0)
						_touchInfo.keyName = null;
				}
				else if (shift)
				{
					if (!_charByScanCode2.TryGetValue(evt.KeyId, out _touchInfo.keyName))
						_charByScanCode1.TryGetValue(evt.KeyId, out _touchInfo.keyName);
				}
				else
					_charByScanCode1.TryGetValue(evt.KeyId, out _touchInfo.keyName);
			}

			_touchInfo.UpdateEvent();
			DisplayObject f = this.focus;
			if (f != null)
				f.onKeyDown.BubbleCall(_touchInfo.evt);
			else
				this.onKeyDown.Call(_touchInfo.evt);
		}

		void HandleTextInput()
		{
			InputTextField textField = (InputTextField)_focused;
			if (!textField.editable)
				return;

			textField.CheckComposition();
		}

		void HandleRollOver(TouchInfo touch)
		{
			DisplayObject element;
			element = touch.lastRollOver;
			while (element != null)
			{
				_rollOutChain.Add(element);
				element = element.parent;
			}

			touch.lastRollOver = touch.target;

			element = touch.target;
			int i;
			while (element != null)
			{
				i = _rollOutChain.IndexOf(element);
				if (i != -1)
				{
					_rollOutChain.RemoveRange(i, _rollOutChain.Count - i);
					break;
				}
				_rollOverChain.Add(element);

				element = element.parent;
			}

			int cnt = _rollOutChain.Count;
			if (cnt > 0)
			{
				for (i = 0; i < cnt; i++)
				{
					element = _rollOutChain[i];
					if (element.stage != null)
						element.onRollOut.Call();
				}
				_rollOutChain.Clear();
			}

			cnt = _rollOverChain.Count;
			if (cnt > 0)
			{
				for (i = 0; i < cnt; i++)
				{
					element = _rollOverChain[i];
					if (element.stage != null)
						element.onRollOver.Call();
				}
				_rollOverChain.Clear();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="texture"></param>
		public void MonitorTexture(NTexture texture)
		{
			if (_toCollectTextures.IndexOf(texture) == -1)
				_toCollectTextures.Add(texture);
		}

		void RunTextureCollector(object param)
		{
			int cnt = _toCollectTextures.Count;
			float curTime = Engine.Timer.GetCurrTime();
			int i = 0;
			while (i < cnt)
			{
				NTexture texture = _toCollectTextures[i];
				if (texture.disposed)
				{
					_toCollectTextures.RemoveAt(i);
					cnt--;
				}
				else if (curTime - texture.lastActive > 5)
				{
					texture.Dispose();
					_toCollectTextures.RemoveAt(i);
					cnt--;
				}
				else
					i++;
			}
		}

		public void AddTouchMonitor(int touchId, EventDispatcher target)
		{
			if (_touchInfo.touchMonitors.IndexOf(target) == -1)
				_touchInfo.touchMonitors.Add(target);
		}

		public void RemoveTouchMonitor(EventDispatcher target)
		{
			int i = _touchInfo.touchMonitors.IndexOf(target);
			if (i != -1)
				_touchInfo.touchMonitors[i] = null;
		}
	}

	class MouseEventListener : IHardwareMouseEventListener
	{
		public override void OnHardwareMouseEvent(int iX, int iY, EHARDWAREMOUSEEVENT eHardwareMouseEvent)
		{
			Stage.inst.HandleMouseEvents(iX, iY, eHardwareMouseEvent, 0);
		}

		public override void OnHardwareMouseEvent(int iX, int iY, EHARDWAREMOUSEEVENT eHardwareMouseEvent, int wheelDelta)
		{
			Stage.inst.HandleMouseEvents(iX, iY, eHardwareMouseEvent, wheelDelta);
		}
	}

	class TouchInfo
	{
		public float x;
		public float y;
		public int touchId;
		public int clickCount;
		public KeyId keyCode;
		public string keyName;
		public char character;
		public InputModifierFlags modifiers;
		public int mouseWheelDelta;
		public int button;

		public float downX;
		public float downY;
		public bool began;
		public bool clickCancelled;
		public float lastClickTime;
		public DisplayObject target;
		public List<DisplayObject> downTargets;
		public DisplayObject lastRollOver;
		public List<EventDispatcher> touchMonitors;

		public InputEvent evt;

		static List<EventBridge> sHelperChain = new List<EventBridge>();

		public TouchInfo()
		{
			evt = new InputEvent();
			downTargets = new List<DisplayObject>();
			touchMonitors = new List<EventDispatcher>();
			Reset();
		}

		public void Reset()
		{
			touchId = -1;
			x = 0;
			y = 0;
			clickCount = 0;
			button = -1;
			keyCode = KeyId.Unknown;
			keyName = null;
			character = '\0';
			modifiers = 0;
			mouseWheelDelta = 0;
			lastClickTime = 0;
			began = false;
			target = null;
			downTargets.Clear();
			lastRollOver = null;
			clickCancelled = false;
			touchMonitors.Clear();
		}

		public void UpdateEvent()
		{
			evt.touchId = this.touchId;
			evt.x = this.x;
			evt.y = this.y;
			evt.clickCount = this.clickCount;
			evt.keyCode = this.keyCode;
			evt.KeyName = this.keyName;
			evt.modifiers = this.modifiers;
			evt.mouseWheelDelta = this.mouseWheelDelta;
			evt.button = this.button;
		}

		public void Begin()
		{
			began = true;
			clickCancelled = false;
			downX = x;
			downY = y;

			downTargets.Clear();
			if (target != null)
			{
				downTargets.Add(target);
				DisplayObject obj = target;
				while (obj != null)
				{
					downTargets.Add(obj);
					obj = obj.parent;
				}
			}
		}

		public void Move()
		{
			UpdateEvent();

			if (touchMonitors.Count > 0)
			{
				int len = touchMonitors.Count;
				for (int i = 0; i < len; i++)
				{
					EventDispatcher e = touchMonitors[i];
					if (e != null)
					{
						if ((e is DisplayObject) && ((DisplayObject)e).stage == null)
							continue;
						if ((e is GObject) && !((GObject)e).onStage)
							continue;
						e.GetChainBridges("onTouchMove", sHelperChain, false);
					}
				}

				Stage.inst.BubbleEvent("onTouchMove", evt, sHelperChain);
				sHelperChain.Clear();
			}
			else
				Stage.inst.onTouchMove.Call(evt);
		}

		public void End()
		{
			began = false;

			UpdateEvent();

			if (touchMonitors.Count > 0)
			{
				int len = touchMonitors.Count;
				for (int i = 0; i < len; i++)
				{
					EventDispatcher e = touchMonitors[i];
					if (e != null)
						e.GetChainBridges("onTouchEnd", sHelperChain, false);
				}
				target.BubbleEvent("onTouchEnd", evt, sHelperChain);

				touchMonitors.Clear();
				sHelperChain.Clear();
			}
			else
				target.onTouchEnd.BubbleCall(evt);

			if (Engine.Timer.GetCurrTime() - lastClickTime < 0.35f)
			{
				if (clickCount == 2)
					clickCount = 1;
				else
					clickCount++;
			}
			else
				clickCount = 1;
			lastClickTime = Engine.Timer.GetCurrTime();
		}

		public DisplayObject ClickTest()
		{
			if (downTargets.Count == 0
				|| clickCancelled
				|| Math.Abs(x - downX) > 50 || Math.Abs(y - downY) > 50)
				return null;

			DisplayObject obj = downTargets[0];
			if (obj.stage != null) //依然派发到原来的downTarget，虽然可能它已经偏离当前位置，主要是为了正确处理点击缩放的效果
				return obj;

			obj = target;
			while (obj != null)
			{
				int i = downTargets.IndexOf(obj);
				if (i != -1 && obj.stage != null)
					return obj;

				obj = obj.parent;
			}

			downTargets.Clear();

			return obj;
		}
	}
}