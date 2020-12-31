using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using PullToRefresh;
using System.Threading.Tasks;
using UnityEngine.Events;

public class StageViewContentOperator : MonoBehaviour
{
    public const int PAGE_LIMIT = 10;
    public const int STAGE_LIMIT = 10;

    private bool UsePages;
    private bool Refreshable;

    public GameObject content;
    public GameObject PnlPage;
    public TextMeshProUGUI TxtPage;
    public Button BtnBack;
    public Button BtnNext;

    public GameObject ImgRefresh;
    public UIRefreshControl refresh;

    public MenuOperator menuOp;
    public Transform parent;

    public UnityEvent PageChanged = new UnityEvent();

    // 表示中のステージ
    private List<Stage> Stages;
    private List<StageItemOperator> StageItems;

    private bool isLastPage;    // 最終ページか
    // 表示中のページ
    private int _page;
    public int Page
    {
        get => _page;
        set
        {
            _page = value;
            PageChanged.Invoke();
        }
    }

    // ページ数の初期値を設定
    public void SetPageWithoutNotify(int page)
    {
        _page = page;
    }

    // 自分のステージか（ダウンロードしたステージ含む）
    public bool IsMyStages { get; private set; } = true;

    // PanelからAuthor情報を取得できるようにするか
    public bool AuthorAccessible;

    public void SetStages(List<Stage> stages, bool usePages, bool isMyStages, bool refreshable, IDType? lastStageID = null)
    {
        // 表示していたStagesを消去
        if (StageItems != null)
        {
            foreach (var item in StageItems)
            {
                Destroy(item.gameObject);
            }
        }

        Stages = new List<Stage>(stages);
        UsePages = usePages;
        IsMyStages = isMyStages;
        Refreshable = refreshable;
        if (Stages.Count == 0)
            isLastPage = true;
        else if (lastStageID != null)
            isLastPage = lastStageID == Stages.Last().ID;
        else
            isLastPage = true;

        InitializeControls();
        UpdateControls();
    }

    // Page変更時に呼ばれる
    public void PageUpdate(List<Stage> stages, IDType lastStageID)
    {
        // 表示していたStagesを消去
        if (StageItems != null)
        {
            foreach (var item in StageItems)
            {
                Destroy(item.gameObject);
            }
        }

        Stages = stages;
        if (Stages.Count == 0)
            isLastPage = true;
        else
            isLastPage = (lastStageID == Stages.Last().ID) || Page == PAGE_LIMIT - 1;

        UpdateControls();
    }

    // SetStagesでのみ呼ばれる
    private void InitializeControls()
    {
        if (!UsePages)
        {
            PnlPage.SetActive(false);
        }
        else
        {
            PnlPage.SetActive(true);
            _page = 0;
        }
        ImgRefresh.SetActive(Refreshable);
    }

    // SetStages, Page変更時に呼ばれる
    private void UpdateControls()
    {
        StageItems = new List<StageItemOperator>();

        // StageをContentに追加
        foreach (var stage in Stages)
        {
            var item = Instantiate(Prefabs.StageItemPrefab, content.transform, false);
            var script = item.GetComponent<StageItemOperator>();
            script.Stage = stage;
            script.parent = parent;
            script.IsMyStage = IsMyStages;
            script.ParentView = this;
            script.AuthorAccessible = AuthorAccessible;
            script.Initialize();
            StageItems.Add(script);
        }

        // Previewを取得
        StartCoroutine(GetPreviews());

        if (UsePages)
        {
            BtnBack.interactable = Page != 0;
            BtnNext.interactable = !isLastPage;
            TxtPage.text = (Page + 1).ToString();
        }
    }

    // 順番にPreviewを取得していく
    IEnumerator GetPreviews()
    {
        for (int i = 0; i < Stages.Count; ++i)
        {
            menuOp.CreatePreview(Stages[i]);
            yield return new WaitForEndOfFrame();
            StageItems[i].SetPreview();
        }
    }

    // ステージの変更を反映
    // ステージの内容が変更されたときに呼ぶ（ステージ自体が追加、削除されたときには呼ばない）
    public void UpdateStages()
    {
        foreach (var item in StageItems)
        {
            item.UpdateValue();
        }
    }

    // ステージを削除する
    // StageItemのDestroyまではしない
    public bool RemoveStage(Stage stage)
    {
        for (int i = 0; i < Stages.Count; ++i)
        {
            if (Stages[i] == stage)
            {
                Stages.RemoveAt(i);
                StageItems.RemoveAt(i);
                return true;
            }
        }
        return false;
    }

    public void BtnNextClicked()
    {
        ++Page;
    }

    public void BtnBackClicked()
    {
        --Page;
    }

}
