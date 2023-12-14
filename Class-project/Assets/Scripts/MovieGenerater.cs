using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

//[RequireComponent(typeof(Button))]
public class MovieGenerater : MonoBehaviour
{
    public int count = 1;
    public bool displayingTitle = false;
    // Start is called before the first frame update
    void Start()
    {
        //var button = GetComponent<Button>();

        //button.onClick.AddListener(ChangeTitle);
    }

    // Update is called once per frame
    void Update()
    {
        if(displayingTitle == false){
            displayingTitle = true;
            MovieDisplay.title = "test1";
        }
    }

    void ChangeTitle()
    {
        count = count + 1;
    }
}