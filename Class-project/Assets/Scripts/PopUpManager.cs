using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PopUpManager : MonoBehaviour
{
    public GameObject popUpPanel;
    public GameObject background;
    public Image moviePosterImage;
    public Button infoButton;  // Reference to the info button
    public Button likeButton;  // Reference to the like button
    public Button dislikeButton;  // Reference to the dislike button
    public GameObject Genres;
    public GameObject Overview;
    public static string newOverview;
    public static string newGenres;

    public void ShowPopUp()
    {
        Genres.GetComponent<TextMeshProUGUI>().text = newGenres;
        Overview.GetComponent<TextMeshProUGUI>().text = newOverview;

        // Disable other elements in the canvas
        DisableCanvasElements();

        //background.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.8f);

        popUpPanel.SetActive(true);
    }

    private void DisableCanvasElements()
    {
        // Disable the info button, like button, dislike button, and movie title
        infoButton.interactable = false;
        likeButton.interactable = false;
        dislikeButton.interactable = false;

    }

    private void EnableCanvasElements()
    {
        // Enable the info button, like button, dislike button, and movie title
        infoButton.interactable = true;
        likeButton.interactable = true;
        dislikeButton.interactable = true;

    }

    public void ClosePopUp()
    {
        // Enable other elements in the canvas
        EnableCanvasElements();

        popUpPanel.SetActive(false);
        // Additional cleanup or actions can be added here if needed
    }

    // Method to be called by the onClick event of the "info" button
    public void OpenPopUp()
    {
        ShowPopUp();
        // You can replace the hardcoded values with actual data or parameters as needed
    }
}
