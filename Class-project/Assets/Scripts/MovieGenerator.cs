using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;

public class MovieGenerator : MonoBehaviour
{
    public static bool displayingTitle = false;
    public string apiKey = "81e872ea74c4fe76eed2dd856b47223d"; // Replace with your TMDb API key
    private List<int> popularMovieIds = new List<int>();
    private List<int> likedMovies = new List<int>();
    private List<int> dislikedMovies = new List<int>();
    private int currentMovieId;
    public delegate void TextureFetchedAction();
    public static event TextureFetchedAction OnTextureFetched;

    void Start()
    {
        if (string.IsNullOrEmpty(apiKey))
        {
            Debug.LogError("TMDb API key is not set. Please provide your API key.");
            return;
        }

        StartCoroutine(FetchPopularMovieIds());
    }

    IEnumerator FetchPopularMovieIds()
    {
        // Fetch the list of the 50 most popular movies
        string apiUrl = $"https://api.themoviedb.org/3/movie/popular?api_key={apiKey}&language=en-US&page=1";
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

                        if (popularMovieIds.Count > 0)
                        {
                            // Select a random movie ID
                            currentMovieId = popularMovieIds[Random.Range(0, popularMovieIds.Count)];

                            // Fetch detailed information about the randomly selected movie
                            StartCoroutine(FetchMovieInformation(currentMovieId));
                        }
                        else
                        {
                            Debug.LogError("No movies found in the popular movies list");
                        }

                        }
                        else
                        {
                            Debug.LogError("Error fetching popular movie IDs");
                        }
            }
            else
            {
                Debug.LogError("Error fetching popular movie IDs: " + request.error);
            }
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

        // Construct the full poster URL using the TMDb base URL
        string posterUrl = $"https://image.tmdb.org/t/p/w500/{posterPath}";

        StartCoroutine(FetchTexture(posterUrl));

        // Concatenate genres into a single string
        string genresString = "";
        foreach (var genre in genres)
        {
            genresString += genre.name + ", ";
        }

        // Remove the trailing comma and space
        genresString = genresString.TrimEnd(',', ' ');

        // Assign the concatenated genres string to PopUpManager.newGenres
        PopUpManager.newGenres = genresString;

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
                OnTextureFetched?.Invoke();
            }
            else
            {
                Debug.LogError("Error fetching movie poster: " + request.error);
            }
        }
    }


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


    public void OnLikeButtonClick()
    {
        // Store the current movie ID in the likedMovies list
        int currentMovieId = GetCurrentMovieId();
        likedMovies.Add(currentMovieId);

        // Display another movie that hasn't been liked or disliked yet
        ShowNextUnratedMovie();
    }

    public void OnDislikeButtonClick()
    {
        // Store the current movie ID in the dislikedMovies list
        int currentMovieId = GetCurrentMovieId();
        dislikedMovies.Add(currentMovieId);

        // Display another movie that hasn't been liked or disliked yet
        ShowNextUnratedMovie();
    }

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
        int nextMovieId = FindNextUnratedMovie(unratedMovies);

        // Fetch detailed information about the next movie
        StartCoroutine(FetchMovieInformation(nextMovieId));
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
        int nextMovieId = unratedMovies[Random.Range(0, unratedMovies.Count)];
        return nextMovieId;
    }

}