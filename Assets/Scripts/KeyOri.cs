
    using System.Collections;
    using UnityEngine;
    using UnityEngine.UI;

    public delegate void OnKeyOriPressed(object sender);

    public class KeyOri : MonoBehaviour
    {
      
        private Text keyCapText;

        void Start()
        {
            keyCapText = GetComponentInChildren<Text>();
        }


        public string GetKeyText()
        {
            return keyCapText.text;
        }

    }