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

public class VotingSystem : MonoBehaviour
{
    public string apiKey = "81e872ea74c4fe76eed2dd856b47223d"; // Replace with your TMDb API key
    public static List<int> LikedMovies;  // Assuming this is the list of liked movie IDs from MovieGenerator
    private Queue<Matchup> matchups = new Queue<Matchup>();
    private Matchup currentMatchup;
    private int currentRound = 1;

    public GameObject movieATitle;
    public GameObject movieAGenres;
    public GameObject movieAPoster;
    public Button movieAButton;

    public GameObject movieBTitle;
    public GameObject movieBGenres;
    public GameObject movieBPoster;
    public Button movieBButton;

    private bool multiGame;
    private MovieGenerator movieGenerator; // Reference to the MovieGenerator script

    void Start()
    {
        movieGenerator = FindObjectOfType<MovieGenerator>(); // Find the MovieGenerator script in the scene
        multiGame = PlayerPrefs.GetInt("MultiGame", 0) == 1;

        // Subscribe to the event to start voting when requested
        MovieGenerator.OnVotingStartRequested += StartVotingIfRequested;
    }

    void StartVotingIfRequested()
    {
        // Check if LikedMovies is not null and has items
        if (LikedMovies != null && LikedMovies.Count > 0)
        {
            CreateMatchups(LikedMovies);
            SetMatchup();
        }
        else
        {
            Debug.LogError("LikedMovies is null or does not have enough movies. Make sure it is properly initialized in MovieGenerator.");
        }
    }

void CreateMatchups(List<int> movieIds)
{
    ShuffleLikedMovies();
    // Handle the case where there is an odd number of movies
    if (movieIds.Count % 2 != 0)
    {
        // Save the last movie ID before removing it from the list
        int remainingMovieId = movieIds[movieIds.Count - 1];
        movieIds.RemoveAt(movieIds.Count - 1);

        // Continue with creating matchups for the remaining movies
        while (movieIds.Count > 1)
        {
            int movieA = movieIds[0];
            movieIds.RemoveAt(0);

            int movieB = movieIds[0];
            movieIds.RemoveAt(0);

            matchups.Enqueue(new Matchup(movieA, movieB));
        }

        // Add the remaining movie back to the list
        movieIds.Add(remainingMovieId);
    }
    else
    {
        // Create matchups for all movies if the count is even
        while (movieIds.Count > 1)
        {
            int movieA = movieIds[0];
            movieIds.RemoveAt(0);

            int movieB = movieIds[0];
            movieIds.RemoveAt(0);

            matchups.Enqueue(new Matchup(movieA, movieB));
        }
    }
}


void RecordVote(int winnerId, int loserId)
{
    // Add the winnerId to LikedMovies
    LikedMovies.Add(winnerId);

    // You can implement your own logic to record the vote for the selected movie   
    //Debug.Log("Vote recorded for movie ID: " + winnerId + " liked movies count: " + LikedMovies.Count + " Matchups left: " + matchups.Count);

    movieAButton.interactable = false;
    movieBButton.interactable = false;
}


void SetMatchup()
{
    if (matchups.Count > 0)
    {
        currentMatchup = matchups.Dequeue();
        FetchMovieInformation(currentMatchup.movieA, currentMatchup.movieB);

        movieAButton.interactable = true;
        movieBButton.interactable = true;
    }
    else
    {
        //Debug.Log("Voting process completed. No more matchups available.");

        // Disable buttons when there are no more matchups
        movieAButton.interactable = false;
        movieBButton.interactable = false;

        // Check if LikedMovies has only one item (the winner) before determining the winner
        DetermineWinner();
    }
}

void DetermineWinner()
{
    if (LikedMovies.Count > 1)
    {
        currentRound++;
        CreateMatchups(LikedMovies);
        SetMatchup();
    }
    else if (LikedMovies.Count == 1)
    {
        int winnerId = LikedMovies[0];

        Debug.Log("Tournament completed! The winner is Movie ID: " + winnerId);
        
        PlayerPrefs.SetInt("WinnerId", winnerId);
        PlayerPrefs.Save();

        if (multiGame)
        {
            object[] eventData = new object[] { winnerId };
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
            PhotonNetwork.RaiseEvent(159, eventData, raiseEventOptions, ExitGames.Client.Photon.SendOptions.SendReliable);
        }
        
        SceneManager.LoadScene("WaitRoom");
    }
    
    else
    {
        Debug.Log("No more matchups. LikedMovies is empty.");
    }
}

    public void OnMovieAButtonClick()
    {
        // Check if there are more matchups in the queue
        if (matchups.Count >= 0)
        {
            RecordVote(currentMatchup.movieA, currentMatchup.movieB);
            SetMatchup();
        }
        else
        {
            Debug.Log("problem");
        }
    }

    public void OnMovieBButtonClick()
    {
        // Check if there are more matchups in the queue
        if (matchups.Count >= 0)
        {
            RecordVote(currentMatchup.movieB, currentMatchup.movieA);
            SetMatchup();
        }
        else
        {
            Debug.Log("problem");
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

    public void OnClickStopButton()
    {
        SceneManager.LoadScene("Title");    
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

    void ShuffleLikedMovies()
    {
        System.Random rng = new System.Random();
        int n = LikedMovies.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            int value = LikedMovies[k];
            LikedMovies[k] = LikedMovies[n];
            LikedMovies[n] = value;
        }
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
