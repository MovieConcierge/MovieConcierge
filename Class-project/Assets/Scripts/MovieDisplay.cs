using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MovieDisplay : MonoBehaviour
{
    public GameObject movieTitle;
    public GameObject Genresbtn;

    public GameObject moviePoster;
    public static string newTitle;
    public static string newGenres;

    public static Texture2D newPosterTexture;
    private Sprite defaultSprite;

    void Start()
    {
        MovieGenerator.OnTextureFetched += DisplayMovie;
        WaitGenerator.OnTextureFetchedWait += DisplayMovie;
        MultiVoting.OnTextureFetchedMulti += DisplayMovie;
        StartCoroutine(PushTextOnScreen());
    }


    IEnumerator PushTextOnScreen()
    {
        yield return new WaitForSeconds(0.1f);
        DisplayMovie();
    }


    void DisplayMovie()
    {
        if (moviePoster != null & movieTitle!= null & Genresbtn != null)
        {
            moviePoster.SetActive(true);
            movieTitle.GetComponent<TextMeshProUGUI>().text = newTitle;
            Genresbtn.GetComponent<TextMeshProUGUI>().text = newGenres;
            moviePoster.GetComponent<Image>().sprite = CreateSpriteFromTexture(newPosterTexture, moviePoster.GetComponent<RectTransform>());
        }
    }

    Sprite CreateSpriteFromTexture(Texture2D texture, RectTransform rectTransform)
    {
        // Retrieve target dimensions from RectTransform
        int targetWidth = Mathf.RoundToInt(rectTransform.rect.width);
        int targetHeight = Mathf.RoundToInt(rectTransform.rect.height);

        // Resize the texture to the target dimensions
        Texture2D resizedTexture = ResizeTexture(texture, targetWidth, targetHeight);

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
}