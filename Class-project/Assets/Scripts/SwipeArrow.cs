using UnityEngine;
using UnityEngine.EventSystems;

public class SwipeArrow : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    private Vector2 prevPos;
    public float swipeThreshold = 50f;
    public float xCoordinateLimit; // This will be set in Start()

    private RectTransform posterRectTransform;
    private Canvas canvas;
    private MultiVoting multiVoting;

    void Start()
    {
        GameObject movieController = GameObject.Find("MultiVotingController");
        multiVoting = movieController.GetComponent<MultiVoting>();
        // Assuming the MainCanvas is full width of the screen
        posterRectTransform = GetComponent<RectTransform>();
        canvas = posterRectTransform.GetComponentInParent<Canvas>();
        
        if (canvas != null)
        {
            // Set the xCoordinateLimit to half the canvas width
            xCoordinateLimit = canvas.GetComponent<RectTransform>().sizeDelta.x / 2;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        prevPos = posterRectTransform.anchoredPosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Update the posterRectTransform position by the drag delta directly without smoothing
        posterRectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor; // Adjusting with scaleFactor for different resolutions
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        float swipeDistance = (posterRectTransform.anchoredPosition - prevPos).magnitude;
        float swipeDirection = posterRectTransform.anchoredPosition.x - prevPos.x;

        if (swipeDistance < swipeThreshold)
        {
            // Snap back to the original position if it's not a swipe
            posterRectTransform.anchoredPosition = prevPos;
        }
        else
        {
            if (swipeDirection < 0 && Mathf.Abs(posterRectTransform.anchoredPosition.x) > xCoordinateLimit)
            {
                multiVoting.ShowPreviousMovie();
                // You can animate the posterRectTransform off-screen here if desired
            }
            else if (swipeDirection > 0 && Mathf.Abs(posterRectTransform.anchoredPosition.x) > xCoordinateLimit)
            {
                multiVoting.ShowNextMovie();
                // You can animate the posterRectTransform off-screen here if desired
            }
            posterRectTransform.anchoredPosition = prevPos;
        }
    }
}
