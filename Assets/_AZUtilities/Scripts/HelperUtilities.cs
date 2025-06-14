﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class ShakeProps
{
    public float duration = 1f;
    public float strength = 3f;
    public int vibrato = 10;
    public float randomness = 90f;
}

[Serializable]
public class PunchProps
{
    public float duration = 1f;
    public float power = 1f;
    public int vibrato = 10;
    public float elasticity = 1f;
}

[Serializable]
public class RangeFloat
{
    public float min;
    public float max;

    public float selected => _selected ?? SelectRandom();
    private float? _selected = null;

    public RangeFloat(float _min, float _max)
    {
        min = _min;
        max = _max;
    }

    public float GetRandom()
    {
        return Random.Range(min, max);
    }

    public float SelectRandom()
    {
        _selected = GetRandom();
        return selected;
    }
}

[Serializable]
public class RangeInt
{
    public int min;
    [Tooltip("Inclusive")] public int max;

    public int selected => _selected ?? SelectRandom();
    private int? _selected = null;

    public RangeInt(int _min, int _max)
    {
        min = _min;
        max = _max;
    }

    public int GetRandom()
    {
        return Random.Range(min, max + 1);
    }

    public int SelectRandom()
    {
        _selected = GetRandom();
        return selected;
    }
}

[Serializable]
public class SimpleTimer
{
    public RangeFloat durationRange;

    public float elapsedTime => _elapsedTime;
    [SerializeField] [ReadOnly] private float _elapsedTime;

    public bool expired => elapsedTime >= durationRange.selected;
    public float timeSinceExpiry => _elapsedTime - durationRange.selected;

    public float elapsedTimeClamped => expired ? durationRange.selected : elapsedTime;
    public float timeLeft => durationRange.selected - elapsedTimeClamped;

    public SimpleTimer(float duration = 3)
    {
        durationRange = new RangeFloat(duration, duration);
    }

    public void Update(bool useFixedDeltaTime = false)
    {
        _elapsedTime +=  Time.deltaTime;
    }

    public void Reset(bool expireImmediately = false)
    {
        _elapsedTime = 0;
        durationRange.SelectRandom();

        if (expireImmediately)
        {
            Expire();
        }
    }

    public void Expire()
    {
        _elapsedTime = durationRange.selected;
    }
}

public class HelperUtilities
{
    public static Vector3 CloneVector3(Vector3 origVector3)
    {
        return new Vector3(origVector3.x, origVector3.y, origVector3.z);
    }

    public static float Remap(float value, float from1, float to1, float from2, float to2)
    {
        value = Mathf.Clamp(value, from1, to1);
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }

    public static float ClampAngle(float angle, float min, float max)
    {
        bool over180 = false;
        if (angle > 180)
        {
            angle = angle - 360;
            over180 = true;
        }

        angle = Mathf.Clamp(angle, min, max);
        if (over180)
        {
            angle = 360 + angle;
        }

        return angle;
    }

    public static string GetTimeDisplay(float time)
    {
        if (time < 0)
        {
            return "--:--";
        }

        int minutes = (int) (time / 60);
        int seconds = (int) (time % 60);

        return $"{minutes:00}:{seconds:00}";
    }

    public static string GetPositionSuffix(int position)
    {
        switch (position)
        {
            case 1:
                return "st";
            case 2:
                return "nd";
            case 3:
                return "rd";
            default:
                return "th";

            // After 20th position, numbers ending with 1, 2, and 3 don't have the "th" suffix, but we are not supporting such large amounts of players. 
            // So this will suffice for now.
        }
    }

