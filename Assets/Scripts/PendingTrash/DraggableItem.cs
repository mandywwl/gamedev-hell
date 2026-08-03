using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("Begin Dragging " + gameObject.name);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Debug.Log("Dragging " + gameObject.name);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("End Dragging " + gameObject.name);
    }
}