

using System.Collections.Generic;
using System;
using System.Linq;
using System.Collections;
using UnityEngine.UI;
using System.IO;
using UnityEngine;
using TMPro;
using System.Collections.Concurrent;
using MultiInput.Internal.Platforms.Windows;
using MultiInput.Internal.Platforms.Windows.PInvokeNet;
using MultiInput.Internal;


// #for #curve #fitting
using burningmime.curves;
using utilities;
using Targetgesture;
using Random = UnityEngine.Random;

[System.Serializable]
public class TypeData
{
    public int studentID;
    public int toolID;
    public int questionIndex;
    public float userResponseInterval;
    public List<float> intervals;
    public int T;   //transcribed string
    public float S;
    public int C;
    public int F;
    public int IF;
    public int INF;
    public float WMP;  //words per minute
    public float TotalER;
    public float CorrectedER;
    public float UncorrectedER;
    public string inputStream;
    public string transribedText;
    public float CorrectionEfficiency;
    public float Participant;
    public float Utilised;
    public float Wasted;

    public TypeData(int sID, int tID, int question)
    {
        studentID = sID;
        toolID = tID;
        questionIndex = question;
        userResponseInterval = 0f;
        intervals = new List<float>();
        T = 0;
        S = 0f;
        C = 0;
        F = 0;
        IF = 0;
        INF = 0;
        WMP = 0f;
        TotalER = 0f;
        CorrectedER = 0f;
        UncorrectedER = 0f;
        inputStream = string.Empty;
        transribedText = string.Empty;
        CorrectionEfficiency = 0f;
        Participant = 0f;
        Utilised = 0f;
        Wasted = 0f;
    }
}






public enum CursorType
{
    Left,
    Right
}




public class Hive : MonoBehaviour
{


    private MyWMListener listener;

    private readonly ConcurrentQueue<RawInput> inputQueue = new ConcurrentQueue<RawInput>();

    public TMP_InputField inputField;

    public TextMeshProUGUI answerSentence;
    public TextMeshProUGUI progress;

    private int questionIndex = 1;
    private int round = 1;

    private float displayQuestionTime = -1f;
    private float curTypeTime = -1f;
    private float preTypeTime = -1f;


    private string[] questions = {"practice makes perfect",
        "the future is here",
        "better things are coming",
        "my preferred treat is chocolate",
        "physics and chemistry are hard",
        "we are subjects and must obey",
        "this is a very good idea",
        "movie about a nutty professor",
        "my bank account is overdrawn",
        "the king sends you to the tower",
        "everyday is a second chance",
    };

    private string inputStream;
    private TypeData typeData;

    private string filePath = @"D:\TestingData\";

    private int toolNum = 4;
    private const int testingQuestionNum = 10;




    public Transform leftCursor;
    public Vector3 leftCursorPosition; // for debugging
    public Transform rightCursor;

    [SerializeField] private Vector3 leftCircleCenter;
    private Vector3 rightCircleCenter;

    private const int leftTrackballDeviceID = 250353131;
    private const int rightTrackballDeviceID = 65599;

    private const float cursorSpeed = 2.5f;

    [HideInInspector] public GameObject selectedButtonR;
    [HideInInspector] public GameObject selectedButtonL;

    [SerializeField]
    private GameObject[] buttonsR;
    [SerializeField]
    private GameObject[] buttonsL;

    //combining buttonsR and buttonsL
    [SerializeField]
    private GameObject[] alphabetButtons;


    [Header("TypingData")] // from Settings.Instance......
    public string fileName;
    public int studentID;
    public int toolID;



    // tag:#Symbol_Keys
    public SymbolKeysData symbolData;


    // tag:#Start_Key
    bool isStartPressed = false;
    GameObject startButton = null;

    #region Cursor Trajectory related members
    // #Cursor #trajectory #storage
    // using in circular array
    //[SerializeField] private MouseMovementRight mouseMovementRight; //object of right mouse movement class
    //[SerializeField] private MouseMovementLeft mouseMovementLeft;   //object of left mouse movement class

    private const int maxMouseArraySize = 30;  // size of array Note: set it also in MouseMovement.cs
    [SerializeField] private GameObject mouseTrail;

