using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameConst
{
    // int -> floatの変換スケール
    // PositionInt = Position * POSITION_SCALE
    public const float POSITION_SCALE = 4;
    public const float LOCALSCALE_SCALE = 2;

    // 半透明時のアルファ値
    public const float FADE_ALPHA = 0.3f;

    // ゲームオーバーになるy座標
    public const int GAMEOVER_Y = -10;
}
