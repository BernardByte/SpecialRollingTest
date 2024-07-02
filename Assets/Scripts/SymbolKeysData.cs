using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class SymbolKeysData : MonoBehaviour
{
    
    

    [HideInInspector] public readonly string[] symbolArray = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0", "@", "#", "$", "-", "_", "+", "(", ")", "/", "*", "\"", "'", ":", ";", "!", "?" };
    [HideInInspector] public readonly string[] alphabetArray = { "q", "w", "e", "r", "t", "y", "u", "i", "o", "p", "a", "s", "d", "f", "g", "h", "j", "k", "l", "z", "x", "c", "v", "b", "n", "m" };

    
    public bool IsTextExist(string[] arrayName,string keyText)
    {
        bool keyExist = Array.IndexOf(arrayName,keyText) >= 0;
        return keyExist;
    }    

    public int GetKeyIndex(string[] arrayName,string keyText)
    {
        int keyIndex = Array.IndexOf(arrayName, keyText);
        return keyIndex;
    }
}

