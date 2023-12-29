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
    private Queue<Matchup> matchups = new Queue<Matchup>();
    private Queue<Matchup> losersBracket = new Queue<Matchup>();
    private int roundCount = 1;
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

    void Start()
    {
        movieGenerator = FindObjectOfType<MovieGenerator>(); // Find the MovieGenerator script in the scene

        // Subscribe to the event to start voting when requested
        MovieGenerator.OnVotingStartRequested += StartVotingIfRequested;
    }

    void StartVotingIfRequested()
    {
        // Check if LikedMovies is not null and has items
        if (LikedMovies != null && LikedMovies.Count > 1)
        {
            CreateMatchups();
            SetMatchup();
        }
        else
        {
            Debug.LogError("LikedMovies is null or does not have enough movies. Make sure it is properly initialized in MovieGenerator.");
        }
    }

    void CreateMatchups()
    {
        List<int> shuffledMovies = new List<int>(LikedMovies.OrderBy(x => Guid.NewGuid()));

        while (shuffledMovies.Count > 1)
        {
            int movieA = shuffledMovies[0];
            shuffledMovies.RemoveAt(0);

            int movieB = shuffledMovies[0];
            shuffledMovies.RemoveAt(0);

            matchups.Enqueue(new Matchup(movieA, movieB));
        }
    }

    void SetMatchup()
    {
        if (matchups.Count > 0)
        {
            Matchup currentMatchup = matchups.Dequeue();

            FetchMovieInformation(currentMatchup.movieA, currentMatchup.movieB);

            movieAButton.interactable = true;
            movieBButton.interactable = true;
        }
        else if (roundCount == 1)
        {
            // Move to the losers bracket for the second round
            roundCount++;
            matchups = new Queue<Matchup>(losersBracket);
            losersBracket.Clear();
            SetMatchup();
        }
        else
        {
            Debug.Log("Voting process completed. No more matchups available.");
            DetermineWinner();
        }
    }

    void FetchMovieInformation(int movieAId, int movieBId)
    {
        StartCoroutine(FetchMovieDetails(movieAId, movieADetails =>
        {
            StartCoroutine(FetchMovieDetails(movieBId, movieBDetails =>
            {
                SetMovieUI(movieADetails, movieBDetails);
            }));
        }));
    }

    IEnumerator FetchMovieDetails(int movieId, Action<MovieDetails> callback)
    {
        string apiUrl = $"https://api.themoviedb.org/3/movie/{movieId}?api_key={apiKey}&language=en-US";
        using (UnityWebRequest request = UnityWebRequest.Get(apiUrl))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonResult = request.downloadHandler.text;
                MovieDetails movieDetails = JsonUtility.FromJson<MovieDetails>(jsonResult);
                callback?.Invoke(movieDetails);
            }
            else
            {
                Debug.LogError("Error fetching movie information for ID " + movieId + ": " + request.error);
            }
        }
    }

    void SetMovieUI(MovieDetails movieA, MovieDetails movieB)
    {
        movieATitle.GetComponent<TextMeshProUGUI>().text = movieA.title;
        movieAGenres.GetComponent<TextMeshProUGUI>().text = ConvertGenresToString(movieA.genres);

        StartCoroutine(FetchTexture($"https://image.tmdb.org/t/p/w500/{movieA.poster_path}", texture =>
        {
            movieAPoster.GetComponent<Image>().sprite = CreateSpriteFromTexture(texture, movieAPoster.GetComponent<RectTransform>());
        }));

        movieBTitle.GetComponent<TextMeshProUGUI>().text = movieB.title;
        movieBGenres.GetComponent<TextMeshProUGUI>().text = ConvertGenresToString(movieB.genres);

        StartCoroutine(FetchTexture($"https://image.tmdb.org/t/p/w500/{movieB.poster_path}", texture =>
        {
            movieBPoster.GetComponent<Image>().sprite = CreateSpriteFromTexture(texture, movieBPoster.GetComponent<RectTransform>());
        }));
    }

    void DetermineWinner()
    {
        Debug.Log("Voting process completed. Determining the winner...");

        if (losersBracket.Count > 0)
        {
            roundCount = 1;
            matchups = new Queue<Matchup>(losersBracket);
            losersBracket.Clear();
            SetMatchup();
        }
        else
        {
            Debug.Log("No more matchups. Tournament completed!");
        }
    }
    public void OnMovieAButtonClick()
    {
        // Check if there are more matchups in the queue
        if (matchups.Count > 0)
        {
            RecordVote(matchups.Peek().movieA, matchups.Peek().movieB);
        }
        else
        {
            Debug.LogWarning("No more matchups available.");
        }
    }

    public void OnMovieBButtonClick()
    {
        // Check if there are more matchups in the queue
        if (matchups.Count > 0)
        {
            RecordVote(matchups.Peek().movieB, matchups.Peek().movieA);
        }
        else
        {
            Debug.LogWarning("No more matchups available.");
        }
    }
    void RecordVote(int winnerId, int loserId)
    {
        losersBracket.Enqueue(new Matchup(winnerId, loserId));

        movieAButton.interactable = false;
        movieBButton.interactable = false;

        // Move to the next matchup
        currentMatchIndex++;

        // Check if there are more matchups in the queue
        if (currentMatchIndex < matchups.Count)
        {
            SetMatchup();
        }
        else if (losersBracket.Count > 0)
        {
            // If there are still matchups in the losers bracket, go to the next one
            SetNextLosersBracketMatchup();
        }
        else
        {
            Debug.Log("Voting process completed. No more matchups available.");
            DetermineWinner();
        }
    }

    void SetNextLosersBracketMatchup()
    {
        // Check if there are more matchups in the losers bracket
        if (losersBracket.Count > 0)
        {
            // Get the next matchup from the losers bracket
            Matchup nextLosersBracketMatchup = losersBracket.Dequeue();

            // Implement logic to set up UI elements for the next losers bracket matchup
            // For example, you can call a method to set UI based on matchup details
            SetLosersBracketMatchupUI(nextLosersBracketMatchup);
        }
        else
        {
            // If there are no more losers bracket matchups, determine the final winner
            DetermineWinner();
        }
    }

        void SetLosersBracketMatchupUI(Matchup matchup)
    {
        // Set up UI elements for movie A and B in the losers bracket
        // Similar to how you set up UI for winners bracket matchups
        FetchMovieInformation(matchup.movieA, matchup.movieB);

        // Enable buttons for voting
        movieAButton.interactable = true;
        movieBButton.interactable = true;
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
        int targetWidth = Mathf.RoundToInt(rectTransform.rect.width);
        int targetHeight = Mathf.RoundToInt(rectTransform.rect.height);

        // Create a RenderTexture to resize the texture
        RenderTexture rt = RenderTexture.GetTemporary(targetWidth, targetHeight, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Default);
        rt.filterMode = FilterMode.Bilinear;

        // Set the active RenderTexture and blit the texture to the new size
        RenderTexture.active = rt;
        Graphics.Blit(texture, rt);

        // Create a new Texture2D to read from the RenderTexture
        Texture2D resizedTexture = new Texture2D(targetWidth, targetHeight);
        resizedTexture.ReadPixels(new Rect(0, 0, targetWidth, targetHeight), 0, 0);
        resizedTexture.Apply();

        // Release the temporary RenderTexture
        RenderTexture.ReleaseTemporary(rt);

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

    [Serializable]
    public class MovieDetails
    {
        public string title;
        public string poster_path;
        public List<Genre> genres;
    }

    [Serializable]
    public class Genre
    {
        public int id;
        public string name;
    }

    public class Matchup
    {
        public int movieA;
        public int movieB;

        public Matchup(int a, int b)
        {
            movieA = a;
            movieB = b;
        }
    }
}
