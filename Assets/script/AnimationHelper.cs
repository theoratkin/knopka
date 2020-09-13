/*
 * Animation utils.
 */

using System.Collections;

using UnityEngine;

public static class AnimationHelper
{
    public delegate float EasingFunction(float p, EasingCoreFunction func);
    public delegate float EasingCoreFunction(float f);

    public delegate void AnimationCoreFunction(float timePercentagePassed, float secondsPassed);
    public delegate void WhenDoneFunction();
    public delegate bool ConditionInProgressFunction();
    public delegate Coroutine StartCoroutineFunction(IEnumerator routine);

    // The following is taken from jQuery UI easing functions
    // https://jqueryui.com/resources/demos/effect/easing.html
    // There's a demo:
    // http://easings.net/

    public static float EaseInOut(float p, EasingCoreFunction func)
    {
        p = Mathf.Clamp01(p);
        return p < 0.5f ? func(p * 2f) / 2f : 1f - func(p * -2f + 2f) / 2f;
    }

    public static float EaseOut(float p, EasingCoreFunction func)
    {
        p = Mathf.Clamp01(p);
        return 1f - func(1f - p);
    }

    public static float EaseIn(float p, EasingCoreFunction func)
    {
        p = Mathf.Clamp01(p);
        return func(p);
    }

    public static float SquareFunction(float p)
    {
        return Mathf.Pow(p, 2f);
    }

    public static float CubicFunction(float p)
    {
        return Mathf.Pow(p, 3f);
    }

    public static float QuartFunction(float p)
    {
        return Mathf.Pow(p, 4f);
    }

    public static float QuintFunction(float p)
    {
        return Mathf.Pow(p, 5f);
    }

    public static float ExpoFunction(float p)
    {
        return Mathf.Pow(p, 6f);
    }

    public static float SineFunction(float p)
    {
        return 1f - Mathf.Cos(p * Mathf.PI / 2f);
    }

    public static float CircFunction(float p)
    {
        return 1f - Mathf.Sqrt(1f - p * p);
    }

    public static float BackFunction(float p)
    {
        return p * p * (3f * p - 2f);
    }

    public static float ElasticFunction(float p)
    {
        return p == 0f || p == 1f ? p :
            -Mathf.Pow(2f, 8f * (p - 1f)) * Mathf.Sin(((p - 1f) * 80f - 7.5f) * Mathf.PI / 15f);
    }

    public static float BounceFunction(float p)
    {
        float pow2, bounce = 4f;

        while (p < ((pow2 = Mathf.Pow(2f, --bounce)) - 1f) / 11f) { }
        return 1f / Mathf.Pow(4f, 3f - bounce) - 7.5625f * Mathf.Pow((pow2 * 3f - 2f) / 22f - p, 2f);
    }

    //template animation function
    public static IEnumerator RunAnimation(
        float animationTimeInSeconds,
        AnimationCoreFunction animateFunction,
        EasingFunction easingFunction = null,
        EasingCoreFunction easingCoreFunction = null,
        WhenDoneFunction onEndAnimationFunction = null,
        float waitForSecondsInsideAnimation = 0f,
        float waitForSecondsBefore = 0f,
        float waitForSecondsAfter = 0f)
    {
        if (animateFunction == null)
        {
            Debug.LogError("Error: runAnimation was called but no animation function provided");
            yield break;
        }

        if (waitForSecondsBefore > 0f)
            yield return new WaitForSeconds(waitForSecondsBefore);

        bool wait_for_seconds_ = waitForSecondsInsideAnimation > 0f;

        float beganAt = Time.time;
        float timePercentagePassed;
        float realSecondsPassed = 0f;

        do
        {
            timePercentagePassed = realSecondsPassed / animationTimeInSeconds;

            if (easingFunction != null && easingCoreFunction != null)
                timePercentagePassed = easingFunction(timePercentagePassed, easingCoreFunction);

            animateFunction(timePercentagePassed, realSecondsPassed);//animation itself goes here...

            if (wait_for_seconds_)
                yield return new WaitForSeconds(waitForSecondsInsideAnimation);
            else
                yield return null;

            realSecondsPassed = Time.time - beganAt;
        }
        while (animationTimeInSeconds > realSecondsPassed);

        if (waitForSecondsAfter > 0f)
            yield return new WaitForSeconds(waitForSecondsAfter);

        if (onEndAnimationFunction != null)
            onEndAnimationFunction();

        yield return null;
    }

    public static IEnumerator RunWaitForCondition(
        ConditionInProgressFunction inProgressFunction,
        WhenDoneFunction onEndAnimationFunction = null,
        float checkConditionDelay = 0f,
        float waitForSecondsBefore = 0f,
        float waitForSecondsAfter = 0f)
    {
        if (inProgressFunction == null)
        {
            Debug.LogError("Error: runWaitForCondition was called but no condition function provided");
            yield break;
        }

        if (waitForSecondsBefore > 0f)
            yield return new WaitForSeconds(waitForSecondsBefore);

        do
        {
            if (checkConditionDelay > 0f)
                yield return new WaitForSeconds(checkConditionDelay);
            else
                yield return null;
        }
        while (inProgressFunction());

        if (waitForSecondsAfter > 0f)
            yield return new WaitForSeconds(waitForSecondsAfter);

        if (onEndAnimationFunction != null)
            onEndAnimationFunction();

        yield return null;
    }

    public static IEnumerator RunOnce(
        WhenDoneFunction oneTimeFunction,
        float waitForSecondsBefore = 0f,
        float waitForSecondsAfter = 0f)
    {
        if (waitForSecondsBefore > 0f)
            yield return new WaitForSeconds(waitForSecondsBefore);

        oneTimeFunction();

        if (waitForSecondsAfter > 0f)
            yield return new WaitForSeconds(waitForSecondsAfter);

        yield return null;
    }
}