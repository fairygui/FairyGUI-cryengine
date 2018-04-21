#if !COMPATIBLE
// Author: Daniele Giardini - http://www.demigiant.com
// Created: 2014/07/10 14:33
// 
// License Copyright (c) Daniele Giardini.
// This work is subject to the terms at http://dotween.demigiant.com/license.php

using DG.Tweening.Core;
using DG.Tweening.Core.Easing;
using DG.Tweening.Core.Enums;
using DG.Tweening.Plugins.Core;
using DG.Tweening.Plugins.Options;
using CryEngine;

#pragma warning disable 1591
namespace DG.Tweening.Plugins
{
	public class ColorPlugin : ABSTweenPlugin<Color, Color, ColorOptions>
	{
		public static Color Color_plus(Color a, Color b)
		{
			return new Color(a.R + b.R, a.G + b.G, a.B + b.B, a.A + b.A);
		}

		public static Color Color_minus(Color a, Color b)
		{
			return new Color(a.R - b.R, a.G - b.G, a.B - b.B, a.A - b.A);
		}

		public static Color Color_multiply(float b, Color a)
		{
			return new Color(a.R * b, a.G * b, a.B * b, a.A * b);
		}

		public static Color Color_multiply(Color a, float b)
		{
			return new Color(a.R * b, a.G * b, a.B * b, a.A * b);
		}

		public override void Reset(TweenerCore<Color, Color, ColorOptions> t) { }

		public override void SetFrom(TweenerCore<Color, Color, ColorOptions> t, bool isRelative)
		{
			Color prevEndVal = t.endValue;
			t.endValue = t.getter();
			t.startValue = isRelative ? Color_plus(t.endValue, prevEndVal) : prevEndVal;
			Color to = t.endValue;
			if (!t.plugOptions.alphaOnly) to = t.startValue;
			else to.A = t.startValue.A;
			t.setter(to);
		}

		public override Color ConvertToStartValue(TweenerCore<Color, Color, ColorOptions> t, Color value)
		{
			return value;
		}

		public override void SetRelativeEndValue(TweenerCore<Color, Color, ColorOptions> t)
		{
			t.endValue = Color_plus(t.endValue, t.startValue);
		}

		public override void SetChangeValue(TweenerCore<Color, Color, ColorOptions> t)
		{
			t.changeValue = Color_minus(t.endValue, t.startValue);
		}

		public override float GetSpeedBasedDuration(ColorOptions options, float unitsXSecond, Color changeValue)
		{
			return 1f / unitsXSecond;
		}

		public override void EvaluateAndApply(ColorOptions options, Tween t, bool isRelative, DOGetter<Color> getter, DOSetter<Color> setter, float elapsed, Color startValue, Color changeValue, float duration, bool usingInversePosition, UpdateNotice updateNotice)
		{
			if (t.loopType == LoopType.Incremental) startValue = Color_plus(startValue, Color_multiply(changeValue, (t.isComplete ? t.completedLoops - 1 : t.completedLoops)));
			if (t.isSequenced && t.sequenceParent.loopType == LoopType.Incremental)
			{
				startValue = Color_plus(startValue, Color_multiply(changeValue, ((t.loopType == LoopType.Incremental ? t.loops : 1)
					* (t.sequenceParent.isComplete ? t.sequenceParent.completedLoops - 1 : t.sequenceParent.completedLoops))));
			}

			float easeVal = EaseManager.Evaluate(t.easeType, t.customEase, elapsed, duration, t.easeOvershootOrAmplitude, t.easePeriod);
			if (!options.alphaOnly)
			{
				startValue.R += changeValue.R * easeVal;
				startValue.G += changeValue.G * easeVal;
				startValue.B += changeValue.B * easeVal;
				startValue.A += changeValue.A * easeVal;
				setter(startValue);
				return;
			}

			// Alpha only
			Color res = getter();
			res.A = startValue.A + changeValue.A * easeVal;
			setter(res);
		}
	}
}
#endif