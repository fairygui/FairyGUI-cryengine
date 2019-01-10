﻿using System.Collections.Generic;
using System.Reflection;
using CryEngine;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class EventListener
	{
		EventBridge _bridge;
		string _type;

		public EventListener(EventDispatcher owner, string type)
		{
			_bridge = owner.GetEventBridge(type);
			_type = type;
		}

		/// <summary>
		/// 
		/// </summary>
		public string type
		{
			get { return _type; }
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="callback"></param>
		public void AddCapture(EventCallback1 callback)
		{
			_bridge.AddCapture(callback);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="callback"></param>
		public void RemoveCapture(EventCallback1 callback)
		{
			_bridge.RemoveCapture(callback);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="callback"></param>
		public void Add(EventCallback1 callback)
		{
			_bridge.Add(callback);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="callback"></param>
		public void Remove(EventCallback1 callback)
		{
			_bridge.Remove(callback);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="callback"></param>
		public void Add(EventCallback0 callback)
		{
			_bridge.Add(callback);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="callback"></param>
		public void Remove(EventCallback0 callback)
		{
			_bridge.Remove(callback);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="callback"></param>
		public void Set(EventCallback1 callback)
		{
			_bridge.Clear();
			if (callback != null)
				_bridge.Add(callback);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="callback"></param>
		public void Set(EventCallback0 callback)
		{
			_bridge.Clear();
			if (callback != null)
				_bridge.Add(callback);
		}

		/// <summary>
		/// 
		/// </summary>
		public bool isEmpty
		{
			get
			{
				return !_bridge.owner.hasEventListeners(_type);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public bool isDispatching
		{
			get
			{
				return _bridge.owner.isDispatching(_type);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public void Clear()
		{
			_bridge.Clear();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public bool Call()
		{
			return _bridge.owner.InternalDispatchEvent(this._type, _bridge, null, null);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public bool Call(object data)
		{
			return _bridge.owner.InternalDispatchEvent(this._type, _bridge, data, null);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public bool BubbleCall(object data)
		{
			return _bridge.owner.BubbleEvent(_type, data);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public bool BubbleCall()
		{
			return _bridge.owner.BubbleEvent(_type, null);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public bool BroadcastCall(object data)
		{
			return _bridge.owner.BroadcastEvent(_type, data);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public bool BroadcastCall()
		{
			return _bridge.owner.BroadcastEvent(_type, null);
		}
	}
}
