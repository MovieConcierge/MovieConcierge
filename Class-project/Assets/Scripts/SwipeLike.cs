using UnityEngine;
using UnityEngine.EventSystems;

public class SwipeLike : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    private Vector2 prevPos;
    private Vector2 prevLikeStamp;
    private Vector2 prevDislikeStamp;
    private Vector2 fromPosToLikeStamp = new Vector2(-100,150);
    private Vector2 fromPosToDislikeStamp = new Vector2(100,150);
    public float swipeThreshold = 50f;
    public float xCoordinateLimit; // This will be set in Start()
    public GameObject likeStamp;

    public GameObject dislikeStamp;

    private RectTransform likeStampTransform;
    private RectTransform dislikeStampTransform;

    private RectTransform posterRectTransform;
    private Vector3 baseRotationRatio = new Vector3(0.0f,0.0f,0.05f);
    private Canvas canvas;
    private MovieGenerator movieGenerator;
    
    

    void Start()
    {
        GameObject movieController = GameObject.Find("MovieController");
        movieGenerator = movieController.GetComponent<MovieGenerator>();
        // Assuming the MainCanvas is full width of the screen
        posterRectTransform = GetComponent<RectTransform>();
        canvas = posterRectTransform.GetComponentInParent<Canvas>();
        likeStampTransform = likeStamp.GetComponent<RectTransform>();
        dislikeStampTransform = dislikeStamp.GetComponent<RectTransform>();
        
        if (canvas != null)
        {
            // Set the xCoordinateLimit to half the canvas width
            xCoordinateLimit = canvas.GetComponent<RectTransform>().sizeDelta.x / 3;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        prevPos = posterRectTransform.anchoredPosition;
        prevLikeStamp = prevPos + fromPosToLikeStamp;
        prevDislikeStamp = prevPos + fromPosToDislikeStamp;
        likeStampTransform.anchoredPosition = prevLikeStamp;
        dislikeStampTransform.anchoredPosition = prevDislikeStamp;
    }
    public void OnDrag(PointerEventData eventData)
    {
        // Update the posterRectTransform position by the drag delta directly without smoothing
        posterRectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor; // Adjusting with scaleFactor for different resolutions
        posterRectTransform.rotation = Quaternion.Euler(baseRotationRatio*posterRectTransform.anchoredPosition.x);
        likeStampTransform.rotation = Quaternion.Euler(baseRotationRatio*posterRectTransform.anchoredPosition.x);
        dislikeStampTransform.rotation = Quaternion.Euler(baseRotationRatio*posterRectTransform.anchoredPosition.x);
        if ((posterRectTransform.anchoredPosition.x - prevPos.x)>0){
            likeStampTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
            dislikeStamp.SetActive(false);
            likeStamp.SetActive(true);
            dislikeStampTransform.anchoredPosition = prevDislikeStamp;
        }
        else if((posterRectTransform.anchoredPosition.x - prevPos.x)<0){
            dislikeStampTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
            likeStamp.SetActive(false);
            dislikeStamp.SetActive(true);
            likeStampTransform.anchoredPosition = prevLikeStamp;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        float swipeDistance = (posterRectTransform.anchoredPosition - prevPos).magnitude;
        float swipeDirection = posterRectTransform.anchoredPosition.x - prevPos.x;
        likeStamp.SetActive(false);
        dislikeStamp.SetActive(false);
        likeStampTransform.anchoredPosition = prevLikeStamp;
        dislikeStampTransform.anchoredPosition = prevDislikeStamp;
        posterRectTransform.rotation = Quaternion.Euler(0f,0f,0f);
        likeStampTransform.rotation = Quaternion.Euler(0f,0f,0f);
        dislikeStampTransform.rotation = Quaternion.Euler(0f,0f,0f);
        if (swipeDistance < swipeThreshold)
        {
            // Snap back to the original position if it's not a swipe
            posterRectTransform.anchoredPosition = prevPos;
        }
        else
        {
            if (swipeDirection < 0 && Mathf.Abs(posterRectTransform.anchoredPosition.x) > xCoordinateLimit)
            {
                this.gameObject.SetActive(false);
                movieGenerator.OnDislikeButtonClick();
                // You can animate the posterRectTransform off-screen here if desired
            }
            else if (swipeDirection > 0 && Mathf.Abs(posterRectTransform.anchoredPosition.x) > xCoordinateLimit)
            {
                this.gameObject.SetActive(false);
                movieGenerator.OnLikeButtonClick();
                // You can animate the posterRectTransform off-screen here if desired
            }
            posterRectTransform.anchoredPosition = prevPos;
        }
    }
}