    public static void UpdateCursorLock(bool lockCursor)
    {
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public static void Rearrange<T>(List<T> items)
    {
        if (items.Count <= 1)
        {
            return;
        }

        System.Random _random = new System.Random();

        T last = items[items.Count - 1];

        int n = items.Count;
        for (int i = 0; i < n; i++)
        {
            int r = i + _random.Next(n - i);
            T t = items[r];
            items[r] = items[i];
            items[i] = t;
        }

        if (items[0].Equals(last))
        {
            int r = _random.Next(1, items.Count);
            T t = items[r];
            items[r] = items[0];
            items[0] = t;
        }
    }

    public static bool TestProbability(float probability)
    {
        return Random.Range(0f, 1f) < probability;
    }

    public static int GetOpaqueLayerMaskForRaycast()
    {
        int layerMask = -5;
        /*int transparentLayer = LayerMask.NameToLayer("TransparentFX");
        layerMask &= ~(1 << transparentLayer);
        int transparentMapLayer = LayerMask.NameToLayer("TransparentFXMap");
        layerMask &= ~(1 << transparentMapLayer);*/
        return layerMask;
    }

    public static IEnumerator WaitAndExecute(Action action, float delay, bool realtime = false)
    {
        if (realtime)
        {
            yield return new WaitForSecondsRealtime(delay);
        }
        else
        {
            yield return new WaitForSeconds(delay);
        }

        action?.Invoke();
    }

    public static IEnumerator WaitForFrameAndExecute(Action action)
    {
        Debug.Log("Before");
        yield return new WaitForEndOfFrame();
        Debug.Log("After");
        action?.Invoke();
    }

    public static List<T> GetAllResources<T>() where T : ScriptableObject
    {
        T[] resources = Resources.FindObjectsOfTypeAll<T>();
        return new List<T>(resources);
    }

    public static T[] InitializeArray<T>(int length) where T : new()
    {
        T[] array = new T[length];
        for (int i = 0; i < length; ++i)
        {
            array[i] = new T();
        }

        return array;
    }

    public static void DrawGizmosForBoxCollider(BoxCollider collider, Color borderColor, Color fillColor)
    {
        //Change the gizmo matrix to the relative space of the boxCollider.
        //This makes offsets with rotation work
        //Source: https://forum.unity.com/threads/gizmo-rotation.4817/#post-3242447
        Gizmos.matrix = Matrix4x4.TRS(collider.transform.TransformPoint(collider.center), collider.transform.rotation,
            collider.transform.lossyScale);

        //Draws the edges of the BoxCollider
        //Center is Vector3.zero, since we've transformed the calculation space in the previous step.
        Gizmos.color = borderColor;
        Gizmos.DrawWireCube(Vector3.zero, collider.size);

        //Draws the sides/insides of the BoxCollider.
        Gizmos.color = fillColor;
        Gizmos.DrawCube(Vector3.zero, collider.size);
    }

    public static void DrawGizmosForRelativeBounds(Bounds bounds, Transform transform, Color borderColor,
        Color fillColor)
    {
        //Change the gizmo matrix to the relative space of the boxCollider.
        //This makes offsets with rotation work
        //Source: https://forum.unity.com/threads/gizmo-rotation.4817/#post-3242447
        Gizmos.matrix =
            Matrix4x4.TRS(transform.TransformPoint(bounds.center), transform.rotation, transform.lossyScale);

        //Draws the edges of the BoxCollider
        //Center is Vector3.zero, since we've transformed the calculation space in the previous step.
        Gizmos.color = borderColor;
        Gizmos.DrawWireCube(Vector3.zero, bounds.size);

        //Draws the sides/insides of the BoxCollider.
        Gizmos.color = fillColor;
        Gizmos.DrawCube(Vector3.zero, bounds.size);
    }

    public static Vector3? FirstOrderIntercept(
        Vector3 shooterPos,
        Vector3 targetPos,
        Vector3 targetVelocity,
        float projectileSpeed)
    {
        Vector3 dirToTarget = targetPos - shooterPos;
        float targetSpeedSq = targetVelocity.sqrMagnitude;
        float projectileSpeedSq = projectileSpeed * projectileSpeed;

        float a = targetSpeedSq - projectileSpeedSq;
        float b = 2f * Vector3.Dot(dirToTarget, targetVelocity);
        float c = dirToTarget.sqrMagnitude;

        float discriminant = b * b - 4f * a * c;

        if (discriminant < 0)
            return null; // No interception possible

        float sqrtDisc = Mathf.Sqrt(discriminant);
        float t1 = (-b + sqrtDisc) / (2f * a);
        float t2 = (-b - sqrtDisc) / (2f * a);

        float time = Mathf.Min(t1, t2);
        if (time < 0) time = Mathf.Max(t1, t2);
        if (time < 0) return null;

        return targetPos + targetVelocity * time;
    }
}