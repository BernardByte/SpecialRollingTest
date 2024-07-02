using burningmime.curves;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using utilities;

namespace Targetgesture
{


    public class TargetGesture : MonoBehaviour
    {
        // for curve visualizaiton
        [SerializeField] private GameObject anchorPoint;        // start and end points of a curve
        [SerializeField] private GameObject controlPoint;       // control points of a curve
        [SerializeField] private GameObject centerPoint;       // control points of a curve
        [SerializeField] private GameObject samplePoint;       // control points of a curve
        public int samplesPerCurve = 50;

        public List<Vector2> targetSplinePoints; 
        // Start is called before the first frame update
        public Vector2 p0;
        public Vector2 p1;
        public Vector2 p2;
        public Vector2 p3;

        List<Vector2> list = new List<Vector2>();
        public List<Vector2> targetControlPoints;
        
        void Start()
        {
            

            if(anchorPoint != null && controlPoint != null && centerPoint != null && samplePoint != null)
            {
                Debug.Log("Visual points are set");
            }

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void TargetCurve()
        {
            

            p0 = new Vector2(-1.0f, -0.5f);
            p1 = new Vector2(-1f, 0f);
            p2 = new Vector2(1f, 0f);
            p3 = new Vector2(1f, -0.5f);

            list.Add(p0);
            list.Add(p1);
            list.Add(p2);
            list.Add(p3);

            CubicBezier[] oldCurve = CurveFit.Fit(list, 0.1f);
            List<Vector2> offsetAndScale = Util.CalculateAndScaleOffsets(oldCurve, Util.CalculateCentroid(oldCurve));
            
            targetControlPoints = offsetAndScale; // will be used for comparision
            
            CubicBezier[] curves = Util.BuildCubicBezierArray(offsetAndScale);

            for (int i = 0; i < curves.Length; i++)
            {
                if (curves[i].p0 != null && curves[i].p3 != null)
                {
                    Instantiate(anchorPoint, curves[i].p0, Quaternion.identity);
                    


                    Instantiate(anchorPoint, curves[i].p3, Quaternion.identity);
                    

                }
                if (curves[i].p1 != null && curves[i].p2 != null)
                {
                   Instantiate(controlPoint, curves[i].p1, Quaternion.identity);


                    Instantiate(controlPoint, curves[i].p2, Quaternion.identity);

                }

            }


            Instantiate(centerPoint, Util.CalculateCentroid(curves) , Quaternion.identity);

            Spline spline = new Spline(curves, samplesPerCurve);


            // Sample points along the spline and instantiate the samplePoint prefab
            for (float u = 0; u <= 1; u += 1f / (curves.Length * samplesPerCurve))
            {
                Vector2 position = spline.Sample(u);
                targetSplinePoints.Add(position);
                GameObject point = Instantiate(samplePoint, position, Quaternion.identity);

            }
        }
    }
}