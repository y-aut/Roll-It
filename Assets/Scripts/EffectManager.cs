using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectManager
{
    // ステージクリア時のエフェクト
    const float CLEAR_DURATION = 2;  // エフェクト表示秒数

    public IEnumerator StageClear(GameObject ImgClear, Action after)
    {
        ImgClear.SetActive(true);
        yield return new WaitForSecondsRealtime(CLEAR_DURATION);

        after();
    }

}