    // #curve_objects
    [Header("Curve Left")]
    public CurveManager curveManagerLeft;
    [Header("Curve Right")]
    public CurveManager curveManagerRight;

    //// Count frames 
    //private int frameCounter = 0;
    //private const int maxFrames = 150;

    // TargetGesture object
    TargetGesture targetGesture;
    [SerializeField] public List<Vector2> targetGestureControlPoints; // holds predefined gestures control points 

    // #bool_values_for_gesture
    public bool isLeftGestureMatched = false;
    public bool isLeftSideButtonPressed = false;
    public List<Vector2> dataPointsListLeft;

    public bool isRightGestureMatched = false;
    public bool isRightSideButtonPressed = true;

    public List<Vector2> dataPointsListRight;
    #endregion
    private void Awake()
    {
        curveManagerLeft = gameObject.AddComponent<CurveManager>();
        curveManagerRight = gameObject.AddComponent<CurveManager>();

        listener = new MyWMListener(OnInput, OnDeviceAdded, OnDeviceRemoved);
        typeData = new TypeData(studentID, toolID, questionIndex);
        symbolData = GetComponent<SymbolKeysData>();

    }

    private bool OnInput(RawInput input)
    {
        inputQueue.Enqueue(input);
        return true;
    }

    private void OnDeviceAdded(RawInputDevicesListItem device)
    {
    }

    private void OnDeviceRemoved(RawInputDevicesListItem device)
    {
    }





    private void Start()
    {
        // Target Gesture
        targetGesture = gameObject.GetComponent<TargetGesture>();

        targetGesture.TargetCurve(); // Create target curve

        // The List<Vector2> that holds target gestures control poitns
        targetGestureControlPoints = targetGesture.targetControlPoints;



        leftCircleCenter = leftCursor.localPosition;
        rightCircleCenter = rightCursor.localPosition;

        buttonsR = GameObject.FindGameObjectsWithTag("AlphabetKey"); //Right Hexakeys
        buttonsL = GameObject.FindGameObjectsWithTag("AlphabetKeyL"); //Left HexaKeys

        ///////////////////////////////////////////////////
        // Getting All alphabet's GameObjects into one array
        alphabetButtons = new GameObject[buttonsR.Length + buttonsL.Length];
        buttonsR.CopyTo(alphabetButtons, 0);
        buttonsL.CopyTo(alphabetButtons, buttonsR.Length);


        questionIndex = 0;
        filePath = filePath.Insert(filePath.Length, fileName);

        if (!File.Exists(filePath))
        {
            using (StreamWriter outStream = new StreamWriter(filePath, true, System.Text.Encoding.GetEncoding("utf-8")))
            {
                outStream.WriteLine("StudentID,ToolID,TranscribedLength,Seconds,Correct,IF,INF,WPM,TotalER,CorrectedER,UncorrectedER,InputStream,TranscribedText,F,CorrectionEfficiency,Participant,Utilised,Wasted");
            }
        }

        inputField.ActivateInputField();
        //progress.text = string.Empty;

        // Referencing start button
        startButton = GameObject.FindGameObjectWithTag("StartL");
        Debug.Log(startButton.GetComponentInChildren<TextMeshProUGUI>().text);


    }






