using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ユーザー情報を表すクラス
public class User
{
    // ユーザーID
    public IDType ID = IDType.Empty;

    // ユーザー名
    public string Name = "";

    // アカウント作成日時
    public DateTime StartDate;

    // 最終ログイン日時
    public DateTime LastDate;

    // クリアしたステージ数（重複なし）
    public int ClearedCount = 0;

    // 総高評価数
    public int PosEvaCount = 0;

    // 公開したステージ
    public List<IDType> PublishedStages;

    // ローカルデータ
    public UserLocal LocalData;

    public User()
    {
        StartDate = DateTime.Now;
        PublishedStages = new List<IDType>();
        LocalData = new UserLocal();
    }

    // UserZipからの解凍時に用いる
    public User(IDType id, string name, DateTime start, DateTime last, int clear, int posEva, IDTypeCollection published)
    {
        ID = id;
        Name = name;
        StartDate = start;
        LastDate = last;
        ClearedCount = clear;
        PosEvaCount = posEva;
        PublishedStages = published.ToList();
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
    public List<IDType> ClearedIDs;

    // 高評価したステージのIDリスト
    [SerializeField]
    public List<IDType> PosEvaIDs;

    // 低評価したステージのIDリスト
    [SerializeField]
    public List<IDType> NegEvaIDs;

    public UserLocal()
    {
        ClearedIDs = new List<IDType>();
        PosEvaIDs = new List<IDType>();
        NegEvaIDs = new List<IDType>();
    }
}