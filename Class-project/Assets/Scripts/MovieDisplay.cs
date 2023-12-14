using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

public class MovieDisplay : MonoBehaviour
{
    public TextMeshProUGUI screenTitle;
    public static string title;
    
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(PushTextOnScreen());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    IEnumerator PushTextOnScreen(){
        yield return new WaitForSeconds(0.25f);
        screenTitle.text = title;
    }
}
