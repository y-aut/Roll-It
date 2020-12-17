using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ユーザー情報を圧縮したクラス
[Serializable]
public class UserZip
{
    // ユーザーID
    [SerializeField]
    private IDType i;

    // ユーザー名
    [SerializeField]
    private string n;

    // アカウント作成日時
    [SerializeField]
    private DateTime s;  // Start

    // 最終ログイン日時
    [SerializeField]
    private DateTime l;  // Last

    // クリアしたステージ数（重複なし）
    [SerializeField]
    private int c;

    // 総高評価数
    [SerializeField]
    private int f;   // Favored

    // 公開したステージ
    [SerializeField]
    private IDTypeCollection p;  // Published

    public UserZip(User src)
    {
        i = src.ID;
        n = src.Name;
        s = src.StartDate;
        l = src.LastDate;
        c = src.ClearedCount;
        f = src.PosEvaCount;
        p = new IDTypeCollection(src.PublishedStages);
    }

    public User ToUser()
    {
        return new User(i, n, s, l, c, f, p);
    }
}