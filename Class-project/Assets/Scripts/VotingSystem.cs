using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Linq;
using TMPro;

public class VotingSystem : MonoBehaviour
{
    public string apiKey = "81e872ea74c4fe76eed2dd856b47223d"; // Replace with your TMDb API key
    public static List<int> LikedMovies;  // Assuming this is the list of liked movie IDs from MovieGenerator
    private List<MovieMatchup> matchups = new List<MovieMatchup>();
    private int currentMatchIndex = 0;

    public GameObject movieATitle;
    public GameObject movieAGenres;
    public GameObject movieAPoster;
    public Button movieAButton;

    public GameObject movieBTitle;
    public GameObject movieBGenres;
    public GameObject movieBPoster;
    public Button movieBButton;

    private MovieGenerator movieGenerator; // Reference to the MovieGenerator script
    public static Texture2D newPosterTexture;

    void Start()
    {
        movieGenerator = FindObjectOfType<MovieGenerator>(); // Find the MovieGenerator script in the scene

        // Subscribe to the event to start voting when requested
        MovieGenerator.OnVotingStartRequested += StartVotingIfRequested;
    }

    void StartVotingIfRequested()
    {
        // Check if LikedMovies is not null and has items
        if (LikedMovies != null && LikedMovies.Count > 0)
        {
            CreateMatchups();
            SetMatchup();
        }
        else
        {
            Debug.LogError("LikedMovies is null or empty. Make sure it is properly initialized in MovieGenerator.");
        }
    }

    void CreateMatchups()
    {
        // Generate matchups based on the liked movies
        List<int> remainingMovies = new List<int>(LikedMovies);
        while (remainingMovies.Count >= 2)
        {
            int movieA = remainingMovies[UnityEngine.Random.Range(0, remainingMovies.Count)];
            remainingMovies.Remove(movieA);

            int movieB = remainingMovies[UnityEngine.Random.Range(0, remainingMovies.Count)];
            remainingMovies.Remove(movieB);

            matchups.Add(new MovieMatchup(movieA, movieB));
        }
    }

    void SetMatchup()
    {
        if (currentMatchIndex < matchups.Count)
        {
            // Get the current matchup
            MovieMatchup currentMatchup = matchups[currentMatchIndex];

            // Set the UI elements for movie A based on the current matchup
            FetchMovieInformation(currentMatchup.movieA, currentMatchup.movieB);

            // Enable buttons
            movieAButton.interactable = true;
            movieBButton.interactable = true;
        }
        else
        {
            Debug.Log("Voting process completed. No more matchups available.");
            DetermineWinner();
        }
    }

    void SetMovieUI(MovieApiResponse movieA, MovieApiResponse movieB)
    {
        // Set the UI elements for movie A
        movieATitle.GetComponent<TextMeshProUGUI>().text = movieA.title;
        movieAGenres.GetComponent<TextMeshProUGUI>().text = ConvertGenresToString(movieA.genres);

        StartCoroutine(FetchTexture($"https://image.tmdb.org/t/p/w500/{movieA.poster_path}", texture =>
        {
            movieAPoster.GetComponent<Image>().sprite = CreateSpriteFromTexture(texture, movieAPoster.GetComponent<RectTransform>());
        }));

        // Set the UI elements for movie B
        movieBTitle.GetComponent<TextMeshProUGUI>().text = movieB.title;
        movieBGenres.GetComponent<TextMeshProUGUI>().text = ConvertGenresToString(movieB.genres);

        StartCoroutine(FetchTexture($"https://image.tmdb.org/t/p/w500/{movieB.poster_path}", texture =>
        {
            movieBPoster.GetComponent<Image>().sprite = CreateSpriteFromTexture(texture, movieBPoster.GetComponent<RectTransform>());
        }));
    }

