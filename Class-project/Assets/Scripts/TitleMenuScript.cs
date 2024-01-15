using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleMenuScript : MonoBehaviour
{
    public Canvas mainScreenCanvas;
    public Canvas genresCanvas;

    void Start()
    {
        // Ensure the initial state is as desired
        ShowMainScreen();
    }
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

    public void ShowGenres()
    {
        mainScreenCanvas.gameObject.SetActive(false);
        genresCanvas.gameObject.SetActive(true);
    }

    public void ShowMainScreen()
    {
        mainScreenCanvas.gameObject.SetActive(true);
        genresCanvas.gameObject.SetActive(false);
    }
}
