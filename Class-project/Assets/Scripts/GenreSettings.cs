using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GenreSettings : MonoBehaviour
{
    public Canvas mainScreenCanvas;
    public Canvas genresCanvas;
    public Toggle[] genreToggles; // Assign toggles in the Inspector
    public Button confirmButton;
    public Button selectAllButton;
    public Toggle flexibleGenreMatchingToggle;
    public TMP_InputField numberOfPages;
    private List<string> selectedGenres = new List<string>();

    private void Start()
    {
        ShowMainScreen();

        selectAllButton.onClick.AddListener(ToggleAllGenres);
        confirmButton.onClick.AddListener(OnConfirmButtonClicked);
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

    public void OnConfirmButtonClicked()
    {
        // Collect selected genres
        selectedGenres.Clear();

        foreach (Toggle toggle in genreToggles)
        {
            if (toggle.isOn)
            {
                string genreName = toggle.GetComponentInChildren<Text>().text;

                // Map the genre name to its ID
                if (genreNameToIdMap.TryGetValue(genreName, out int genreId))
                {
                    selectedGenres.Add(genreId.ToString());
                }
            }
        }
        PlayerPrefs.SetInt("FlexibleGenreMatching", flexibleGenreMatchingToggle.isOn ? 1 : 0);

        int numberOfPagesInt; 
        if(int.TryParse(numberOfPages.text, out numberOfPagesInt))
        {
            PlayerPrefs.SetInt("NumberOfPages", numberOfPagesInt);
        }
        else
        {
            // Use 3 as the default number of pages to browse
            PlayerPrefs.SetInt("NumberOfPages", 3);
        }
        SaveSelectedGenres();
    }

    private void SaveSelectedGenres()
    {
        if (selectedGenres.Count > 0)
        {
            string selectedGenresString = string.Join(",", selectedGenres.ToArray());
            PlayerPrefs.SetString("SelectedGenres", selectedGenresString);

        }
        else
        {
            string selectedGenresString = "";
            PlayerPrefs.SetString("SelectedGenres", selectedGenresString);  
        }
        ShowMainScreen();
    }

    private void ToggleAllGenres()
    {
        // Check if all toggles are currently on
        bool allTogglesAreOn = genreToggles.All(toggle => toggle.isOn);

        // Set all toggles to true if any toggle is off, otherwise set all to false
        bool newToggleState = !allTogglesAreOn;
        foreach (Toggle toggle in genreToggles)
        {
            toggle.isOn = newToggleState;
        }
    }

    Dictionary<string, int> genreNameToIdMap = new Dictionary<string, int>
    {
        {"Action", 28},
        {"Adventure", 12},
        {"Animation", 16},
        {"Comedy", 35},
        {"Crime", 80},
        {"Documentary", 99},
        {"Drama", 18},
        {"Family", 10751},
        {"Fantasy", 14},
        {"History", 36},
        {"Horror", 27},
        {"Music", 10402},
        {"Mystery", 9648},
        {"Romance", 10749},
        {"Science Fiction", 878},
        {"TV Movie", 10770},
        {"Thriller", 53},
        {"War", 10752},
        {"Western", 37},
    };
};