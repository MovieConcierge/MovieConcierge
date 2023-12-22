using UnityEngine;
using TMPro;

public class PopUpManager : MonoBehaviour
{
    public GameObject popUpPanel;

    public TextMeshProUGUI movieTitleText;
    public TextMeshProUGUI genresText;
    public TextMeshProUGUI overviewText;

    private string currentTitle;
    private string currentGenres;
    private string currentOverview;

    public void ShowPopUp(string title, string genres, string overview)
    {
        currentTitle = title;
        currentGenres = genres;
        currentOverview = overview;

        UpdatePopUpUI(); // Call a method to update the UI with the current data

        popUpPanel.SetActive(true);
    }

    private void UpdatePopUpUI()
    {
        movieTitleText.text = currentTitle;
        genresText.text = currentGenres;
        overviewText.text = currentOverview;
    }

    // Merge HidePopUp and ExitPopUp into a single function
    public void ClosePopUp()
    {
        popUpPanel.SetActive(false);
        // Additional cleanup or actions can be added here if needed
    }

    // Method to be called by the onClick event of the "info" button
    public void OpenPopUp()
    {
        ShowPopUp("New Movie Title", "New Genres", "New Overview");
        // You can replace the hardcoded values with actual data or parameters as needed
    }
}
