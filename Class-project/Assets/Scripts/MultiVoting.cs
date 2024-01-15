using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Linq;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class MultiVoting : MonoBehaviour
{
    public string apiKey = "81e872ea74c4fe76eed2dd856b47223d"; // Replace with your TMDb API key
    public Canvas MovieViewCanvas;
    public Canvas RankingCanvas;

    private List<int> winnersList; // List of movie IDs
    private List<MovieDetails> movieDetailsList = new List<MovieDetails>(); // List of MovieDetails
    public delegate void TextureFetchedMultiAction();
    public static event TextureFetchedMultiAction OnTextureFetchedMulti;
    private int currentIndex = 0;

    private Dictionary<int, Dropdown> movieDropdowns = new Dictionary<int, Dropdown>();
    private Dictionary<int, int> userPreferences = new Dictionary<int, int>();

    void Start()
    {
        // Initialize winnersList with movie IDs (populate it with actual data)
        string winnersString = (string)PhotonNetwork.CurrentRoom.CustomProperties["Winners"];
        winnersList = new List<int>(Array.ConvertAll(winnersString.Split(','), int.Parse));

        Debug.Log(winnersList.Count);
        Debug.Log(winnersList[0]);
        Debug.Log(winnersList[1]);

        // Fetch details for the winnersList
        StartCoroutine(FetchMovieDetails(winnersList));

        // Display the first movie
        DisplayMovie();

        InstantiateDropdowns();
    }

    #region Movie List Canvas methods

    public void ShowNextMovie()
    {
        currentIndex = (currentIndex + 1) % winnersList.Count;
        DisplayMovie();
    }

    public void ShowPreviousMovie()
    {
        currentIndex = (currentIndex - 1 + winnersList.Count) % winnersList.Count;
        DisplayMovie();
    }

    void DisplayMovie()
    {
        MovieDetails currentMovie = movieDetailsList[currentIndex];

        SetMovieInformation(currentMovie.title,currentMovie.poster_path,currentMovie.overview,currentMovie.genres);
    }

    void SetMovieInformation(string title, string posterPath, string info, List<Genre> genres)
    {
        // Update the movie information using the fetched data
        MovieDisplay.newTitle = title;

        // Construct the full poster URL using the TMDb base URL
        string posterUrl = $"https://image.tmdb.org/t/p/w500/{posterPath}";
        StartCoroutine(FetchTexture(posterUrl));

        // Assign the concatenated genres string to PopUpManager.newGenres
        PopUpManager.newGenres = ConvertGenresToString(genres);
        PopUpManager.newOverview = info;

    }

    IEnumerator FetchMovieDetails(List<int> movieIds)
    {
        foreach (int movieId in movieIds)
        {
            string apiUrl = $"https://api.themoviedb.org/3/movie/{movieId}?api_key={apiKey}&language=en-US";
            using (UnityWebRequest request = UnityWebRequest.Get(apiUrl))
            {
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    string jsonResult = request.downloadHandler.text;
                    MovieDetails movieDetails = JsonUtility.FromJson<MovieDetails>(jsonResult);
                    movieDetailsList.Add(movieDetails);
                }
                else
                {
                    Debug.LogError("Error fetching movie details for ID " + movieId + ": " + request.error);
                }
            }
        }
    }

    IEnumerator FetchTexture(string url)
    {
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                MovieDisplay.newPosterTexture = DownloadHandlerTexture.GetContent(request);

                // Notify that the texture has been fetched
                OnTextureFetchedMulti?.Invoke();
            }
            else
            {
                Debug.LogError("Error fetching movie poster: " + request.error);
            }
        }
    }

    public void OnClickReturnToMenuButton()
    {
        PhotonNetwork.Disconnect();
        SceneManager.LoadScene("Title");
    }
    public class MovieDetails
    {
        public string title;
        public string overview;
        public string poster_path;
        public List<Genre> genres;
    }
    [Serializable]
    public class Genre
    {
        public int id;
        public string name;
    }
    
    string ConvertGenresToString(List<Genre> genres)
    {
        string genresString = "";

        foreach (var genre in genres)
        {
            genresString += genre.name + ", ";
        }

        genresString = genresString.TrimEnd(',', ' ');

        return genresString;
    }

    public void onClickGoRank()
    {
        RankingCanvas.gameObject.SetActive(true);
        MovieViewCanvas.gameObject.SetActive(false);
    }
    public void onClickGoView()
    {
        MovieViewCanvas.gameObject.SetActive(true);
        RankingCanvas.gameObject.SetActive(false);
    }

    #endregion
    void InstantiateDropdowns()
    {
        for (int i = 0; i < winnersList.Count; i++)
        {
            // Instantiate a new Dropdown
            GameObject dropdownGO = new GameObject("Dropdown" + i);
            RectTransform dropdownTransform = dropdownGO.AddComponent<RectTransform>();
            dropdownTransform.SetParent(transform); // Parent it to the MultiVoting script's GameObject
            dropdownTransform.anchoredPosition = new Vector2(0, 700 - i * 80); // Adjust as needed
            dropdownTransform.sizeDelta = new Vector2(700, 80); // Adjust as needed

            Dropdown dropdown = dropdownGO.AddComponent<Dropdown>();

            // Create a new text component for the ranking number
            TextMeshProUGUI rankingNumberText = new GameObject("RankingNumber" + i).AddComponent<TextMeshProUGUI>();
            rankingNumberText.transform.SetParent(dropdownGO.transform);
            rankingNumberText.text = (i + 1).ToString(); // Display the ranking number
            rankingNumberText.alignment = TextAlignmentOptions.Center;
            rankingNumberText.fontSize = 30;

            // Add a Clear Button next to the dropdown
            Button clearButton = new GameObject("ClearButton" + i).AddComponent<Button>();
            RectTransform clearButtonTransform = clearButton.GetComponent<RectTransform>();
            clearButtonTransform.SetParent(dropdownGO.transform);
            clearButtonTransform.anchoredPosition = new Vector2(600, 0); // Adjust as needed
            clearButtonTransform.sizeDelta = new Vector2(100, 30);

            TextMeshProUGUI buttonText = new GameObject("ClearButtonText" + i).AddComponent<TextMeshProUGUI>();
            buttonText.transform.SetParent(clearButton.transform);
            buttonText.text = "Clear";
            buttonText.alignment = TextAlignmentOptions.Center;
            buttonText.fontSize = 30;
            buttonText.rectTransform.sizeDelta = new Vector2(100, 30);
            clearButton.onClick.AddListener(() => ClearDropdown(dropdown));

            // Populate dropdown options with movie titles
            PopulateDropdownOptions(dropdown, i);

            // Add listener for dropdown value changes
            dropdown.onValueChanged.AddListener(delegate { OnDropdownValueChanged(dropdown); });

            // Keep track of dropdown instances
            movieDropdowns.Add(i, dropdown);
        }

        // Add a global Clear All Button
        Button clearAllButton = new GameObject("ClearAllButton").AddComponent<Button>();
        RectTransform clearAllButtonTransform = clearAllButton.GetComponent<RectTransform>();
        clearAllButtonTransform.SetParent(transform);
        clearAllButtonTransform.anchoredPosition = new Vector2(600, -990); // Adjust as needed
        clearAllButtonTransform.sizeDelta = new Vector2(100, 30);

        TextMeshProUGUI clearAllButtonText = new GameObject("ClearAllButtonText").AddComponent<TextMeshProUGUI>();
        clearAllButtonText.transform.SetParent(clearAllButton.transform);
        clearAllButtonText.text = "Clear All";
        clearAllButtonText.alignment = TextAlignmentOptions.Center;
        clearAllButtonText.fontSize = 30;
        clearAllButton.onClick.AddListener(ClearAllDropdowns);
    }



    void ClearDropdown(Dropdown dropdown)
    {
        // Clear the selected option in the dropdown
        dropdown.value = 0;
    }

    void ClearAllDropdowns()
    {
        // Clear all dropdowns
        foreach (var dropdown in movieDropdowns.Values)
        {
            dropdown.value = 0;
        }
    }


    void PopulateDropdownOptions(Dropdown dropdown, int movieIndex)
    {
        // Ensure you have access to movieDetailsList or adjust based on your data structure
        if (movieDetailsList.Count > movieIndex)
        {
            MovieDetails movieDetails = movieDetailsList[movieIndex];
            dropdown.options.Clear();

            // Add movie titles to dropdown options
            foreach (var genre in movieDetails.genres)
            {
                dropdown.options.Add(new Dropdown.OptionData(genre.name));
            }
        }
    }

    void OnDropdownValueChanged(Dropdown dropdown)
    {
        // Get the movie index associated with this dropdown
        int movieIndex = movieDropdowns.FirstOrDefault(x => x.Value == dropdown).Key;

        // Update user preferences
        if (userPreferences.ContainsKey(movieIndex))
        {
            userPreferences[movieIndex] = dropdown.value;
        }
        else
        {
            userPreferences.Add(movieIndex, dropdown.value);
        }
    }

    public void OnClickSubmitButton()
    {
        // Process user preferences (e.g., send to server, save locally, etc.)
        // For now, let's just print the preferences to the console
        foreach (var entry in userPreferences)
        {
            Debug.Log($"Movie Index: {entry.Key}, User Preference: {entry.Value}");
        }
    }
}
