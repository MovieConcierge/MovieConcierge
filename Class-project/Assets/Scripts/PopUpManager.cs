using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PopUpManager : MonoBehaviour
{
    public GameObject popUpPanel;
    public GameObject Genres;
    public GameObject movieTitlePanel;

    public GameObject Overview;
    public static string newOverview;
    public static string newGenres;
    public static string newTitle;
    public Button infoButton;
    public ScrollRect scrollRect;

    public void ShowPopUp()
    {
        Genres.GetComponent<TextMeshProUGUI>().text = newGenres;
        Overview.GetComponent<TextMeshProUGUI>().text = newOverview;
        movieTitlePanel.GetComponent<TextMeshProUGUI>().text = newTitle;

        infoButton.gameObject.SetActive(false);
        popUpPanel.SetActive(true);
    }

    public void ClosePopUp()
    {
        infoButton.gameObject.SetActive(true);
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
