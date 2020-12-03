using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameConst
{
    // int -> floatの変換スケール
    // PositionInt = Position * POSITION_SCALE
    public const float LOCALSCALE_SCALE = 2;
    public const float POSITION_SCALE = LOCALSCALE_SCALE * 2;

    // 半透明時のアルファ値
    public const int FADE_ALPHA = 100;

    // ゲームオーバーになるy座標
    public const int GAMEOVER_Y = -10;

    // プレイ時の視点のボールからの距離
    public const int PLAY_CAMDIST_HOR = 6;
    public const int PLAY_CAMDIST_VER = 3;

    // 視点回転の速度(ms/ROTATE_VIEWPOINT_DEGREE °)
    public const int ROTATE_VIEWPOINT_MS = 20;
    // 視点回転の速度(°/ROTATE_VIEWPOINT_MS ms)（90の約数である必要がある）
    public const int ROTATE_VIEWPOINT_DEGREE = 2;

    // ステージの限界座標
    public const int X_NLIMIT = -500;
    public const int X_PLIMIT = 500;
    public const int Y_NLIMIT = -500;
    public const int Y_PLIMIT = 500;
    public const int Z_NLIMIT = -500;
    public const int Z_PLIMIT = 500;

}
