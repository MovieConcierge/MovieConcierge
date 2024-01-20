using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PopUpManager : MonoBehaviour
{
    public GameObject popUpPanel;
    public GameObject Genres;
    public GameObject Genresbtn;
    public GameObject Overview;
    public static string newOverview;
    public static string newGenres;
    public Button infoButton;
    public ScrollRect scrollRect;

    public void ShowPopUp()
    {
        if (Genres != null)
        {
            Genres.GetComponent<TextMeshProUGUI>().text = newGenres;
        }
        if (Genresbtn != null)
        {
            Genresbtn.GetComponent<TextMeshProUGUI>().text = newGenres;
        }
        if (Overview != null)
        {
            Overview.GetComponent<TextMeshProUGUI>().text = newOverview;
        }

        popUpPanel.SetActive(true);
    }

    public void ClosePopUp()
    {
        popUpPanel.SetActive(false);

        // Reset the scroll position for each ScrollRect referenced
        scrollRect.normalizedPosition = new Vector2(0, 1);

    }

    // Method to be called by the onClick event of the "info" button
    public void OpenPopUp()
    {
        ShowPopUp();
        // You can replace the hardcoded values with actual data or parameters as needed
    }
}
