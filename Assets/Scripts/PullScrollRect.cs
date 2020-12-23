using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using PullToRefresh;

public class PullScrollRect : ScrollRect, IScrollable
{
    private bool m_dragging;

    public bool Dragging { get { return m_dragging; } }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        base.OnBeginDrag(eventData);

        m_dragging = true;
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        base.OnEndDrag(eventData);

        m_dragging = false;
    }
}
