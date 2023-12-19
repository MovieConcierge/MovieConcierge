using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

//[RequireComponent(typeof(Button))]
public class MovieGenerater : MonoBehaviour
{

    public static bool displayingTitle = false;
    public Sprite posterImage;

    void Start()
    {
        //var button = GetComponent<Button>();

        //button.onClick.AddListener(ChangeTitle);
    }


    void Update()
    {
        if(displayingTitle == false)
        {
            displayingTitle = true;
            SetMovieInformation("21 jump street", posterImage, "idk Johnny Depp is in here and it seems like it has to do with dancing");
        }
    }

        void SetMovieInformation(string title, Sprite poster, string info)
    {
        MovieDisplay.newTitle = title;
        MovieDisplay.newPoster = poster;
        MovieDisplay.newInfo = info;


    }
}