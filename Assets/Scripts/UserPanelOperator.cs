using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UserPanelOperator : MonoBehaviour
{
    // 各種コントロール
    public TextMeshProUGUI TxtName;
    public TextMeshProUGUI TxtPosEvaCount;
    public TextMeshProUGUI TxtChallengedCount;
    public TextMeshProUGUI TxtFavoredCount;
    public TextMeshProUGUI TxtRanking;
    public GameObject ImgGoldCrown;
    public GameObject ImgSilverCrown;
    public GameObject ImgBronzeCrown;
    public GameObject ImgStar;
    public GameObject ImgStarAbsent;
    public StageViewContentOperator PublishedContent;

    private Transform parent;
    public Popup popup;

    // 対応するユーザー
    public User User { get; set; }

    private readonly List<Stage>[] StageCache = new List<Stage>[StageViewContentOperator.STAGE_LIMIT];

    public static void ShowDialog(Transform parent, User user, MenuOperator menuOp)
    {
        var panel = Instantiate(Prefabs.UserPanelPrefab, parent, false);
        panel.transform.localScale = Prefabs.OpenCurve.Evaluate(0f) * Vector3.one;  // 初めに見えてしまうのを防ぐ
        var script = panel.GetComponent<UserPanelOperator>();

        script.User = user;
        script.parent = parent;
        script.PublishedContent.menuOp = menuOp;
        script.PublishedContent.parent = parent;

        script.Initialize();
        script.popup.Open();
    }

    public void Initialize()
    {
        // 初期値
        UpdateControls();
        UpdateRanking();
        InitializeStages();
    }

    // コントロールの値を更新
    private void UpdateControls()
    {
        TxtName.text = User.Name;
        TxtPosEvaCount.text = string.Format("{0:#,0}", User.PosEvaCount);
        TxtChallengedCount.text = string.Format("{0:#,0}", User.ChallengedCount);
        TxtFavoredCount.text = string.Format("{0:#,0}", User.FavoredCount);
        if (!GameData.User.LocalData.FavorUserIDs.Contains(User.ID))
        {
            ImgStar.SetActive(false);
            ImgStarAbsent.SetActive(true);
        }
        else
        {
            ImgStar.SetActive(true);
            ImgStarAbsent.SetActive(false);
        }
    }

    // 公開したステージ一覧を初期化
    public async void InitializeStages()
    {
        // 0ページ目を追加
        StageCache[0] = new List<Stage>();
        int imax = Math.Min(User.PublishedStages.Count, StageViewContentOperator.STAGE_LIMIT);
        for (int i = 0; i < imax; ++i)
        {
            var stage = await Cache.GetStage(User.PublishedStages[i]);
            if (!stage.IsNotFound)
                StageCache[0].Add(stage);
        }

        PublishedContent.SetStages(StageCache[0], true, false, false, User.PublishedStages.LastOrDefault(IDType.Empty));
    }

    // ページが変更された
    public async void PageChanged()
    {
        if (StageCache[PublishedContent.Page] == null)
        {
            StageCache[PublishedContent.Page] = new List<Stage>();
            int imin = PublishedContent.Page * StageViewContentOperator.STAGE_LIMIT;
            int imax = Math.Min(User.PublishedStages.Count, imin + StageViewContentOperator.STAGE_LIMIT);
            for (int i = imin; i < imax; ++i)
            {
                var stage = await Cache.GetStage(User.PublishedStages[i]);
                if (!stage.IsNotFound)
                    StageCache[PublishedContent.Page].Add(stage);
            }
        }

        PublishedContent.PageUpdate(StageCache[PublishedContent.Page], User.PublishedStages.LastOrDefault(IDType.Empty));
    }

    // ランキングを更新
    public void UpdateRanking()
    {

    }

    // Click Events
    public async void BtnStarClicked()
    {
        NowLoading.Show(parent, "Connecting...");
        if (!GameData.User.LocalData.FavorUserIDs.Contains(User.ID))
        {
            try
            {
                await FirebaseIO.IncrementFavoredCount(User).WaitWithTimeOut();
                ImgStar.SetActive(true);
                ImgStarAbsent.SetActive(false);
            }
            catch (Exception e)
            {
                e.Show(parent);
            }
        }
        else
        {
            try
            {
                await FirebaseIO.DecrementFavoredCount(User).WaitWithTimeOut();
                ImgStar.SetActive(false);
                ImgStarAbsent.SetActive(true);
            }
            catch (Exception e)
            {
                e.Show(parent);
            }
        }
        NowLoading.Close();
    }

    public void BtnCloseClicked()
    {
        popup.CloseAndDestroy();
    }

    private void Update()
    {
        // 戻るボタン
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            BtnCloseClicked();
        }
    }
}
