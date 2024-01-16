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

    private Dictionary<int, TMP_Dropdown> movieDropdowns = new Dictionary<int, TMP_Dropdown>();
    private Dictionary<int, int> userPreferences = new Dictionary<int, int>();
    public Sprite ClearButtonSprite;
    public GameObject firstRankItem; // Assign this in the editor
    int nbMovies; 

    public Button submitButton; // Assign this in the editor
    public TextMeshProUGUI feedbackText; // Assign a TextMeshProUGUI object in the editor
    List<int> selectedMovieIds = new List<int>();
    List<List <int>> playerMovieRankings = new List<List <int>> ();
    private List<int> grandWinners;
    void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.NetworkingClient.EventReceived += OnReceivePlayerRanking;
        }
        // Initialize winnersList with movie IDs (populate it with actual data)
        string winnersString = (string)PhotonNetwork.CurrentRoom.CustomProperties["Winners"];
        winnersList = winnersString.Split(',').Select(int.Parse).ToList();

        nbMovies = winnersList.Count;
        // Fetch details for the winnersList
        StartCoroutine(FetchMovieDetails(winnersList));
    }
    void OnMovieDetailsFetched()
    {
        // Display the first movie
        DisplayMovie();

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
    }
    #region Movie List Canvas methods
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
        public int id; // Add this line to include an ID field
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
        float startY = 67f; // Starting Y position
        float endY = -1167f; // Last possible Y position
        float spaceBetween = Mathf.Min((endY - startY) / (nbMovies - 1), 400);

        Vector3 prefabPosition = firstRankItem.transform.position; // Get the prefab's original position

        for (int i = 0; i < nbMovies; i++)
        {
            // Calculate the Y position for this clone
            float positionY = startY - (i * spaceBetween);

            // Use the prefab's x and z, and the new y for the position
            Vector3 newPosition = new Vector3(prefabPosition.x, positionY, prefabPosition.z);

            // Instantiate a new GameObject from the "Rank1" prefab
            GameObject rankClone = Instantiate(firstRankItem, newPosition, Quaternion.identity) as GameObject;
            rankClone.name = "Rank" + (i + 1); // Naming it Rank1, Rank2, etc.
            rankClone.transform.SetParent(RankingCanvas.transform, false);

            // Change the rank number
            TextMeshProUGUI numberText = rankClone.transform.Find("Number").GetComponent<TextMeshProUGUI>();
            numberText.text = (i + 1).ToString();

            // Configure the Dropdown component within the Rank1 clone if needed
            TMP_Dropdown dropdown = rankClone.GetComponentInChildren<TMP_Dropdown>();
            rankClone.gameObject.SetActive(true);
            if (dropdown != null)
            {
                Debug.Log("populating dropdown i");
                // Configure the dropdown options, listeners, etc.
                PopulateDropdownOptions(dropdown, i);
                dropdown.onValueChanged.AddListener(delegate { OnDropdownValueChanged(dropdown); });
            }

            Button clearButton = rankClone.transform.Find("Clear Button").GetComponent<Button>();
            clearButton.onClick.AddListener(delegate { ClearDropdown(dropdown); });
        }
    }

    void OnDropdownValueChanged(TMP_Dropdown changedDropdown)
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


    void ClearDropdown(TMP_Dropdown dropdown)
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

    void PopulateDropdownOptions(TMP_Dropdown dropdown, int movieIndex)
    {
        var selectedTitles = new HashSet<string>(movieDropdowns.Where(d => d.Value != dropdown)
                                            .Select(d => d.Value.options[d.Value.value].text)
                                            .Where(t => !string.IsNullOrEmpty(t) && t != "Select a movie"));

        dropdown.options.Clear();
        dropdown.options.Add(new TMP_Dropdown.OptionData("Select a movie"));

        foreach (var movieDetails in movieDetailsList)
        {
            if (!selectedTitles.Contains(movieDetails.title))
            {
                dropdown.options.Add(new TMP_Dropdown.OptionData(movieDetails.title));
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

    public void onClickSubmitButton()
    {
        bool allDropdownsSelected = true;

        foreach (var movieDropdownPair in movieDropdowns)
        {
            TMP_Dropdown dropdown = movieDropdownPair.Value;
            int selectedIndex = dropdown.value;

            if (selectedIndex > 0)
            {
                selectedMovieIds.Add(movieDetailsList[selectedIndex - 1].id);
            }
            else
            {
                allDropdownsSelected = false;
                break;
            }
        }

        if (allDropdownsSelected && selectedMovieIds.Count == nbMovies)
        {
            submitButton.interactable = false;
            // Send the list to the Photon room, potentially only to the Master Client
            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.RaiseEvent(113, selectedMovieIds.ToArray(), new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient }, new ExitGames.Client.Photon.SendOptions { Reliability = true });
            }

            foreach (var movieDropdownPair in movieDropdowns)
            {
                // Disable dropdowns to keep the visual order of the votes
                TMP_Dropdown dropdown = movieDropdownPair.Value;
                dropdown.interactable = false;
            }   
        }
        else
        {
            feedbackText.text = "You have to rank all of the movies!";
            StartCoroutine(ClearFeedback());
        }

    }

    private IEnumerator ClearFeedback()
    {
        yield return new WaitForSeconds(3); // Wait for 3 seconds
        feedbackText.text = "Rank from most to least preferred"; // Clear the feedback message
    }


    public void OnReceivePlayerRanking(ExitGames.Client.Photon.EventData photonEvent)
    {
        if (photonEvent.Code == 113)
        {
            // Store the rankings with the player's ID
            playerMovieRankings.Add(selectedMovieIds);

            // Check if all players have submitted their rankings
            if (playerMovieRankings.Count == PhotonNetwork.PlayerList.Length)
            {
                // All players have submitted their rankings
                RankedPairsVoting rankedPairs = new RankedPairsVoting();
                grandWinners = rankedPairs.GetWinners(playerMovieRankings);
            }
        }
    }
    public class RankedPairsVoting
    {
        private Dictionary<int, Dictionary<int, int>> CalculatePairwisePreferences(List<List<int>> playerMovieRankings)
        {
            var preferences = new Dictionary<int, Dictionary<int, int>>();

            foreach (var ranking in playerMovieRankings)
            {
                for (int i = 0; i < ranking.Count; i++)
                {
                    if (!preferences.ContainsKey(ranking[i]))
                        preferences[ranking[i]] = new Dictionary<int, int>();

                    for (int j = i + 1; j < ranking.Count; j++)
                    {
                        if (!preferences[ranking[i]].ContainsKey(ranking[j]))
                            preferences[ranking[i]][ranking[j]] = 0;

                        preferences[ranking[i]][ranking[j]] += 1;
                    }
                }
            }

            return preferences;
        }

        private List<Tuple<int, int, int>> SortPairs(Dictionary<int, Dictionary<int, int>> preferences)
        {
            var pairs = new List<Tuple<int, int, int>>();

            foreach (var movie1 in preferences)
            {
                foreach (var movie2 in movie1.Value)
                {
                    int opposite = preferences.ContainsKey(movie2.Key) && preferences[movie2.Key].ContainsKey(movie1.Key)
                                ? preferences[movie2.Key][movie1.Key]
                                : 0;

                    if (movie2.Value > opposite)
                    {
                        pairs.Add(new Tuple<int, int, int>(movie1.Key, movie2.Key, movie2.Value - opposite));
                    }
                }
            }

            return pairs.OrderByDescending(p => p.Item3).ToList();
        }

        private Dictionary<int, HashSet<int>> LockPairs(List<Tuple<int, int, int>> sortedPairs)
        {
            var lockedPairs = new Dictionary<int, HashSet<int>>();
            foreach (var pair in sortedPairs)
            {
                if (!CreatesCycle(lockedPairs, pair.Item1, pair.Item2))
                {
                    if (!lockedPairs.ContainsKey(pair.Item1))
                        lockedPairs[pair.Item1] = new HashSet<int>();

                    lockedPairs[pair.Item1].Add(pair.Item2);
                }
            }
            return lockedPairs;
        }

        private bool CreatesCycle(Dictionary<int, HashSet<int>> lockedPairs, int winner, int loser, int? start = null)
        {
            if (start == null) start = winner;

            if (lockedPairs.ContainsKey(loser))
            {
                if (lockedPairs[loser].Contains(start.Value)) return true;

                foreach (var nextLoser in lockedPairs[loser])
                {
                    if (CreatesCycle(lockedPairs, winner, nextLoser, start)) return true;
                }
            }

            return false;
        }

        private List<int> FindWinners(Dictionary<int, HashSet<int>> lockedPairs, HashSet<int> movieIds)
        {
            // Find all candidates who are not defeated by anyone else
            var potentialWinners = movieIds.Where(movieId => 
                lockedPairs.Values.All(losers => !losers.Contains(movieId))).ToList();

            return potentialWinners;
        }


        public List<int> GetWinners(List<List<int>> playerMovieRankings)
        {
            var movieIds = playerMovieRankings.SelectMany(x => x).ToHashSet();    
            var preferences = CalculatePairwisePreferences(playerMovieRankings);
            var sortedPairs = SortPairs(preferences);
            var lockedPairs = LockPairs(sortedPairs);
            return FindWinners(lockedPairs, movieIds);
        }
    }
}
