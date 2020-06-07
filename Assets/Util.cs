using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Util
{
    public static float GetAngle(Vector2 fromVector2, Vector2 toVector2)
    {
        float angle = Vector2.Angle(fromVector2, toVector2);
        Vector3 cross = Vector3.Cross(fromVector2, toVector2);

        if (cross.z > 0)
        {
            angle = 360 - angle;
        }

        return angle;
    }

    public static float GetAngle(Vector2 p_vector2)
    {
        if (p_vector2.x < 0)
        {
            return 360 - (Mathf.Atan2(p_vector2.x, p_vector2.y) * Mathf.Rad2Deg * -1);
        }
        else
        {
            return Mathf.Atan2(p_vector2.x, p_vector2.y) * Mathf.Rad2Deg;
        }
    }

    public static float ConvertRange(float oldValue, float oldMin, float oldMax, float newMin, float newMax)
    {
        return (((oldValue - oldMin) * (newMax - newMin)) / (oldMax - oldMin)) + newMin;
    }

    public static GameObject InstantianteParent(GameObject go, Transform parent)
    {
        GameObject newObj;
        if (parent != null)
        {
            newObj = GameObject.Instantiate(go, parent);
        } else
        {
            newObj = GameObject.Instantiate(go);
        }
        newObj.transform.position = go.transform.position;
        newObj.transform.rotation = go.transform.rotation;

        //for (int i = newObj.transform.childCount - 1; i > 0; i--)
        //{
        //    GameObject.Destroy(newObj.transform.GetChild(i));
        //}
        DestroyChildren(newObj);

        return newObj;
    }

    public static void DestroyChildren(GameObject go)
    {
        foreach (Transform child in go.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
    }

    public static void DestroySiblings(GameObject go)
    {
        foreach (Transform child in go.transform.parent)
        {
            if (child != go.transform)
            {
                GameObject.Destroy(child.gameObject);
            }
        }
    }

    public static Vector3 RandomPerpendicularVector(Vector3 vector)
    {
        Quaternion quat = Quaternion.Euler(vector);
        Quaternion random = Quaternion.AngleAxis(Random.Range(0, 360), vector) * quat;
        return random.eulerAngles.normalized;
    }

    public static void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (null == obj)
        {
            return;
        }

        obj.layer = newLayer;

        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }

    public static void SetTagRecursively(GameObject obj, string newTag)
    {
        if (null == obj)
        {
            return;
        }

        obj.tag = newTag;

        foreach (Transform child in obj.transform)
        {
            SetTagRecursively(child.gameObject, newTag);
        }
    }

    public static bool LayerInMask(LayerMask layerMask, int layer)
    {
        return layerMask == (layerMask | (1 << layer));
    }

    public static bool GetButton(string name)
    {
        return Input.GetButton(name) || (Input.GetAxis(name) != 0);
    }

    public static Vector3 HiddenPoint = new Vector3(0, -100000, 0);

    public static List<Vector3> GeneratePath(Vector3 start, Vector3 middle, Vector3 end, float parabolaPointDistance)
    {
        Vector2 start2 = new Vector2(start.x, start.z);
        Vector2 middle2 = new Vector2(middle.x, middle.z);
        Vector2 end2 = new Vector2(end.x, end.z);

        //calculate parabola
        float a, b, c;
        CalculateParabola(start, middle, end, out a, out b, out c);

        //calculate direction, and distance for each anchor point
        Vector2 direction = (end2 - start2).normalized * parabolaPointDistance;
        float flatDistanceToMiddle = Vector2.Distance(start2, middle2);
        float flatDistanceToEnd = Vector2.Distance(start2, end2);

        //start building path
        List<Vector3> path = new List<Vector3>();
        path.Add(start);

        Vector2 offset = direction;
        while (offset.magnitude < flatDistanceToMiddle)
        {
            path.Add(GenerateParabolaPoint(start, offset, a, b, c));
            offset += direction;
        }
        path.Add(middle);
        while (offset.magnitude < flatDistanceToEnd)
        {
            path.Add(GenerateParabolaPoint(start, offset, a, b, c));
            offset += direction;
        }
        path.Add(end);

        return path;
    }

    //calculate parabola in x, z space
    public static void CalculateParabola(Vector3 start, Vector3 middle, Vector3 end, out float a, out float b, out float c)
    {
        Vector2 start2 = new Vector2(start.x, start.z);
        Vector2 middle2 = new Vector2(middle.x, middle.z);
        Vector2 end2 = new Vector2(end.x, end.z);

        Vector2 point1 = new Vector2(0, start.y);
        Vector2 point2 = new Vector2(Vector2.Distance(start2, middle2), middle.y);
        Vector2 point3 = new Vector2(Vector2.Distance(start2, end2), end.y);

        //c = y - ax^2 - bx
        //for point 1, x is 0
        //so c = point1.y
        c = point1.y;

        //a = (y2 - bx2 - c) / (x2^2)
        //b = (y3 - a(x3^2) - c) / x3

        //a = (y2 - [x2 * (y3 - a(x3^2) - c) / x3] - c) / (x2^2)
        //a = (y2 - x2*y3/x3 - a(x2*x3^2)/x3 - x2*c/x3 - c) / (x2^2)
        //a = (y2 - x2*y3/x3 - a(x2*x3) - x2*c/x3 - c) / (x2^2)
        //a = [ (y2 - x2*y3/x3 - x2*c/x3 - c) / (x2^2) ] - a(x2*x3)/(x2^2)
        //a + a(x2*x3)/(x2^2) = [ (y2 - x2*y3/x3 - x2*c/x3 - c) / (x2^2) ]
        //a [ 1 + (x2*x3)/(x2^2) ] = [(y2 - x2*y3/x3 - x2*c/x3 - c) / (x2^2) ]
        //a = [(y2 - x2*y3/x3 - x2*c/x3 - c) / (x2^2) ] / [ 1 + (x2*x3)/(x2^2) ]

        float x2 = point2.x;
        float y2 = point2.y;
        float x3 = point3.x;
        float y3 = point3.y;
        // a = ((y2 - x2 * y3 / x3 - x2 * c / x3 - c) / (x2 * x2)) / (1 + (x2 * x3) / (x2 * x2));
        a = (y2 - (x2 * y3 / x3) + (c * x2 / x3) - c) / ((x2 * x2) - (x2 * x3));

        b = (y3 - a * (x3 * x3) - c) / x3;

        c = 0;
    }

    private static Vector3 GenerateParabolaPoint(Vector3 start, Vector2 point, float a, float b, float c)
    {
        float y = EvaluateParabola(point.magnitude, a, b, c);
        return start + new Vector3(point.x, y, point.y);
    }

    public static float EvaluateParabola(float x, float a, float b, float c)
    {
        return (a * x * x) + (b * x) + c;
    }

    public static Vector3 Average(ContactPoint[] contactPoints)
    {
        int numPoints = contactPoints.Length;
        Vector3 avg = Vector3.zero;

        foreach(ContactPoint cp in contactPoints)
        {
            avg += cp.point;
        }

        avg /= numPoints;

        return avg;
    }

    public static Vector3[] MakeSmoothCurve(Vector3[] arrayToCurve, float smoothness)
    {
        List<Vector3> points;
        List<Vector3> curvedPoints;
        int pointsLength = 0;
        int curvedLength = 0;

        if (smoothness < 1.0f) smoothness = 1.0f;

        pointsLength = arrayToCurve.Length;

        curvedLength = (pointsLength * Mathf.RoundToInt(smoothness)) - 1;
        curvedPoints = new List<Vector3>(curvedLength);

        float t = 0.0f;
        for (int pointInTimeOnCurve = 0; pointInTimeOnCurve < curvedLength + 1; pointInTimeOnCurve++)
        {
            t = Mathf.InverseLerp(0, curvedLength, pointInTimeOnCurve);

            points = new List<Vector3>(arrayToCurve);

            for (int j = pointsLength - 1; j > 0; j--)
            {
                for (int i = 0; i < j; i++)
                {
                    points[i] = (1 - t) * points[i] + t * points[i + 1];
                }
            }

            curvedPoints.Add(points[0]);
        }

        return (curvedPoints.ToArray());
    }
}
