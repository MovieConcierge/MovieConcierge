using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GenreSettings : MonoBehaviour
{
    public Toggle[] genreToggles; // Assign toggles in the Inspector
    public Button confirmButton;

    private List<string> selectedGenres = new List<string>();

    private void Start()
    {
        confirmButton.onClick.AddListener(OnConfirmButtonClicked);
    }

    private void OnConfirmButtonClicked()
    {
        // Collect selected genres
        selectedGenres.Clear();

        foreach (Toggle toggle in genreToggles)
        {
            if (toggle.isOn)
            {
                // Assuming toggle text is the genre name
                string genreName = toggle.GetComponentInChildren<Text>().text;

                // Map the genre name to its ID
                if (genreNameToIdMap.TryGetValue(genreName, out int genreId))
                {
                    selectedGenres.Add(genreId.ToString());
                }
            }
        }
        Debug.Log(selectedGenres.Count);

        // Now, you can save the selectedGenres list using PlayerPrefs or perform any other actions.
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