    public void Update()
    {
        leftCursorPosition = leftCursor.position;

        Vector2 moveL = Vector2.zero;
        Vector2 moveR = Vector2.zero;

        while (inputQueue.TryDequeue(out var val))
        {
            if (val.Header.Type == RawInputType.Mouse && val.Header.Device.ToInt32() == leftTrackballDeviceID)
            {
                moveL.x += val.Data.Mouse.LastX;
                moveL.y -= val.Data.Mouse.LastY;
                //Debug.Log(moveL);
            }
            else if (val.Header.Type == RawInputType.Mouse && val.Header.Device.ToInt32() == rightTrackballDeviceID)
            {
                moveR.x += val.Data.Mouse.LastX;
                moveR.y -= val.Data.Mouse.LastY;
            }

            //Debug.Log("Mouse L,R: " + leftCursor.localPosition + "," + rightCursor.localPosition);
            // Debug.Log("Mouse L,R: " + moveL + "," + moveR);
        }


        // #Left_Mouse

        if (moveL != Vector2.zero)
        {

            UpdateCursorPosition(leftCursor, moveL, leftCircleCenter);


            //Add mouse position in a cicular array
            curveManagerLeft.mouseMovement.AddMousePosition(ref curveManagerLeft.mouseMovement.CursorMovements, leftCursor.position);
            dataPointsListLeft = curveManagerLeft.mouseMovement.MousePositionInOrder().ToList();
            curveManagerLeft.curves = CurveFit.Fit(dataPointsListLeft, curveManagerLeft.MaxCurveFitError);

            curveManagerLeft.centroidPoint = Util.CalculateCentroid(curveManagerLeft.curves);
            curveManagerLeft.offsetAndScaledOfConrolPoints = Util.CalculateAndScaleOffsets(curveManagerLeft.curves, curveManagerLeft.centroidPoint);

            // This function compare both (target & Drawn gestures)
            isLeftGestureMatched = curveManagerLeft.ComparePoints(curveManagerLeft.offsetAndScaledOfConrolPoints, targetGestureControlPoints);

            // if wanna visualize then put inside if (moveL != Vector2.zero)

            CubicBezier[] bezierArrayLeft = Util.BuildCubicBezierArray(curveManagerLeft.offsetAndScaledOfConrolPoints);

            curveManagerLeft.VisualizePoints(bezierArrayLeft, "LeftSideVisualizaiton");

            //curveManagerLeft.mouseMovement.CursorPositionsInOrder = curveManagerLeft.mouseMovement.MousePositionInOrder();
            curveManagerLeft.CurveLength = curveManagerLeft.curves.Length;  // assign curves length here
                                                                            // #Finish_Left
        }


        #region Right Side        // #Right_Mouse


        if (moveR != Vector2.zero)
        {




            UpdateCursorPosition(rightCursor, moveR, rightCircleCenter);


            //Add mouse position in a cicular array
            curveManagerRight.mouseMovement.AddMousePosition(ref curveManagerRight.mouseMovement.CursorMovements, rightCursor.position);
            dataPointsListRight = curveManagerRight.mouseMovement.MousePositionInOrder().ToList();
            curveManagerRight.curves = CurveFit.Fit(dataPointsListRight, curveManagerRight.MaxCurveFitError);

            curveManagerRight.centroidPoint = Util.CalculateCentroid(curveManagerRight.curves);
            curveManagerRight.offsetAndScaledOfConrolPoints = Util.CalculateAndScaleOffsets(curveManagerRight.curves, curveManagerRight.centroidPoint);

            // This function compare both (target & Drawn gestures)
            isRightGestureMatched = curveManagerRight.ComparePoints(curveManagerRight.offsetAndScaledOfConrolPoints, targetGestureControlPoints);

            // if wanna visualize then put inside if (moveR != Vector2.zero)
            CubicBezier[] bezierArrayRight = Util.BuildCubicBezierArray(curveManagerRight.offsetAndScaledOfConrolPoints);
            curveManagerRight.VisualizePoints(bezierArrayRight, "RightSideVisualizaiton");

            //curveManagerLeft.mouseMovement.CursorPositionsInOrder = curveManagerLeft.mouseMovement.MousePositionInOrder();
            curveManagerRight.CurveLength = curveManagerRight.curves.Length;  // assign curves length here
        }


        #endregion








        //from TypeChecker
        answerSentence.text = questions[questionIndex];
        if (round == 0)
        {
            progress.text = string.Empty;
        }
        else
        {
            progress.text = "(" + round.ToString() + " / " + testingQuestionNum.ToString() + ")";
        }
    }

