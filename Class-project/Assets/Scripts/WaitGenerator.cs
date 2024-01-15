using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Linq;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class WaitGenerator : MonoBehaviourPunCallbacks
{
    public string apiKey = "81e872ea74c4fe76eed2dd856b47223d"; // Replace with your TMDb API key
    private List<string> selectedGenres = new List<string>();
    public delegate void TextureFetchedWaitAction();
    public static event TextureFetchedWaitAction OnTextureFetchedWait;

    public GameObject SceneTitle;
    private int winnerId;
    private int eventwinnerId;
    private bool multiGame;

    public List<int> winners = new List<int>();


    void Awake()
    {
        selectedGenres = PlayerPrefs.GetString("SelectedGenres", "").Split(',').ToList();
        multiGame = PlayerPrefs.GetInt("multiGame", 0) == 1;
    }

    IEnumerator Start()
    {
        PhotonNetwork.NetworkingClient.EventReceived += OnPhotonEvent;

        FetchWinnersListFromRoomProperties();

        if (string.IsNullOrEmpty(apiKey))
        {
            Debug.LogError("TMDb API key is not set. Please provide your API key.");
            yield break;
        }
        winnerId = PlayerPrefs.GetInt("WinnerId", -1);

        if (winnerId == -1)
        {
            Debug.LogError("Winner ID not found in PlayerPrefs");
        }

        yield return StartCoroutine(OnTournamentWinnerSelected(winnerId));
    }
    #region MovieGenerator

    IEnumerator FetchMovieInformation(int movieId)
    {
        // Fetch detailed information about the selected movie
        string apiUrl = $"https://api.themoviedb.org/3/movie/{movieId}?api_key={apiKey}&language=en-US";
        using (UnityWebRequest request = UnityWebRequest.Get(apiUrl))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonResult = request.downloadHandler.text;
                MovieApiResponse movieApiResponse = JsonUtility.FromJson<MovieApiResponse>(jsonResult);

                if (movieApiResponse != null)
                {
                    // Extract "overview" and "genres" from the API response
                    string overview = movieApiResponse.overview;
                    List<MovieApiResponse.Genre> genres = movieApiResponse.genres;

                    // Call SetMovieInformation with API response
                    SetMovieInformation(movieApiResponse.title, movieApiResponse.poster_path, overview, genres);
                }
                else
                {
                    Debug.LogError("Error fetching movie information for ID " + movieId);
                }
            }
            else
            {
                Debug.LogError("Error fetching movie information for ID " + movieId + ": " + request.error);
            }
        }
    }

    void SetMovieInformation(string title, string posterPath, string info, List<MovieApiResponse.Genre> genres)
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

    IEnumerator FetchTexture(string url)
    {
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                MovieDisplay.newPosterTexture = DownloadHandlerTexture.GetContent(request);

                // Notify that the texture has been fetched
                OnTextureFetchedWait?.Invoke();
            }
            else
            {
                Debug.LogError("Error fetching movie poster: " + request.error);
            }
        }
    }
    
    [System.Serializable]
    public class MovieApiResponse
    {
        public string title;
        public string poster_path;
        public string overview;
        public List<Genre> genres; // Add a property for genres
        // Other properties you might want to extract
        [System.Serializable]
        public class Genre
        {
            public int id;
            public string name;
        }
    }
    string ConvertGenresToString(List<MovieApiResponse.Genre> genres)
    {
        string genresString = "";

        foreach (var genre in genres)
        {
            genresString += genre.name + ", ";
        }

        // Remove the trailing comma and space
        genresString = genresString.TrimEnd(',', ' ');

        return genresString;
    }
    #endregion

    IEnumerator OnTournamentWinnerSelected(int winnerId)
    {
        if (multiGame)
        {
            SceneTitle.GetComponent<TextMeshProUGUI>().text = "Waiting Room don't leave!";
        }
        else
        {
            SceneTitle.GetComponent<TextMeshProUGUI>().text = "Movie Selected";
        }

        SceneTitle.gameObject.SetActive(true);
        StartCoroutine(FetchMovieInformation(winnerId));

        yield return null;

    }
    public void OnClickExitButton()
    {
        if (multiGame)
        {
            PhotonNetwork.LeaveRoom();
        }
        SceneTitle.gameObject.SetActive(false);
        SceneManager.LoadScene("Title");
    }

    #region Photon check if all players finished voting 
    public void OnDestroy()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= OnPhotonEvent;
    }

    private void FetchWinnersListFromRoomProperties()
    {
        if (PhotonNetwork.InRoom)
        {
            if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("Winners", out object winnersObj))
            {
                string winnersString = (string)winnersObj;
                winners = winnersString.Split(',')
                .Where(s => int.TryParse(s, out _))
                .Select(int.Parse)
                .ToList();
            }
        }
    }
    public void OnPhotonEvent(ExitGames.Client.Photon.EventData photonEvent)
    {
        if (photonEvent.Code == 159)
        {
            int eventwinnerId = (int)photonEvent.CustomData;
            if (!winners.Contains(eventwinnerId))
            {
                winners.Add(eventwinnerId);

                // Update the custom property on the Photon room
                UpdateWinnersCustomProperty();
            }
        }
    }

    public void UpdateWinnersCustomProperty()
    {
        // Convert the winners list to a format that can be easily sent through Photon (e.g., a string)
        string winnersString = string.Join(",", winners.Select(w => w.ToString()).ToArray());

        // Set the custom property on the Photon room
        var webFlags = new WebFlags(0x1); // WebFlags.HttpForward
        PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { {"Winners", winnersString} }, null, webFlags);
    }

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        Debug.Log("WAITROOM OnRoomPropertiesUpdate");
        // Check if the "Winners" custom property has changed
        if (propertiesThatChanged.ContainsKey("Winners"))
        {
            // Extract the updated winners string and update the local list
            string winnersString = (string)propertiesThatChanged["Winners"];
            winners = winnersString.Split(',').Select(int.Parse).ToList();

            if (winners.Count == PhotonNetwork.CurrentRoom.PlayerCount)
            {
                //Remove duplicates
                winners = winners.Distinct().ToList();
                winnersString = string.Join(",", winners.Select(w => w.ToString()).ToArray());
                PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { {"Winners", winnersString} });

                PhotonNetwork.LoadLevel("MultiVoting");
            }
        }
    }
    #endregion
}
