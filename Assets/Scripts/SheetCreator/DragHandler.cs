using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using SheetCreator;

public class DragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {

    public static GameObject currentDragged;
    RectTransform rectT;
    RectTransform tab;

    void Start()
    {
        rectT = gameObject.GetComponent<RectTransform>();
        tab = transform.parent.GetComponent<RectTransform>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        currentDragged = gameObject;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (currentDragged == gameObject)
        {
            Vector2 size = new Vector2(rectT.rect.width, rectT.rect.height);

            //Bottom left / top left / top right / bottom right
            Vector3[] tabCorners = new Vector3[4];
            tab.GetWorldCorners(tabCorners);

            Vector3 tabSize = new Vector3(tabCorners[3].x - tabCorners[0].x, tabCorners[1].y - tabCorners[0].y, 0);

            Vector3 mousePos = Input.mousePosition;
            Vector3 LocalMouse = Vector3.zero;
            LocalMouse.x = ((mousePos.x - tabCorners[0].x)/tabSize.x)*tab.rect.width;
            LocalMouse.y = ((mousePos.y - tabCorners[1].y)/tabSize.y)*tab.rect.height;
            LocalMouse.z = mousePos.z;

            var xTo = Mathf.Clamp(LocalMouse.x - (size.x/2), tab.rect.xMin, tab.rect.xMax - size.x);
            var yTo = Mathf.Clamp(LocalMouse.y + (size.y/2), tab.rect.yMin + size.y, tab.rect.yMax);

            transform.localPosition = new Vector3(xTo,yTo,transform.localPosition.z);
            scr_EditBox.active.UpdateInputs();
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        currentDragged = null;
    }
}
