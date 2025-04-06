using System;
using UnityEngine;

public static class Utils
{
    public static string ToSpacedString(this string str)
    {
        for (int i = str.Length - 1; i > 0; --i)
        {
            if (char.IsUpper(str[i]))
                str = str.Insert(i, " ");
        }
        return str;
    }
    public const float PixelsPerUnit = 4;
    public static Vector2 RotatedBy(this Vector2 spinningpoint, float radians, Vector2 center = default(Vector2))
    {
        float xMult = (float)MathF.Cos(radians);
        float yMult = (float)MathF.Sin(radians);
        Vector2 vector = spinningpoint - center;
        Vector2 result = center;
        result.x += vector.x * xMult - vector.y * yMult;
        result.y += vector.x * yMult + vector.y * xMult;
        return result;
    }
    /// <summary>
    /// Converts the vector to the rotation in radians which would make the vector (magnitude, 0) become the vector.
    /// Basically runs arctan on the vector. y/x
    /// </summary>
    /// <param name="directionVector"></param>
    /// <returns></returns>
    public static float ToRotation(this Vector2 directionVector)
    {
        return Mathf.Atan2(directionVector.y, directionVector.x);
    }
    public static Quaternion ToQuaternion(this float rotation)
    {
        Quaternion relativeRotation = Quaternion.AngleAxis(rotation * Mathf.Rad2Deg, new Vector3(0, 0, 1));
        return relativeRotation;
    }
    public static float WrapAngle(this float x)
    {
        x = (x + Mathf.PI) % (2 * Mathf.PI);
        if (x < 0)
            x += Mathf.PI * 2;
        return x - Mathf.PI;
    }
    public static Vector2 MouseWorld => Camera.main.ScreenToWorldPoint(Input.mousePosition);
    public static float RandFloat(float max = 1)
    {
        return UnityEngine.Random.Range(0, max);
    }
    public static float RandFloat(float min, float max)
    {
        return UnityEngine.Random.Range(min, max);
    }
    public static int RandInt(int max = 1)
    {
        return UnityEngine.Random.Range(0, max);
    }
    public static int RandInt(int min, int maxExclusive)
    {
        return UnityEngine.Random.Range(min, maxExclusive);
    }
    public static Vector2 RandCircle(float r)
    {
        return UnityEngine.Random.insideUnitCircle * r;
    }
    public static Vector3 Lerp(this Vector3 vector3, Vector3 other, float amt)
    {
        return vector3 = Vector3.Lerp(vector3, other, amt);
    }
}