    IEnumerator CurveProcessingDelay()
    {
        //isRightSideButtonPressed = false;
        //RightCurveProcessing();
        yield return new WaitForSeconds(0.7f);
    }
    void RightCurveProcessing()
    {
        curveManagerRight.mouseMovement.AddMousePosition(ref curveManagerRight.mouseMovement.CursorMovements, rightCursor.position);

        //Add mouse position in a cicular array
        dataPointsListRight = curveManagerRight.mouseMovement.MousePositionInOrder().ToList();
        curveManagerRight.curves = CurveFit.Fit(dataPointsListRight, curveManagerRight.MaxCurveFitError);

        curveManagerRight.centroidPoint = Util.CalculateCentroid(curveManagerRight.curves);
        // c
        //curveManagerRight.offsetAndScaledOfConrolPoints.Clear();
        curveManagerRight.offsetAndScaledOfConrolPoints = Util.CalculateAndScaleOffsets(curveManagerRight.curves, curveManagerRight.centroidPoint);

        // This function compare both (target & Drawn gestures)
        isRightGestureMatched = curveManagerRight.ComparePoints(curveManagerRight.offsetAndScaledOfConrolPoints, targetGestureControlPoints);


        CubicBezier[] bezierArrayRight = Util.BuildCubicBezierArray(curveManagerRight.offsetAndScaledOfConrolPoints);
        curveManagerRight.VisualizePoints(bezierArrayRight, "RightSideVisualizaiton");

        //curveManagerLeft.mouseMovement.CursorPositionsInOrder = curveManagerLeft.mouseMovement.MousePositionInOrder();
        curveManagerRight.CurveLength = curveManagerRight.curves.Length;  // assign curves length here

    }





    private void LateUpdate()
    {
        //ProcessKeyPress();
        if (isRightGestureMatched)
        {
            ProcessKeyPress();
            //            isRightGestureMatched = false;
        }
        if (isLeftGestureMatched)
        {
            ProcessKeyPress();
            isLeftGestureMatched = false;
        }
        inputField.MoveToEndOfLine(false, false);

    }





    private void UpdateCursorPosition(Transform cursor, Vector2 move, Vector3 cursorCenter)
    {
        Vector3 currentPosition = cursor.transform.localPosition;
        currentPosition.x += move.x * cursorSpeed * Time.deltaTime;
        currentPosition.y -= move.y * cursorSpeed * Time.deltaTime;

        Vector3 offset = currentPosition - cursorCenter;
        currentPosition = cursorCenter + Vector3.ClampMagnitude(offset, 6.3f);
        cursor.transform.localPosition = currentPosition;
    }

    public void SetButtonColor(Color color, GameObject button)
    {
        MeshRenderer[] renderers = button.GetComponents<MeshRenderer>();
        foreach (MeshRenderer renderer in renderers)
        {
            renderer.material.color = color;
        }
    }









    private void ProcessKeyPress()
    {

        if (isRightGestureMatched && selectedButtonR != null && selectedButtonR.tag == "Enter" && isStartPressed)  // Enter key 
        {
            Submit();
            ResetCurveRelatedData("RightSide");
        }
        if (isLeftGestureMatched && selectedButtonL != null && selectedButtonL.tag == "StartL")
        {
            Debug.Log("StartL was pressed.");
            isStartPressed = !isStartPressed;
            if (isStartPressed)
            {
                round = 1;
                questionIndex = round;
                inputStream = null;
                CleanInputField();
            }
            selectedButtonL.GetComponentInChildren<TextMeshProUGUI>().text = isStartPressed ? "End" : "Start";

            ResetCurveRelatedData("LeftSide");
        }
        else
        {

            Typing();
        }

    }



