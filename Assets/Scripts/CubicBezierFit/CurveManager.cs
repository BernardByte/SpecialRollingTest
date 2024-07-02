/**
 * @author Muhammad Imran
 * 06/24/2024
 **/
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Targetgesture;
using utilities;
namespace burningmime.curves
{
    public class CurveManager : MonoBehaviour
    {
        [SerializeField] private List<Vector2> dataPoints = new List<Vector2>();  // Holds mouse positions in an order
        [SerializeField] private CubicBezier[] returnedCurves;
        [SerializeField] private int curveLenght;           // curve(s) length 
        [SerializeField] private float maxErrorForCurveFit = 0.5f; // the distance between fitted curve and data points
        [SerializeField] private float maxErrorForPreprocess = 0.1f;   // when calling methods from the class CurvePreprocess
        [SerializeField] private int samplesPerCurve = 50; // Number of samples per curve for the spline
        [SerializeField] private float multiplyFactor = 1.0f;   // multiply with control points (not used) default value is 1.0
        [SerializeField] public Vector2 centroidPoint = Vector2.zero;  //centroid point of curve(s)
        [SerializeField] public List<Vector2> offsetAndScaledOfConrolPoints = new List<Vector2>();  //offsets and scaled of control points of fitted curve(s)
        [SerializeField] private List<Vector2> userGestureSplinePoints; //holds curves points

        public int CurveLength
        {
            get { return curveLenght; }
            set { curveLenght = value; }
        }

        public float MaxCurveFitError
        {
            get { return maxErrorForCurveFit; }
            set { maxErrorForCurveFit = value; }
        }

        public float MaxPreprocessError
        {
            get { return maxErrorForPreprocess; }
            set { maxErrorForPreprocess = value;}
        }


        // for curve visualizaiton
        [SerializeField] private GameObject anchorPoint;        // start and end points of a curve
        [SerializeField] private GameObject controlPoint;       // control points of a curve
        [SerializeField] private GameObject samplePoint;        // prefab for visualizing sample points on the curve
        [SerializeField] private GameObject mouseTrail;        // prefab for visualizing sample points on the curve
        [SerializeField] private GameObject centerPoint;        // prefab for visualizing centroid of curves


        // #bool_values_for_visualization
        public bool isVisualizationOn = true; // if true then visualize points
        public bool isMouseVisualizationOn = true; // if true then visualize mouse movements points
        public bool isCurvePointVisualizationOn = true; // if true then visualize points on curve points
        public bool isCurveControlPointVisualizationOn = true; // if true then visualize control points of fitted curve


        // mouse position(s) in a circular array.
        public MouseMovements mouseMovement;


        // Curve fitting
        public List<Vector2> reduced;   // data points after preprocessing
        public CubicBezier[] curves;    // Actual fitted curve(s)

        // Target Gesture
        [SerializeField] public List<Vector2> targetGestureControlPoints; // holds predefined gestures control points 


        // #Target_Gesture
        TargetGesture targetGesture; // Object reference to TargetGesture

        // Start is called before the first frame update
        void Start()
        {
            // Reference to Mouse Movement class
            mouseMovement = gameObject.AddComponent<MouseMovements>();

            
            

            
            //Reference to TargetGesture class

            anchorPoint = Resources.Load<GameObject>("AnchorPoint");
            controlPoint = Resources.Load<GameObject>("ControlPoint");
            mouseTrail = Resources.Load<GameObject>("MouseTrail");
            centerPoint = Resources.Load<GameObject>("Centroid");
            samplePoint = Resources.Load<GameObject>("Points");

            


        }

        // Update is called once per frame


        /// <summary>
        /// This function caculate absolute difference between point1 and point2 of Vector2 type. (ManhatanDistance)
        /// </summary>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <returns>Absolute distance</returns>
        public Vector2 ManhattanDistance(Vector2 point1, Vector2 point2)
        {
            float distanceX = Mathf.Abs(point1.x - point2.x);
            float distanceY = Mathf.Abs(point1.y - point2.y);
            Vector2 difference = new Vector2(distanceX, distanceY);
            Debug.Log(difference);
            return difference;
        }



