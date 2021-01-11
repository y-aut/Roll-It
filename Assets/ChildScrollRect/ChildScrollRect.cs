using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChildScrollRect : ScrollRect
{
    public ScrollRect parentScrollRect;
    private Vector2 startPos;
    private bool eventFixed;
    private bool parentEvent;
    private bool parentEventInit;

    public override void OnInitializePotentialDrag(PointerEventData eventData)
    {
        startPos = eventData.position;
        base.OnInitializePotentialDrag(eventData);
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        base.OnBeginDrag(eventData);
    }

    public override void OnDrag(PointerEventData eventData)
    {
        if (!eventFixed)
        {
            Vector2 pos = eventData.position - startPos;
            float x = Mathf.Abs(pos.x);
            float y = Mathf.Abs(pos.y);
            if (x != y)
            {
                // 縦移動の方が多ければ親のイベントを発火
                // ただし、このScrollRectが不動ならば全て親に渡す
                parentEvent = (x < y) || (!vertical && !horizontal);
                eventFixed = true; // 親と子のどちらのイベントを実行するかをFix
            }
        }
        else
        {
            if (parentEvent)
            {
                if (!parentEventInit)
                {
                    // 親のイベント初期処理を実行
                    parentScrollRect.OnInitializePotentialDrag(eventData);
                    parentScrollRect.OnBeginDrag(eventData);
                    parentEventInit = true;
                }
                parentScrollRect.OnDrag(eventData);
            }
            else
            {
                base.OnDrag(eventData);
            }
        }

    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        if (parentEvent)
        {
            parentScrollRect.OnEndDrag(eventData);
        }
        else
        {
            base.OnEndDrag(eventData);
        }
        // フラグ関連を初期化
        parentEvent = false;
        parentEventInit = false;
        eventFixed = false;
    }
}