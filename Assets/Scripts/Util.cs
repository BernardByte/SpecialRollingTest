
using UnityEngine;
using burningmime.curves;
using System.Collections.Generic;

namespace utilities
{


    public class Util : MonoBehaviour
    {

        public static List<Vector2> curvePoints = new List<Vector2>();
        public static List<Vector2> dataPoints = new List<Vector2>();
        public static List<Vector2> NormalizeControlPoints(CubicBezier[] curves)
        {
            float maxDistance = 0f;

            // Finding the maximum distance from the origin
            foreach (var curve in curves)
            {
                // Calculate distance for each control point and update maxDistance if needed
                float distanceP0 = curve.p0.magnitude;
                float distanceP1 = curve.p1.magnitude;
                float distanceP2 = curve.p2.magnitude;
                float distanceP3 = curve.p3.magnitude;

                // Find the maximum distance among all control points in this curve
                float maxDistanceCurve = Mathf.Max(distanceP0, distanceP1, distanceP2, distanceP3);

                // Update overall maxDistance
                if (maxDistanceCurve > maxDistance)
                {
                    maxDistance = maxDistanceCurve;
                }
            }
            //Debug.Log("Max Distance" + maxDistance);


            //Normalize the control points
            curvePoints.Clear();
            Vector2[,] controlPoints = new Vector2[curves.Length, 4];
            

            foreach (var curve in curves)
            {
                // Divide each control point by maxDistance
                Vector2 p0 = curve.p0 / maxDistance;
                Vector2 p1 = curve.p1 / maxDistance;
                Vector2 p2 = curve.p2 / maxDistance;
                Vector2 p3 = curve.p3 / maxDistance;

                curvePoints.Add(p0);
                curvePoints.Add(p1);
                curvePoints.Add(p2);
                curvePoints.Add(p3);

                
            }


            return curvePoints;
        }


        public static List<Vector2> NormalizeDataPoints(List<Vector2> datapoints)
        {
            float maxDistance = 0f;

            // Finding the maximum distance from the origin
            foreach (var point in datapoints)
            {
                // Calculate distance for each data point and update maxDistance if needed
                float distance = point.magnitude;

                // Update overall maxDistance
                if (distance > maxDistance)
                {
                    maxDistance = distance;
                }
            }
            //Debug.Log("Max Distance" + maxDistance);


            //Normalize the data points
            dataPoints.Clear();



            foreach (var point in datapoints)
            {
                // Divide each data point by maxDistance

                Vector2 p = point / maxDistance;

                dataPoints.Add(p);

            }


            return dataPoints;
        }
        public static void Print2DArray(Vector2[,] arrayData)
        {
            for (int row = 0; row < arrayData.GetLength(0); row++)
            {
                string rowLog = $"Curve {row} Control Points: ";
                for (int col = 0; col < arrayData.GetLength(1); col++)
                {
                    rowLog += $"{arrayData[row, col]} ";
                }
                Debug.Log(rowLog);
            }
        }

        // #calculate_centroid
        public static Vector2 CalculateCentroid(CubicBezier[] curves)
        {
            float xValues = 0.0f;
            float yValues = 0.0f;
            int pointCount = curves.Length * 4; // Each curve has 4 control points

            foreach (var curve in curves)
            {
                xValues += curve.p0.x + curve.p1.x + curve.p2.x + curve.p3.x;
                yValues += curve.p0.y + curve.p1.y + curve.p2.y + curve.p3.y;
            }

            return new Vector2(xValues / pointCount, yValues / pointCount);
        }

        // #Calculate_Offsets of fitted curve's control points
        public static List<Vector2> CalculateOffsets(CubicBezier[] curves, Vector2 centroid)
        {
            List<Vector2> offsets = new List<Vector2>();

            foreach (var curve in curves)
            {
                offsets.Add(curve.p0 - centroid);
                offsets.Add(curve.p1 - centroid);
                offsets.Add(curve.p2 - centroid);
                offsets.Add(curve.p3 - centroid);
            }

            return offsets;
        }


        public static List<Vector2> CalculateAndScaleOffsets(CubicBezier[] curves, Vector2 centroid)
        {
            List<Vector2> offsets = new List<Vector2>();
            float maxOffsetLength = 0f;

            // Calculate offsets and find the maximum offset length
            foreach (var curve in curves)
            {
                offsets.Add(curve.p0 - centroid);
                offsets.Add(curve.p1 - centroid);
                offsets.Add(curve.p2 - centroid);
                offsets.Add(curve.p3 - centroid);
            }

            foreach (var offset in offsets)
            {
                float length = offset.magnitude;
                if (length > maxOffsetLength)
                {
                    maxOffsetLength = length;
                }
            }

            // Scale offsets
            List<Vector2> scaledOffsets = new List<Vector2>();
            foreach (var offset in offsets)
            {
                scaledOffsets.Add(offset / maxOffsetLength);
            }

            return scaledOffsets;
        }


        public static CubicBezier[] BuildCubicBezierArray(List<Vector2> points)
        {
            // offsetAndScaledCurve
            int numCubicBeziers = points.Count / 4;
            CubicBezier[] cubicArray = new CubicBezier[numCubicBeziers];

            // Fill the CubicBezier array
            for (int i = 0; i < numCubicBeziers; i++)
            {
                int index = i * 3;
                cubicArray[i] = new CubicBezier(
                    points[index],
                    points[index + 1],
                    points[index + 2],
                    points[index + 3]
                );
            }
            return cubicArray;
        }
    }
}