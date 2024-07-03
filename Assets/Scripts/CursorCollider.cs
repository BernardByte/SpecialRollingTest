using UnityEngine;
using System;
using Unity.VisualScripting;

public class CursorCollider : MonoBehaviour
{
    private Hive hiveScript;

    private Color selectedColor = new Color(0.055f, 0.561f, 0.243f);
    private Color originalColor = new Color(0.08891062f, 0.1522242f, 0.1698113f);

    public GameObject arc;
    public GameObject visualizeArc;

    private void Start()
    {
        hiveScript = FindObjectOfType<Hive>();

    }


    public void OnTriggerEnter(Collider other)
    {
        hiveScript.SetButtonColor(selectedColor, other.gameObject);
        if (other.gameObject.tag == "AlphabetKey" || other.gameObject.tag == "Space" || other.gameObject.tag == "Enter" || other.gameObject.tag == "BackSpace" || other.gameObject.tag == "Shift")
        {
            hiveScript.selectedButtonR = other.gameObject;
            //visualizeArc = Instantiate(arc, hiveScript.selectedButtonR.transform.position, Quaternion.identity);
            //visualizeArc.transform.localScale = new Vector3(0.007f,0.007f,0);
            //visualizeArc.transform.SetParent(hiveScript.selectedButtonR.transform, true);
            //if (other.gameObject.transform.Find("Arc").gameObject)
            //{
            //    visualizeArc = hiveScript.selectedButtonR.transform.Find("Arc").gameObject;
            //    visualizeArc.SetActive(true);

            //}


        }
        else if (other.gameObject.tag == "AlphabetKeyL" || other.gameObject.tag == "SpaceL" || other.gameObject.tag == "NumKeyL" || other.gameObject.tag == "ABC" || other.gameObject.tag == "StartL")
        {
            hiveScript.selectedButtonL = other.gameObject;

        }
    }

    public void OnTriggerExit(Collider other)
    {
        hiveScript.SetButtonColor(originalColor, other.gameObject);
        if (other.gameObject == hiveScript.selectedButtonR)
        {
            hiveScript.selectedButtonR = null;
            hiveScript.ResetCurveRelatedData("RightSide");
            //if(visualizeArc != null)
            //{
            //visualizeArc.SetActive(false);
            //}
        }
        else if (other.gameObject == hiveScript.selectedButtonL)
        {
            hiveScript.selectedButtonL = null;
            hiveScript.ResetCurveRelatedData("LeftSide");
        }
    }

}
