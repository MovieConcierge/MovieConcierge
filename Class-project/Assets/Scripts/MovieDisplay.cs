using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MovieDisplay : MonoBehaviour
{
    public GameObject movieTitle;
    public GameObject moviePoster;
    public GameObject movieInfo;
    public static string newTitle;
    public static Sprite newPoster;
    public static string newInfo;

    void Start()
    {
        StartCoroutine(PushTextOnScreen());
    }


    IEnumerator PushTextOnScreen()
    {
        yield return new WaitForSeconds(0.25f);

        DisplayMovie();
    }

    void DisplayMovie()
    {
        movieTitle.GetComponent<TextMeshProUGUI>().text = newTitle;
        moviePoster.GetComponent<Image>().sprite = newPoster;
        movieInfo.GetComponent<TextMeshProUGUI>().text = newInfo;
    }
}
