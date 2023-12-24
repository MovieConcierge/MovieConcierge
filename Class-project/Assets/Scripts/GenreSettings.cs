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
                selectedGenres.Add(toggle.GetComponentInChildren<Text>().text);
            }
        }

        // Pass selected genres to another script or store them for later use
        // (You can use PlayerPrefs, scriptable objects, etc.)
    }
}
