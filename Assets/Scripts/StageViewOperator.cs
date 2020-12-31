using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using PullToRefresh;

public class StageViewOperator : MonoBehaviour
{
    public StageViewContentOperator Content;
    public GameObject BtnNew;
    public GameObject PnlTabs;
    public Toggle TabTrending;
    public Toggle TabLatest;
    public Toggle TabPopular;

    public MenuOperator menuOp;

    public Transform parent;

    // 自分のステージか（ダウンロードしたステージ含む）
    public bool IsMyStages
    {
        set
        {
            PnlTabs.SetActive(!value);
            BtnNew.SetActive(value);
        }
    }

    // 選択されているタブ
    public StageViewTabs SelectedTab { get; private set; }

    // タブの初期値を設定
    public void SetSelectedTabWithoutNotify(StageViewTabs value)
    {
        switch (value)
        {
            case StageViewTabs.Trending:
                TabTrending.SetIsOnWithoutNotify(true);
                break;
            case StageViewTabs.Latest:
                TabLatest.SetIsOnWithoutNotify(true);
                break;
            case StageViewTabs.Popular:
                TabPopular.SetIsOnWithoutNotify(true);
                break;
        }
    }

    private void Awake()
    {
        TabTrending.SetIsOnWithoutNotify(true);
        SelectedTab = StageViewTabs.Trending;
        // Tab切り替えイベント
        TabTrending.onValueChanged.AddListener(i => TabChanged(i, StageViewTabs.Trending));
        TabLatest.onValueChanged.AddListener(i => TabChanged(i, StageViewTabs.Latest));
        TabPopular.onValueChanged.AddListener(i => TabChanged(i, StageViewTabs.Popular));
    }

    public void SetActive(bool value)
    {
        gameObject.SetActive(value);
    }

    public void BtnNewClicked()
    {
        menuOp.SaveHistory();
        CreateOperator.Ready(new Stage(), true);
        Scenes.LoadScene(SceneType.Create);
    }

    // 更新
    public async void OnRefresh()
    {
        await menuOp.RefreshStages();
        // 表示更新を終了
        Content.refresh.EndRefreshing();
    }

    public async void TabChanged(bool val, StageViewTabs tab)
    {
        if (!val || tab == SelectedTab) return;   // Onになったときにのみ更新
        SelectedTab = tab;
        Content.SetPageWithoutNotify(0);    // 0ページに戻す
        await menuOp.StageViewTabChanged();
    }


}

public enum StageViewTabs
{
    Trending, Latest, Popular, NB,
}

public static partial class AddMethod
{
    // タブをソートキーに変換
    public static string ToSortKey(this StageViewTabs tab)
    {
        switch (tab)
        {
            case StageViewTabs.Trending:
                return StageZip.GetKey(StageParams.PosEvaCount);
            case StageViewTabs.Latest:
                return StageZip.GetKey(StageParams.PublishedDate);
            case StageViewTabs.Popular:
                return StageZip.GetKey(StageParams.PosEvaCount);
            default:
                throw GameException.Unreachable;
        }
    }
}

// 各タブに関する情報
public class StageViewTabCollection<T>
{
    private T[] vals = new T[(int)StageViewTabs.NB];

    public T this[StageViewTabs tab]
    {
        get => vals[(int)tab];
        set => vals[(int)tab] = value;
    }
}