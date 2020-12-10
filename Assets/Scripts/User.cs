using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ユーザー情報を表すクラス
[Serializable]
public class User
{
    // ユーザーID
    [SerializeField]
    public IDType ID = IDType.Empty;

    // ユーザー名
    [SerializeField]
    public string Name = "";

    // 作成日時
    [SerializeField]
    public DateTime StartDate;

    // 最終ログイン日時
    [SerializeField]
    public DateTime LastDate;

    // 公開したステージ数
    [SerializeField]
    public int PublishedCount = 0;

    // クリアしたステージ数（重複なし）
    [SerializeField]
    public int ClearedCount = 0;

    // ローカルデータ
    [NonSerialized]
    public UserLocal LocalData;

    public void Initialize()
    {
        StartDate = DateTime.Now;
        LocalData = new UserLocal();
        LocalData.Initialize();
    }

    public void Login()
    {
        LastDate = DateTime.Now;
    }
}

// ローカルに保存するユーザーデータ
[Serializable]
public class UserLocal
{
    // クリアしたステージのIDリスト
    [SerializeField]
    public List<int> ClearedIDs;

    public void Initialize()
    {
        ClearedIDs = new List<int>();
    }
}