using System;
using UnityEngine;
using System.Linq;

public class Utils {
  private Utils() {}

  public static Vector3 add(Vector3 a, Vector3 b) {
    return a + b;
  }

  public static Vector3 subtract(Vector3 b, Vector3 a) {
    return a - b;
  }

  public static Vector3 multiply(Vector3 a, float multiple) {
    return a * multiple;
  }

  public static Vector3 multiply(Vector3 a, Vector3 multiple) {
    return new Vector3(a.x * multiple.x, a.y * multiple.y, a.z * multiple.z);
  }

  public static Vector3 average(Vector3 a, Vector3 b) {
    return new Vector3((a.x + b.x) * 0.5f, (a.y + b.y) * 0.5f, (a.z + b.x) * 0.5f);
  }

  public static float l2Norm2D(Vector3 point) {
    return (float) Math.Sqrt(Math.Pow(point.x, 2) + Math.Pow(point.y, 2));;
  }

  public static float maxAbs(Vector3 point) {
    return (new float[]{Math.Abs(point.x), Math.Abs(point.y), Math.Abs(point.z)}).Max();
  }

  public static float sumAbs(Vector3 point) {
    return Math.Abs(point.x) + Math.Abs(point.y) + Math.Abs(point.z);
  }

  public static void addAll(Vector3[] pointsList, Vector3 p) {
    for(int x = 0; x < pointsList.Length; x++)
    {
        pointsList[x] = add(pointsList[x], p);
    }
  }

  public static void subtractAll(Vector3 p, Vector3[] pointsList) {
    for(int x = 0; x < pointsList.Length; x++)
    {
        pointsList[x] = subtract(p, pointsList[x]);
    }
  }

  public static void multiplyAll(Vector3[] pointsList, float multiple) {
    for(int x = 0; x < pointsList.Length; x++)
    {
        pointsList[x] = multiply(pointsList[x], multiple);
    }
  }

  public static void multiplyAll(Vector3[] pointsList, Vector3 multiple) {
    for(int x = 0; x < pointsList.Length; x++)
    {
        pointsList[x] = multiply(pointsList[x], multiple);
    }
  }
}
