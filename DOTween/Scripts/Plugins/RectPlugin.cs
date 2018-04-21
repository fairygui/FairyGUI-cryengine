// Author: Daniele Giardini - http://www.demigiant.com
// Created: 2014/07/10 19:17
// 
// License Copyright (c) Daniele Giardini.
// This work is subject to the terms at http://dotween.demigiant.com/license.php

using System;
using DG.Tweening.Core;
using DG.Tweening.Core.Easing;
using DG.Tweening.Core.Enums;
using DG.Tweening.Plugins.Core;
using DG.Tweening.Plugins.Options;
using CryEngine;

#pragma warning disable 1591
namespace DG.Tweening.Plugins
{
    public class RectPlugin : ABSTweenPlugin<Rect, Rect, RectOptions>
    {
        public override void Reset(TweenerCore<Rect, Rect, RectOptions> t) { }

        public override void SetFrom(TweenerCore<Rect, Rect, RectOptions> t, bool isRelative)
        {
            Rect prevEndVal = t.endValue;
            t.endValue = t.getter();
            t.startValue = prevEndVal;
            if (isRelative) {
                t.startValue.x += t.endValue.x;
                t.startValue.y += t.endValue.y;
                t.startValue.Width += t.endValue.Width;
                t.startValue.Height += t.endValue.Height;
            }
            Rect to = t.startValue;
            if (t.plugOptions.snapping) {
                to.x = (float)Math.Round(to.x);
                to.y = (float)Math.Round(to.y);
                to.Width = (float)Math.Round(to.Width);
                to.Height = (float)Math.Round(to.Height);
            }
            t.setter(to);
        }

        public override Rect ConvertToStartValue(TweenerCore<Rect, Rect, RectOptions> t, Rect value)
        {
            return value;
        }

        public override void SetRelativeEndValue(TweenerCore<Rect, Rect, RectOptions> t)
        {
            t.endValue.x += t.startValue.x;
            t.endValue.y += t.startValue.y;
            t.endValue.Width += t.startValue.Width;
            t.endValue.Height += t.startValue.Height;
        }

        public override void SetChangeValue(TweenerCore<Rect, Rect, RectOptions> t)
        {
            t.changeValue = new Rect(
                t.endValue.x - t.startValue.x,
                t.endValue.y - t.startValue.y,
                t.endValue.Width - t.startValue.Width,
                t.endValue.Height - t.startValue.Height
			);
        }

        public override float GetSpeedBasedDuration(RectOptions options, float unitsXSecond, Rect changeValue)
        {
            // Uses length of diagonal to calculate units.
            float diffW = changeValue.Width;
            float diffH = changeValue.Height;
            float diag = (float)Math.Sqrt(diffW * diffW + diffH * diffH);
            return diag / unitsXSecond;
        }

        public override void EvaluateAndApply(RectOptions options, Tween t, bool isRelative, DOGetter<Rect> getter, DOSetter<Rect> setter, float elapsed, Rect startValue, Rect changeValue, float duration, bool usingInversePosition, UpdateNotice updateNotice)
        {
            if (t.loopType == LoopType.Incremental) {
                int iterations = t.isComplete ? t.completedLoops - 1 : t.completedLoops;
                startValue.x += changeValue.x * iterations;
                startValue.y += changeValue.y * iterations;
                startValue.Width += changeValue.Width * iterations;
                startValue.Height += changeValue.Height * iterations;
            }
            if (t.isSequenced && t.sequenceParent.loopType == LoopType.Incremental) {
                int iterations = (t.loopType == LoopType.Incremental ? t.loops : 1)
                    * (t.sequenceParent.isComplete ? t.sequenceParent.completedLoops - 1 : t.sequenceParent.completedLoops);
                startValue.x += changeValue.x * iterations;
                startValue.y += changeValue.y * iterations;
                startValue.Width += changeValue.Width * iterations;
                startValue.Height += changeValue.Height * iterations;
            }

            float easeVal = EaseManager.Evaluate(t.easeType, t.customEase, elapsed, duration, t.easeOvershootOrAmplitude, t.easePeriod);
            startValue.x += changeValue.x * easeVal;
            startValue.y += changeValue.y * easeVal;
            startValue.Width += changeValue.Width * easeVal;
            startValue.Height += changeValue.Height * easeVal;
            if (options.snapping) {
                startValue.x = (float)Math.Round(startValue.x);
                startValue.y = (float)Math.Round(startValue.y);
                startValue.Width = (float)Math.Round(startValue.Width);
                startValue.Height = (float)Math.Round(startValue.Height);
            }
            setter(startValue);
        }
    }
}