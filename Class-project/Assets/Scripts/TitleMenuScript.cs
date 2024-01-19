using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleMenuScript : MonoBehaviour
{
    public void OnClickLoadSoloScene()
    {
        // Set multiGame to 0 for Solo scene
        PlayerPrefs.SetInt("multiGame", 0);
        SceneManager.LoadScene("Solo");
    }

    public void OnClickLoadGroupScene()
    {
        // Set multiGame to 1 for Group scene
        PlayerPrefs.SetInt("multiGame", 1);
        SceneManager.LoadScene("Lobby");
    }

}