    void Typing()
    {
        if (round != 0)
        {
            if (isStartPressed)  // check either start button was pressed if true then calculate.
            {
                curTypeTime = Time.time;
                if (displayQuestionTime != -1f)
                {
                    typeData.userResponseInterval = curTypeTime - displayQuestionTime;
                    displayQuestionTime = -1f;
                }

                if (preTypeTime != -1f)
                {
                    float typeInterval = curTypeTime - preTypeTime;
                    typeData.S += typeInterval;
                    typeData.intervals.Add(typeInterval);
                }
                preTypeTime = curTypeTime;
            }

            // Buttons Handling in Left Hive
            //if (Input.GetKeyDown(KeyCode.F6) && selectedButtonL != null && selectedButtonL.tag == "AlphabetKeyL") //Alphabetical character handling
            if (isLeftGestureMatched && selectedButtonL != null && selectedButtonL.tag == "AlphabetKeyL") //Alphabetical character handling
            {
                TextMeshProUGUI buttonText = selectedButtonL.GetComponentInChildren<TextMeshProUGUI>();
                string character = buttonText.text;
                inputField.text += character.ToString();
                inputStream += buttonText.text; // from typing checker script

                //isLeftSideButtonPressed = true;

                ResetCurveRelatedData("LeftSide");



            }
            //if (Input.GetKeyDown(KeyCode.F6) && selectedButtonL != null && selectedButtonL.tag == "SpaceL") //Space handling
            if (isLeftGestureMatched && selectedButtonL != null && selectedButtonL.tag == "SpaceL") //Space handling
            {
                inputField.text += ' ';
                inputStream += " "; // from typing checker script

                ResetCurveRelatedData("LeftSide");
            }



            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            //NumKey handling
            //if (Input.GetKeyDown(KeyCode.F6) && selectedButtonL != null && (selectedButtonL.tag == "NumKeyL"))
            if (isLeftGestureMatched && selectedButtonL != null && (selectedButtonL.tag == "NumKeyL"))
            {



                Debug.Log("Num Key clicked.");
                selectedButtonL.name = "KeyABC";
                selectedButtonL.tag = "ABC";
                selectedButtonL.GetComponentInChildren<TextMeshProUGUI>().text = "ABC";


                // below loop changes alpha keys into num
                foreach (var buttonObject in alphabetButtons)
                {
                    TextMeshProUGUI buttonText = buttonObject.GetComponentInChildren<TextMeshProUGUI>();
                    buttonText.text = buttonText.text.ToLower();
                    if (symbolData.IsTextExist(symbolData.alphabetArray, buttonText.text))
                    {
                        int keyIndex = symbolData.GetKeyIndex(symbolData.alphabetArray, buttonText.text);
                        //Debug.Log(buttonText.text + ": " + keyIndex);
                        buttonText.text = symbolData.symbolArray[keyIndex];

                    }
                    else
                    {
                        buttonText.text = "";
                    }
                }

                ResetCurveRelatedData("LeftSide");
            }

            // Handling abc button
            else if (isLeftGestureMatched && selectedButtonL != null && (selectedButtonL.tag == "ABC"))  //abc handling
            {



                Debug.Log("ABC key clicked.");

                selectedButtonL.name = "Key123";
                selectedButtonL.tag = "NumKeyL";
                selectedButtonL.GetComponentInChildren<TextMeshProUGUI>().text = "123";

                // below loop changes num keys into alpha
                foreach (var buttonObject in alphabetButtons)
                {
                    TextMeshProUGUI buttonText = buttonObject.GetComponentInChildren<TextMeshProUGUI>();
                    if (symbolData.IsTextExist(symbolData.symbolArray, buttonText.text))
                    {
                        int keyIndex = symbolData.GetKeyIndex(symbolData.symbolArray, buttonText.text);
                        //Debug.Log(buttonText.text + ": " + keyIndex);
                        buttonText.text = symbolData.alphabetArray[keyIndex];

                    }
                    else
                    {
                        buttonText.text = "";
                    }
                }

                ResetCurveRelatedData("LeftSide");

            }
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            // #Buttons #Handling #in #Right #Hive
            //if (Input.GetKeyDown(KeyCode.F1) && selectedButtonR != null && selectedButtonR.tag == "AlphabetKey") //Alphabetical character handling
            if (isRightGestureMatched && selectedButtonR != null && selectedButtonR.tag == "AlphabetKey") //Alphabetical character handling
            {
                TextMeshProUGUI buttonText = selectedButtonR.GetComponentInChildren<TextMeshProUGUI>();
                string character = buttonText.text;
                inputField.text += character.ToString();
                inputStream += buttonText.text; // from typing checker script

                ResetCurveRelatedData("RightSide");

                // StartCoroutine(CallingCurveProcessing());

                isRightGestureMatched = false;
                //isRightSideButtonPressed = true;


            }
            if (isRightGestureMatched && selectedButtonR != null && selectedButtonR.tag == "BackSpace") //Backspace handling
            {
                if (inputField != null && inputField.text.Length > 0)
                {
                    inputField.text = inputField.text.Remove(inputField.text.Length - 1);
                    inputStream += "%"; // from typing checker script
                }
                ResetCurveRelatedData("RightSide");
            }
            if (isRightGestureMatched && selectedButtonR != null && selectedButtonR.tag == "Shift") //Shift handling
            {
                inputStream += "%"; // from typing checker script
                foreach (var buttonObject in buttonsR)
                {
                    TextMeshProUGUI buttonText = buttonObject.GetComponentInChildren<TextMeshProUGUI>();
                    if (buttonText != null && buttonObject.tag == "AlphabetKey")
                    {
                        if (buttonText.text == buttonText.text.ToUpper())
                        {
                            buttonText.text = buttonText.text.ToLower();
                        }
                        else
                        {
                            buttonText.text = buttonText.text.ToUpper();
                        }
                    }
                }

                foreach (var buttonObject in buttonsL)
                {
                    TextMeshProUGUI buttonText = buttonObject.GetComponentInChildren<TextMeshProUGUI>();
                    if (buttonText != null && buttonObject.tag == "AlphabetKeyL")
                    {
                        if (buttonText.text == buttonText.text.ToUpper())
                        {
                            buttonText.text = buttonText.text.ToLower();
                        }
                        else
                        {
                            buttonText.text = buttonText.text.ToUpper();
                        }
                    }
                }

                ResetCurveRelatedData("RightSide");
            }
            else if (Input.anyKeyDown)
            {
                foreach (var buttonObject in buttonsR)
                {
                    TextMeshProUGUI buttonText = buttonObject.GetComponentInChildren<TextMeshProUGUI>();
                    if (buttonText != null && buttonObject.tag == "AlphabetKey")
                    {
                        buttonText.text = buttonText.text.ToLower();
                    }
                }

                foreach (var buttonObject in buttonsL)
                {
                    TextMeshProUGUI buttonText = buttonObject.GetComponentInChildren<TextMeshProUGUI>();
                    if (buttonText != null && buttonObject.tag == "AlphabetKeyL")
                    {
                        buttonText.text = buttonText.text.ToLower();
                    }
                }
            }
        }




    }