        /// <summary>
        /// compare two control points list of Vector2 types.
        /// </summary>
        /// <param name="DrawnGesture"></param>
        /// <param name="targetGestrue"></param>
        /// <returns>if lists are within tolerace then True, else false</returns>
        public bool ComparePoints(List<Vector2> DrawnGesture, List<Vector2> targetGestrue)
        {

            

            float tolerancePercent = 0.4f;

            int mathedControlPoints = 0;

            Vector2[] absoluteValues = new Vector2[targetGestrue.Count];

            //Debug.Log(targetGestrue.Count);
            for (int i = 0; i < absoluteValues.Length; i++)
            {
                absoluteValues[i] = ManhattanDistance(DrawnGesture[i], targetGestrue[i]);

                if (absoluteValues[i].x < tolerancePercent && absoluteValues[i].y < tolerancePercent)
                {
                    mathedControlPoints++;
                }
            }

            Debug.Log("Mathed Points: " + mathedControlPoints);

            
            return mathedControlPoints >= 3;

        }
        
        
        public void VisualizePoints(CubicBezier[] curveS,string visualSide)
        {
            mouseMovement.cursorTransformAsAParent = GameObject.FindGameObjectWithTag(visualSide).transform;
            // Destroy old points before creating new ones
            foreach (var point in GameObject.FindGameObjectsWithTag("anchor"))
            {
                Destroy(point);
            }
            foreach (var point in GameObject.FindGameObjectsWithTag("controlpoint"))
            {
                Destroy(point);
            }
            if (isVisualizationOn)
            {

                var center = Instantiate(centerPoint, Util.CalculateCentroid(curveS) * multiplyFactor, Quaternion.identity);
                center.tag = "anchor";
                // #Mouse_Movements_Visualization
                if (isMouseVisualizationOn) { mouseMovement.VisualizeMousePosition(mouseTrail); }
                if (isCurveControlPointVisualizationOn)
                {
                    // #anchorpoint #contorlpoint #visualization 
                    for (int i = 0; i < curves.Length; i++)
                    {
                        if (curves[i].p0 != null && curves[i].p3 != null)
                        {
                            var startAnchor = Instantiate(anchorPoint, curveS[i].p0, Quaternion.identity);
                            //startAnchor.transform.SetParent(mouseMovement.cursorTransformAsAParent, false);
                            startAnchor.tag = "anchor";
                            Destroy(startAnchor, 5f);
                            Debug.Log("Fist Control" + startAnchor.transform.position);

                            var endAnchor = Instantiate(anchorPoint, curveS[i].p3, Quaternion.identity);
                            //endAnchor.transform.SetParent(mouseMovement.cursorTransformAsAParent, false);
                            endAnchor.tag = "anchor";
                            Destroy(endAnchor, 5f);
                        }
                        if (curves[i].p1 != null && curves[i].p2 != null)
                        {
                            var control1 = Instantiate(controlPoint, curveS[i].p1, Quaternion.identity);
                            //control1.transform.SetParent(mouseMovement.cursorTransformAsAParent, false);
                            control1.tag = "controlpoint";
                            Destroy(control1, 5f);

                            var control2 = Instantiate(controlPoint, curveS[i].p2, Quaternion.identity);
                            //control2.transform.SetParent(mouseMovement.cursorTransformAsAParent, false);
                            control2.tag = "controlpoint";
                            Destroy(control2, 5f);
                        }

                    }
                }


                // #For_DeBugging_Purpose #Debug_Curves
                int x = 0;
                foreach (var curve in curveS)
                {
                   // Debug.Log($"{visualSide} Curve {x}" + curve);
                    x++;
                }




                if (isCurvePointVisualizationOn && CurveLength > 0)
                {
                    // Create a spline from the curves
                    Spline spline = new Spline(curveS, samplesPerCurve);



                    // Sample points along the spline and instantiate the samplePoint prefab
                    for (float u = 0; u <= 1; u += 1f / (curveS.Length * samplesPerCurve))
                    {
                        Vector2 position = spline.Sample(u);
                        // #Visualization_Of_Points_On_Fitted_Curve
                        GameObject point = Instantiate(samplePoint, position, Quaternion.identity);
                        //point.transform.SetParent(mouseMovement.cursorTransformAsAParent,false);
                        //visualizedPoints.Add(point);
                    }
                }
            }
        }

    }
}