    IEnumerator FetchMovieInformation(int movieAId, int movieBId)
    {
        // Fetch detailed information about movie A
        string apiUrlA = $"https://api.themoviedb.org/3/movie/{movieAId}?api_key={apiKey}&language=en-US";
        using (UnityWebRequest requestA = UnityWebRequest.Get(apiUrlA))
        {
            yield return requestA.SendWebRequest();

            if (requestA.result == UnityWebRequest.Result.Success)
            {
                string jsonResultA = requestA.downloadHandler.text;
                MovieApiResponse movieApiResponseA = JsonUtility.FromJson<MovieApiResponse>(jsonResultA);

                if (movieApiResponseA != null)
                {
                    // Fetch detailed information about movie B
                    string apiUrlB = $"https://api.themoviedb.org/3/movie/{movieBId}?api_key={apiKey}&language=en-US";
                    using (UnityWebRequest requestB = UnityWebRequest.Get(apiUrlB))
                    {
                        yield return requestB.SendWebRequest();

                        if (requestB.result == UnityWebRequest.Result.Success)
                        {
                            string jsonResultB = requestB.downloadHandler.text;
                            MovieApiResponse movieApiResponseB = JsonUtility.FromJson<MovieApiResponse>(jsonResultB);

                            if (movieApiResponseB != null)
                            {
                                // Call SetMovieUI with API responses
                                SetMovieUI(movieApiResponseA, movieApiResponseB);
                            }
                            else
                            {
                                Debug.LogError("Error fetching movie information for ID " + movieBId);
                            }
                        }
                        else
                        {
                            Debug.LogError("Error fetching movie information for ID " + movieBId + ": " + requestB.error);
                        }
                    }
                }
                else
                {
                    Debug.LogError("Error fetching movie information for ID " + movieAId);
                }
            }
            else
            {
                Debug.LogError("Error fetching movie information for ID " + movieAId + ": " + requestA.error);
            }
        }
    }

    IEnumerator FetchTexture(string url, Action<Texture> callback)
    {
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Texture texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
                callback?.Invoke(texture);
            }
            else
            {
                Debug.LogError("Error fetching movie poster: " + request.error);
            }
        }
    }


    Sprite CreateSpriteFromTexture(Texture texture, RectTransform rectTransform)
    {
        // Retrieve target dimensions from RectTransform
        int targetWidth = Mathf.RoundToInt(rectTransform.rect.width);
        int targetHeight = Mathf.RoundToInt(rectTransform.rect.height);

        Texture2D convertedTexture = new Texture2D(texture.width, texture.height);
        Graphics.CopyTexture(texture, convertedTexture);
        // Resize the texture to the target dimensions
        Texture2D resizedTexture = ResizeTexture(convertedTexture, targetWidth, targetHeight);

        // Create a sprite from the resized texture
        Sprite sprite = Sprite.Create(
            resizedTexture,
            new Rect(0, 0, resizedTexture.width, resizedTexture.height),
            new Vector2(0.5f, 0.5f));

        return sprite;
    }
    Texture2D ResizeTexture(Texture2D sourceTexture, int targetWidth, int targetHeight)
    {
        RenderTexture rt = RenderTexture.GetTemporary(targetWidth, targetHeight);
        rt.filterMode = FilterMode.Bilinear;
        RenderTexture.active = rt;

        Graphics.Blit(sourceTexture, rt);

        Texture2D resizedTexture = new Texture2D(targetWidth, targetHeight);
        resizedTexture.ReadPixels(new Rect(0, 0, targetWidth, targetHeight), 0, 0);
        resizedTexture.Apply();

        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);

        return resizedTexture;
    }

    void DetermineWinner()
    {

        Debug.Log($"CurrentMatchIndex: {currentMatchIndex}, Matchups Count: {matchups.Count}");

        // Implement logic to determine the winner based on user choices
        // For simplicity, let's assume the movie with more votes wins

        MovieMatchup lastMatchup = matchups[currentMatchIndex - 1];
        int winnerId = (UnityEngine.Random.Range(0, 2) == 0) ? lastMatchup.movieA : lastMatchup.movieB;

        // Now you have the winner (use this information as needed)

        // For now, let's just print the winner's title
        Debug.Log($"Winner ID: {winnerId}");

        // You can implement further logic based on the winner, e.g., load the chosen movie scene

        // Disable buttons to prevent further voting
        movieAButton.interactable = false;
        movieBButton.interactable = false;
    }

    public void OnMovieAButtonClick()
    {
        RecordVote(matchups[currentMatchIndex].movieA);
    }

    public void OnMovieBButtonClick()
    {
        RecordVote(matchups[currentMatchIndex].movieB);
    }

    void RecordVote(int chosenMovieId)
    {
        // Record the user's choice (you can implement more complex logic here if needed)

        // Disable buttons to prevent further voting for this matchup
        movieAButton.interactable = false;
        movieBButton.interactable = false;

        // Move to the next matchup
        currentMatchIndex++;
        SetMatchup();
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

    [Serializable]
    public class MovieMatchup
    {
        public int movieA;
        public int movieB;

        public MovieMatchup(int a, int b)
        {
            movieA = a;
            movieB = b;
        }
    }

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
}