    void Submit()
    {
        if (inputStream != string.Empty)
        {
            if (round != 0)
            {
                // same as Typing, can reduce code
                curTypeTime = Time.time;
                if (preTypeTime != -1f)
                {
                    float typeInterval = curTypeTime - preTypeTime;
                    typeData.S += typeInterval;
                    typeData.intervals.Add(typeInterval);
                }

                //Compare whole answer & save to log file
                typeData.inputStream = inputStream;
                string transribedText = inputField.text;
                typeData.transribedText = transribedText;
                typeData.T = inputField.text.Length;
                typeData.INF = MSD(questions[questionIndex], transribedText);
                typeData.C = Mathf.Max(questions[questionIndex].Length, transribedText.Length) - typeData.INF;

                for (int i = 0; i < inputStream.Length; i++)
                {
                    if (inputStream[i] == '%')
                    {
                        typeData.F++;
                    }
                }

                typeData.IF = inputStream.Length - transribedText.Length - typeData.F;

                typeData.WMP = (float)(typeData.T - 1) / typeData.S * 60f * 0.2f;
                typeData.TotalER = (float)(typeData.INF + typeData.IF) / (float)(typeData.C + typeData.INF + typeData.IF) * 100f;
                typeData.CorrectedER = (float)typeData.IF / (float)(typeData.C + typeData.INF + typeData.IF) * 100f;
                typeData.UncorrectedER = (float)typeData.INF / (float)(typeData.C + typeData.INF + typeData.IF) * 100f;
                typeData.CorrectionEfficiency = (typeData.F == 0f) ? 0f : (float)typeData.IF / (float)typeData.F;
                typeData.Participant = (typeData.IF + typeData.INF == 0f) ? 0f : (float)typeData.IF / ((float)typeData.IF + (float)typeData.INF);
                typeData.Utilised = (float)typeData.C / ((float)typeData.C + (float)typeData.INF + (float)typeData.IF + (float)typeData.F);
                typeData.Wasted = ((float)typeData.INF + (float)typeData.IF + (float)typeData.F) / ((float)typeData.C + (float)typeData.INF + (float)typeData.IF + (float)typeData.F);
                NextRound();
                //preTypeTime = -1f;
            }
        }
        CleanInputField();
    }



    int r(char x, char y)
    {
        if (x == y)
            return 0;
        else
            return 1;
    }

