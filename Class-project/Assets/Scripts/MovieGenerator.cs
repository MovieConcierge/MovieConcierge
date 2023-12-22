using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class MovieGenerator : MonoBehaviour
{
    public static bool displayingTitle = false;
    public string apiKey = "81e872ea74c4fe76eed2dd856b47223d"; // Replace with your TMDb API key
    private List<int> popularMovieIds = new List<int>();

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
                            int randomMovieId = popularMovieIds[Random.Range(0, popularMovieIds.Count)];

                            // Fetch detailed information about the randomly selected movie
                            StartCoroutine(FetchMovieInformation(randomMovieId));
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
                    // Call SetMovieInformation with API response
                    SetMovieInformation(movieApiResponse.title, movieApiResponse.poster_path, movieApiResponse.overview);
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

    void SetMovieInformation(string title, string posterPath, string info)
    {
        // Update the movie information using the fetched data
        MovieDisplay.newTitle = title;

        // Construct the full poster URL using the TMDb base URL
        string posterUrl = $"https://image.tmdb.org/t/p/w500/{posterPath}";
        Debug.Log("poster url: " + posterUrl);

        StartCoroutine(FetchTexture(posterUrl));

        PopUpManager.newOverview = info;
    }

IEnumerator FetchTexture(string url)
{
    using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
    {
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            // Get the downloaded texture
            MovieDisplay.newPosterTexture = DownloadHandlerTexture.GetContent(request);
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
        // Other properties you might want to extract
    }
}
