using UnityEngine;

public class TitleCanvasController : MonoBehaviour
{
    public Canvas mainScreenCanvas;
    public Canvas genresCanvas;

    void Start()
    {
        // Ensure the initial state is as desired
        ShowMainScreen();
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