    int MSD(string A, string B)
    {
        int[,] D = new int[A.Length + 1, B.Length + 1];
        for (int i = 0; i <= A.Length; i++)
        {
            D[i, 0] = i;
        }
        for (int j = 0; j <= B.Length; j++)
        {
            D[0, j] = j;
        }
        for (int i = 1; i <= A.Length; i++)
            for (int j = 1; j <= B.Length; j++)
            {
                D[i, j] = Mathf.Min(D[i - 1, j] + 1, D[i, j - 1] + 1, D[i - 1, j - 1] + r(A[i - 1], B[j - 1]));
            }
        return D[A.Length, B.Length];

    }


    void NextRound()
    {
        displayQuestionTime = Time.time;
        if (round != 0)
        {
            // Save Data
            using (StreamWriter outStream = new StreamWriter(filePath, true, System.Text.Encoding.GetEncoding("utf-8")))
            {
                //outStream.WriteLine("StudentID, ToolID, QuestionID,ResponseTime,Intervals,TranscribedLength,Seconds,Correct,IF,INF,WPM,TotalER,CorrectedER,UncorrectedER,InputStream,TranscribedText");
                string timeList = "\"";
                for (int i = 0; i < typeData.intervals.Count; i++)
                {
                    timeList += typeData.intervals[i].ToString();
                    if (i != typeData.intervals.Count - 1)
                    {
                        timeList += ',';
                    }
                }
                timeList += "\"";
                outStream.WriteLine(typeData.studentID.ToString() +
                    "," + typeData.toolID.ToString() +
                    //"," + typeData.questionIndex.ToString() +
                    //"," + typeData.userResponseInterval.ToString() +
                    //"," + timeList +
                    "," + typeData.T.ToString() +
                    "," + typeData.S.ToString() +
                    "," + typeData.C.ToString() +
                    "," + typeData.IF.ToString() +
                    "," + typeData.INF.ToString() +
                    "," + typeData.WMP.ToString() +
                    "," + typeData.TotalER.ToString() +
                    "," + typeData.CorrectedER.ToString() +
                    "," + typeData.UncorrectedER.ToString() +
                    ",\"" + typeData.inputStream.ToString() + "\"" +
                    ",\"" + typeData.transribedText.ToString() + "\"" +
                    "," + typeData.F.ToString() +
                    "," + typeData.CorrectionEfficiency.ToString() +
                    "," + typeData.Participant.ToString() +
                    "," + typeData.Utilised.ToString() +
                    "," + typeData.Wasted.ToString());
            }
        }
        //round = round%questionsPerRound + 1;
        round = round + 1;
        switch (round)
        {
            case testingQuestionNum + 1://end phase
                round = 0;
                questionIndex = 0;
                isStartPressed = false;
                startButton.GetComponentInChildren<TextMeshProUGUI>().text = "Start";
                toolID = (toolID + 1) % toolNum;
                break;
            default:
                int cur = Random.Range(1, questions.Length);
                while (questionIndex == cur)
                {
                    cur = Random.Range(1, questions.Length);
                }
                questionIndex = cur;
                questionIndex = round;
                break;
        }

        typeData = new TypeData(studentID, toolID, questionIndex);
        inputStream = string.Empty;
        preTypeTime = -1f;
    }


    public void CleanInputField()
    {
        inputField.text = string.Empty;
        inputField.ActivateInputField();
    }

    public void ResetCurveRelatedData(string mouseSide)
    {
        if (mouseSide == "RightSide")
        {
            Array.Clear(curveManagerRight.mouseMovement.CursorMovements, 0, curveManagerRight.mouseMovement.CursorMovements.Length);
            dataPointsListRight.Clear();
            curveManagerRight.offsetAndScaledOfConrolPoints.Clear();
            //StartCoroutine(CurveProcessingDelay());
        }
        else if (mouseSide == "LeftSide")
        {
            Array.Clear(curveManagerLeft.mouseMovement.CursorMovements, 0, curveManagerLeft.mouseMovement.CursorMovements.Length);
            dataPointsListLeft.Clear();
            curveManagerLeft.offsetAndScaledOfConrolPoints.Clear();
            // StartCoroutine(CurveProcessingDelay());
        }
    }
    private void OnDestroy()
    {
        if (listener != null)
        {
            listener.Dispose();
            listener = null;
        }
    }


}
