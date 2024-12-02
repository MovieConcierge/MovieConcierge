using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Linq;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public class MovieGenerator : MonoBehaviour
{
    public string apiKey = "your_api_key"; // Replace with your TMDb API key
    public Canvas mainCanvas;
    public Canvas votingCanvas;
    private List<int> popularMovieIds = new List<int>();
    private List<string> selectedGenres = new List<string>();
    private List<int> likedMovies = new List<int>();
    private List<int> dislikedMovies = new List<int>();
    private int currentMovieId;
    public delegate void TextureFetchedAction();
    public static event TextureFetchedAction OnTextureFetched;
    private int NumberOfPages; //20 movies per page
    public int MaxLikedMovies = 8;
    private string delimiter;

    public static event System.Action OnVotingStartRequested;
    private VotingSystem votingSystem;

    void Awake()
    {
        selectedGenres = PlayerPrefs.GetString("SelectedGenres", "").Split(',').ToList();
    }

    IEnumerator Start()
    {
        PhotonNetwork.AutomaticallySyncScene = false;//Players are independent now
        NumberOfPages = PlayerPrefs.GetInt("NumberOfPages", 3);
        bool useFlexibleMatching = PlayerPrefs.GetInt("FlexibleGenreMatching", 0) == 1;
        delimiter = useFlexibleMatching ? "|" : ",";

        if (string.IsNullOrEmpty(apiKey))
        {
            Debug.LogError("TMDb API key is not set. Please provide your API key.");
            yield break;
        }

        votingSystem = FindObjectOfType<VotingSystem>();

        yield return StartCoroutine(FetchPopularMovieIds(NumberOfPages));
    }

    #region TMDB API Info Fetching
    IEnumerator FetchPopularMovieIds(int pageCount)
    {
        string selectedGenresString = string.Join(delimiter, selectedGenres);
        for (int page = 1; page <= pageCount; page++)
        {
            
            string apiUrl = $"https://api.themoviedb.org/3/discover/movie?api_key={apiKey}&language=en-US&sort_by=popularity.desc&page={page}&with_genres={selectedGenresString}";
            using (UnityWebRequest request = UnityWebRequest.Get(apiUrl))
            {
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    string jsonResult = request.downloadHandler.text;

                    PopularMoviesApiResponse popularMoviesApiResponse = JsonUtility.FromJson<PopularMoviesApiResponse>(jsonResult);

                    if (popularMoviesApiResponse != null && popularMoviesApiResponse.results != null)
                    {
                        foreach (var movie in popularMoviesApiResponse.results)
                        {
                            popularMovieIds.Add(movie.id);
                        }
                    }
                    else
                    {
                        Debug.LogError($"Error fetching popular movies on page {page}");
                    }
                }
                else
                {
                    Debug.LogError($"Error fetching popular movies on page {page}: {request.error}");
                }
            }
        }

        //selecting a random movie ID and fetching detailed information.
        if (popularMovieIds.Count > 0)
        {
            currentMovieId = popularMovieIds[Random.Range(0, popularMovieIds.Count)];
            StartCoroutine(FetchMovieInformation(currentMovieId));
        }
        else
        {
            Debug.LogError("No movies found in the popular movies list");
        }
    }

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
        MovieDisplay.newGenres = ConvertGenresToString(genres);

        // Construct the full poster URL using the TMDb base URL
        string posterUrl = $"https://image.tmdb.org/t/p/w500/{posterPath}";

        StartCoroutine(FetchTexture(posterUrl));

        // Assign the concatenated genres string to PopUpManager.newGenres
        PopUpManager.newGenres = ConvertGenresToString(genres);
        PopUpManager.newOverview = info;
        PopUpManager.newTitle = title;

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
                OnTextureFetched?.Invoke();
            }
            else
            {
                Debug.LogError("Error fetching movie poster: " + request.error);
            }
        }
    }

    #endregion

    #region Data structures
    [System.Serializable]
    public class PopularMoviesApiResponse
    {
        [System.Serializable]
        public class MovieResult
        {
            public int id;
            // Other properties you might want to extract
        }

        public List<MovieResult> results;
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

    #endregion
   
    #region Buttons
    public void OnLikeButtonClick()
    {
        // Store the current movie ID in the likedMovies list
        currentMovieId = GetCurrentMovieId();
        likedMovies.Add(currentMovieId);

        // Check if the number of liked movies reaches the maximum
        if (likedMovies.Count >= MaxLikedMovies)
        {
            // Transition to the voting canvas
            OnStopButtonPressed();
            return; // Stop further execution
        }

        // Display another movie that hasn't been liked or disliked yet
        ShowNextUnratedMovie();
    }

    private void ShowVotingCanvas()
    {
        // Disable the main canvas
        mainCanvas.gameObject.SetActive(false);

        // Enable the voting canvas
        votingCanvas.gameObject.SetActive(true);
    }

    public void OnDislikeButtonClick()
    {
        // Store the current movie ID in the dislikedMovies list
        int currentMovieId = GetCurrentMovieId();
        dislikedMovies.Add(currentMovieId);

        // Check if the number of liked movies reaches 4
        if (likedMovies.Count >= MaxLikedMovies)
        {
            // Transition to the "Voting" scene
            OnStopButtonPressed();
            return; // Stop further execution
        }
        // Display another movie that hasn't been liked or disliked yet
        ShowNextUnratedMovie();
    }

    public void OnStopButtonPressed()
    {
        VotingSystem.LikedMovies = likedMovies;
        // Notify subscribers (e.g., VotingSystem) that voting should start
        OnVotingStartRequested?.Invoke();

        // Set movie information before transitioning to voting canvas
        ShowVotingCanvas();
    }
    #endregion

    #region Movie transition methods
    private int GetCurrentMovieId()
    {
        // Return the stored current movie ID
        return currentMovieId;
    }

    private void ShowNextUnratedMovie()
    {
        // Fetch the list of 50 popular movies
        List<int> unratedMovies = GetUnratedMovies();

        // Find a movie that hasn't been rated yet
        currentMovieId = FindNextUnratedMovie(unratedMovies);

        // Fetch detailed information about the next movie
        StartCoroutine(FetchMovieInformation(currentMovieId));
    }

    private List<int> GetUnratedMovies()
    {
        // Remove liked and disliked movies from the list
        List<int> unratedMovies = popularMovieIds.Except(likedMovies).Except(dislikedMovies).ToList();
        return unratedMovies;
    }

    private int FindNextUnratedMovie(List<int> unratedMovies)
    {
        // Select a random movie from the list of unrated movies
        int nextMovieId = unratedMovies[UnityEngine.Random.Range(0, unratedMovies.Count)];
        return nextMovieId;
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

}
