using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StructureItemOperator : MonoBehaviour
{
    public RawImage ImgPreview;
    public GameObject ImgTriangle;
    public RawImage ImgAdditional;
    public StructureType Type;
    public int Texture { get; private set; }
    private bool IsClickable;
    private CreateOperator CreateOp;

    public void Initialize(CreateOperator createOp, StructureType type, int texture, bool isClickable)
    {
        CreateOp = createOp;
        Type = type;
        Texture = texture;
        IsClickable = isClickable && Prefabs.GetTextureCount(type) != 1;
        ImgTriangle.SetActive(IsClickable);
        ImgPreview.texture = Cache.StructPreviews[Type][Texture];
        GetComponentInChildren<TMPro.TextMeshProUGUI>().text = Type.ToString();

        var adt = Prefabs.AdditionalSprites[Type] != null;
        ImgAdditional.gameObject.SetActive(adt);
        if (adt) ImgAdditional.texture = Prefabs.AdditionalSprites[Type].texture;
    }

    public void Clicked()
    {
        if (!IsClickable) return;
        CreateOp.TextureList.UpdateList(this);
        CreateOp.TextureList.gameObject.SetActive(true);
    }

    public void Dragged()
    {
        CreateOp.ItemDragged(this);
    }

    public void Released()
    {
        CreateOp.ItemReleased();
    }

    // 現在のImgPreviewの画像を保存し、再設定
    public void SetPreview()
    {
        var source = ImgPreview.texture;
        var copy = new RenderTexture((RenderTexture)source);
        Graphics.CopyTexture(source, copy);
        ImgPreview.texture = copy;
    }

}
