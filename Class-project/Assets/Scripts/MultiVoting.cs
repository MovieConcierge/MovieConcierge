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
    public Sprite ClearButtonSprite;
    public GameObject rankPrefab; // Assign this in the editor
    int nbMovies; 

    void Start()
    {
        // Initialize winnersList with movie IDs (populate it with actual data)
        string winnersString = (string)PhotonNetwork.CurrentRoom.CustomProperties["Winners"];
        winnersList = winnersString.Split(',').Select(int.Parse).ToList();

        nbMovies = winnersList.Count;
        // Fetch details for the winnersList
        StartCoroutine(FetchMovieDetails(winnersList));
    }

    #region Movie List Canvas methods

    void OnMovieDetailsFetched()
    {
        // Display the first movie
        DisplayMovie();

        Debug.Log("start dropdowns");
        onClickGoRank(); //Go to the ranking canvas to setup the dropdowns
        InstantiateDropdowns();
        foreach (var dropdownPair in movieDropdowns)
        {
            dropdownPair.Value.onValueChanged.AddListener(delegate { OnDropdownValueChanged(dropdownPair.Value); });
        }
        // Populate each dropdown initially
        foreach (var dropdownPair in movieDropdowns)
        {
            PopulateDropdownOptions(dropdownPair.Value, dropdownPair.Key);
        }

        Debug.Log("finished dropdowns");
    }
    int mod(int x, int m)  
    {
        return (x%m + m)%m;
    }
    public void ShowNextMovie()
    {
        currentIndex = mod((currentIndex + 1), nbMovies);
        DisplayMovie();
    }

    public void ShowPreviousMovie()
    {
        currentIndex = mod((currentIndex - 1), nbMovies);
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
        OnMovieDetailsFetched();
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
        float startY = 700f; // Starting Y position
        float spaceBetween = 80f; // Space between dropdowns
        //float width = 700f;
        //float height = 80f;

    for (int i = 0; i < nbMovies; i++)
    {
        // Calculate the Y position for this clone
        float positionY = startY - (i * spaceBetween);

        // Instantiate a new GameObject from the "Rank1" prefab
        GameObject rankClone = Instantiate(rankPrefab, Vector3.zero, Quaternion.identity) as GameObject;
        rankClone.name = "Rank" + (i + 1); // Naming it Rank1, Rank2, etc.
        rankClone.transform.SetParent(RankingCanvas.transform, false);

        // Configure the Dropdown component within the Rank1 clone if needed
        Dropdown dropdown = rankClone.GetComponentInChildren<Dropdown>();
        if (dropdown != null)
        {
            // Configure the dropdown options, listeners, etc.
            PopulateDropdownOptions(dropdown, i);
            dropdown.onValueChanged.AddListener(delegate { OnDropdownValueChanged(dropdown); });
        }

        // Other configurations specific to the "Rank1" prefab can be set here
    }
    }

    void OnDropdownValueChanged(Dropdown changedDropdown)
    {
        // Update user preferences
        int movieIndex = movieDropdowns.FirstOrDefault(x => x.Value == changedDropdown).Key;
        userPreferences[movieIndex] = changedDropdown.value;

        // Repopulate all dropdowns
        foreach (var dropdown in movieDropdowns)
        {
            PopulateDropdownOptions(dropdown.Value, dropdown.Key);
        }
    }


    void ClearDropdown(Dropdown dropdown)
    {
        // Clear the selected option in the dropdown
        dropdown.value = 0;
        dropdown.RefreshShownValue(); // Update the shown value immediately
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
        var selectedTitles = new HashSet<string>(movieDropdowns.Where(d => d.Value != dropdown)
                                            .Select(d => d.Value.options[d.Value.value].text)
                                            .Where(t => !string.IsNullOrEmpty(t) && t != "Select a movie"));

        dropdown.options.Clear();
        dropdown.options.Add(new Dropdown.OptionData("Select a movie"));

        foreach (var movieDetails in movieDetailsList)
        {
            if (!selectedTitles.Contains(movieDetails.title))
            {
                dropdown.options.Add(new Dropdown.OptionData(movieDetails.title));
            }
        }

        // Ensure the current value is correctly set
        dropdown.value = 0;
        if (userPreferences.ContainsKey(movieIndex))
        {
            var currentSelection = movieDetailsList[userPreferences[movieIndex]].title;
            for (int i = 0; i < dropdown.options.Count; i++)
            {
                if (dropdown.options[i].text == currentSelection)
                {
                    dropdown.value = i;
                    break;
                }
            }
        }

        dropdown.RefreshShownValue();
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
