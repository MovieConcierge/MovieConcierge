using UnityEngine;
using UnityEngine.EventSystems;

public class Drag : MonoBehaviour,IDragHandler,IBeginDragHandler,IEndDragHandler
{
    private Vector2 prevPos;
    GameObject moviegenerator;
    MovieGenerator moviegenerator_script;

    public void OnBeginDrag(PointerEventData eventData)
    {
        prevPos = transform.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
        //Debug.LogFormat("{0}",transform.position);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if(((Vector2)transform.position - prevPos).magnitude < 50){
            transform.position = prevPos;
        }
        else if(((Vector2)transform.position - prevPos).magnitude > 50 && transform.position[0] < 250){//Dislike
            //while(transform.position[0]>0){
            //    transform.Translate(-0.001f,0.001f,0);
            //}
            Debug.Log(1);
            moviegenerator = GameObject.Find("MovieController");
            moviegenerator_script = moviegenerator.GetComponent<MovieGenerator>();
            moviegenerator_script.OnDislikeButtonClick();
            transform.position = prevPos;
        }
        else{//Like
            //while(transform.position[0]<500){
            //    transform.Translate(0.001f,0.001f,0);
            //}
            Debug.Log(1);
            moviegenerator = GameObject.Find("MovieController");
            moviegenerator_script = moviegenerator.GetComponent<MovieGenerator>();
            moviegenerator_script.OnLikeButtonClick();
            transform.position = prevPos;
        }

    }

}
