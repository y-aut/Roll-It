using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectManager
{
    // ステージクリア時のエフェクト
    const int CLEAR_DURATION = 400;  // エフェクト表示フレーム数
    public bool Cleared { get; private set; } = false;
    int ClearGen;
    GameObject ImgClear;

    public void StageClear(GameObject _imgClear)
    {
        Cleared = true;
        ClearGen = 0;
        ImgClear = _imgClear;
    }

    // 各演出をUpdateする
    public void Update()
    {
        if (Cleared)
        {
            if (ClearGen++ == 0)
                ImgClear.SetActive(true);
            else if (ClearGen == CLEAR_DURATION)
            {
                Cleared = false;
            }
        }
    }
}
