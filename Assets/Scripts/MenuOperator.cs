using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Firebase;
using System.Threading.Tasks;
using System;

public class MenuOperator : MonoBehaviour
{
    public Canvas canvas;
    public Camera PrevCam;
    public Camera StructCam;
    public RenderTexture StructPrevRT;

    public HeaderPanelOperator header;
    public StageViewOperator stageView;
    public ResultPanelOperator resultPanel;
    private static bool firstTime = true;   // 起動時に一度だけ処理するため

    private static bool openResult;     // 次回はhistoryではなくResultを開く
    private static Stage resultStage;   // 次回ResultPageロード時に読み込むステージ

    private Stage PrevStage;    // PrevCamで表示しているStage
    private Structure PrevStruct;   // StructCamで表示しているStructure

    // 読み込んだステージを一時保存しておく
    private static StageViewTabCollection<List<Stage>[]> StageCache;
    private static StageViewTabCollection<IDType> LastStageCacheID;

    private MenuPage _page = MenuPage.Unset;
    private MenuPage Page
    {
        get => _page;
        set
        {
            switch (Page)
            {
                case MenuPage.Find:
                    stageView.SetActive(false);
                    break;
                case MenuPage.Create:
                    stageView.SetActive(false);
                    break;
                case MenuPage.Result:
                    resultPanel.SetActive(false);
                    break;
                default:
                    stageView.SetActive(false);
                    resultPanel.SetActive(false);
                    break;
            }
            _page = value;
            LoadNewPage();
        }
    }

    // 別シーンロード前の状態を保持しておき、次回はここを開く
    private static MenuHistory history = new MenuHistory(MenuPage.Create);

    // 必要に応じて別シーンのロード前に呼ばれる
    public void SaveHistory()
    {
        if (Page == MenuPage.Find)
            history = new MenuHistory(stageView.SelectedTab);
        else
            history = new MenuHistory(Page);
    }

    public void LoadHistory()
    {
        if (history.Page == MenuPage.Result)
            history.Page = MenuPage.Find;   // Result画面のユーザー情報からPlayしたときはResultになる
        if (history.Page == MenuPage.Find)
            stageView.SetSelectedTabWithoutNotify(history.StageViewTab);
        Page = history.Page;
    }

    // スタートページを設定する
    public static void Ready(MenuPage page)
    {
        history.Page = page;
    }

    public static void ReadyForFind(StageViewTabs tab)
    {
        Ready(MenuPage.Find);
        history.StageViewTab = tab;
    }

    public static void ReadyForResult(Stage stage)
    {
        openResult = true;
        resultStage = stage;
    }

    private async void LoadNewPage()
    {
        switch (Page)
        {
            case MenuPage.Find:
                {
                    // 先にActiveにしないとGetPreviewのコルーチンが動かない
                    stageView.SetActive(true);
                    stageView.IsMyStages = false;
                    await StageViewTabChanged();
                }
                break;
            case MenuPage.Create:
                {
                    stageView.SetActive(true);
                    stageView.IsMyStages = true;
                    stageView.Content.SetStages(GameData.Stages, false, true, true);
                }
                break;
            case MenuPage.Result:
                {
                    await resultPanel.SetStage(resultStage);
                    resultPanel.SetActive(true);
                }
                break;
            default:
                throw GameException.Unreachable;
        }
    }
    
    // Start is called before the first frame update
    async void Awake()
    {
        if (firstTime)
        {
            firstTime = false;
            await FirstAwake();
        }

        await UpdateUserInfo();
        header.UpdateValue();

        if (openResult)
        {
            openResult = false;
            Page = MenuPage.Result;
        }
        else
        {
            stageView.SetSelectedTabWithoutNotify(history.StageViewTab);
            Page = history.Page;
        }
    }

    // 最後にユーザー情報を更新した時間
    private static DateTime LastUserInfoUpdatedTime = DateTime.MinValue;

    // ユーザー情報を更新する必要があれば更新
    private async Task UpdateUserInfo()
    {
        if (GameData.User.ID != IDType.Empty && DateTime.Now - LastUserInfoUpdatedTime > TimeSpan.FromMinutes(5))
        {
            NowLoading.Show(canvas.transform, "Updating the data...");
            try
            {
                // ローカルのUserを更新
                await FirebaseIO.SyncUser().WaitWithTimeOut();
                LastUserInfoUpdatedTime = DateTime.Now;
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
            NowLoading.Close();
        }
    }

    // FirstAwake()で処理を行ったかどうかのフラグ
    private static bool flgFirebase = false;
    private static bool flgLoadData = false;
    private static bool flgIDCheck = false;

    // 初回起動時の処理
    // 途中でエラーになったら、ダイアログが閉じるのを待って再度呼び出す
    private async Task FirstAwake()
    {
        if (!flgFirebase)
        {
            flgFirebase = true;
            // SDK で他のメソッドを呼び出す前に Google Play 開発者サービスを確認し、必要であれば、Firebase Unity SDK で必要とされるバージョンに更新します。
            // https://firebase.google.com/docs/unity/setup?hl=ja#prerequisites
            var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
            if (dependencyStatus == DependencyStatus.Available)
            {
                // Create and hold a reference to your FirebaseApp,
                // where app is a Firebase.FirebaseApp property of your application class.
                FirebaseIO.SetReference();

                // Set a flag here to indicate whether Firebase is ready to use by your app.
                FirebaseIO.Available = true;
            }
            else
            {
                Debug.LogError(string.Format(
                    "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                // Firebase Unity SDK is not safe to use here.
                FirebaseIO.Available = false;
            }
        }

        // ゲームデータのロード
        if (!flgLoadData)
        {
            flgLoadData = true;
            if (!GameData.Load(canvas.transform))
            {
                MessageBox.ShowDialog(canvas.transform, "Failed to load the data.", MessageBoxType.OKOnly,
                    () => _ = FirstAwake());
                return;
            }
        }

        // ユーザー名とIDを決める
        if (GameData.User.Name == "")
        {
            InputBox.ShowDialog(canvas.transform, "Enter your nickname", result =>
            {
                GameData.User.Name = result;
                header.UpdateValue();
                _ = FirstAwake();
            }, allowCancel: false);
            return;
        }

        if (!flgIDCheck)
        {
            flgIDCheck = true;
            if (GameData.User.ID == IDType.Empty)
            {
                // IDの取得を試みる
                try
                {
                    await FirebaseIO.RegisterUser(GameData.User).WaitWithTimeOut();
                    // 作ったコースがあればすべてAuthorIDを変更
                    foreach (var stage in GameData.Stages)
                    {
                        if (stage.ID == IDType.Empty) stage.ID = GameData.User.ID;
                    }
                    GameData.Save();
                }
                catch (Exception e)
                {
                    e.Show(canvas.transform, () => _ = FirstAwake());
                    return;
                }
            }
        }

        // StageCacheを初期化
        StageCache = new StageViewTabCollection<List<Stage>[]>();
        for (int i = 0; i < (int)StageViewTabs.NB; ++i)
            StageCache[(StageViewTabs)i] = new List<Stage>[StageViewContentOperator.PAGE_LIMIT];
        LastStageCacheID = new StageViewTabCollection<IDType>();

        // StructurePreviewを取得
        StartCoroutine(GetStructPreviews());
    }

    public void BtnFindClicked()
    {
        Page = MenuPage.Find;
    }

    public void BtnCreateClicked()
    {
        Page = MenuPage.Create;
    }

    // StageViewOperatorを更新
    public async Task RefreshStages()
    {
        if (stageView.Content.IsMyStages)
        {
            try
            {
                await FirebaseIO.UpdateMyStages();
                stageView.Content.UpdateStages();
            }
            catch (Exception e)
            {
                e.Show(canvas.transform);
            }
        }
        else
        {
            try
            {
                StageCache[stageView.SelectedTab][0]
                    = await FirebaseIO.GetStagesAtFirstPage(stageView.SelectedTab.ToSortKey()).WaitWithTimeOut();
                LastStageCacheID[stageView.SelectedTab] = await FirebaseIO.GetLastStageID(stageView.SelectedTab.ToSortKey()).WaitWithTimeOut();
                stageView.Content.SetStages(StageCache[stageView.SelectedTab][0], true, false, true,
                    LastStageCacheID[stageView.SelectedTab]);
            }
            catch (Exception e)
            {
                e.Show(canvas.transform);
            }
        }
    }
    
    // StageViewのTabが変更された
    public async Task StageViewTabChanged()
    {
        if (StageCache[stageView.SelectedTab][0] == null)
        {
            NowLoading.Show(canvas.transform, "Loading stages...");
            try
            {
                StageCache[stageView.SelectedTab][0]
                    = await FirebaseIO.GetStagesAtFirstPage(stageView.SelectedTab.ToSortKey()).WaitWithTimeOut();
                LastStageCacheID[stageView.SelectedTab] = await FirebaseIO.GetLastStageID(stageView.SelectedTab.ToSortKey()).WaitWithTimeOut();
            }
            catch (Exception e)
            {
                e.Show(canvas.transform);
                if (StageCache[stageView.SelectedTab][0] == null)
                    StageCache[stageView.SelectedTab][0] = new List<Stage>();
            }
            NowLoading.Close();
        }
        stageView.Content.SetStages(StageCache[stageView.SelectedTab][0], true, false, true,
            LastStageCacheID[stageView.SelectedTab]);
    }

    // StageView.Contentのpageが変更された
    public async void StageViewPageChanged()
    {
        if (StageCache[stageView.SelectedTab][stageView.Content.Page] == null)
        {
            // 次のページを取得
            NowLoading.Show(canvas.transform, "Loading stages...");
            try
            {
                StageCache[stageView.SelectedTab][stageView.Content.Page]
                    = await FirebaseIO.GetStagesAtNextPage(stageView.SelectedTab.ToSortKey()).WaitWithTimeOut();
            }
            catch (Exception e)
            {
                e.Show(canvas.transform);
                if (StageCache[stageView.SelectedTab][stageView.Content.Page] == null)
                    StageCache[stageView.SelectedTab][stageView.Content.Page] = new List<Stage>();
            }
            // LastStageは取得済みのはず
            NowLoading.Close();
        }
        stageView.Content.PageUpdate(StageCache[stageView.SelectedTab][stageView.Content.Page],
            LastStageCacheID[stageView.SelectedTab]);
    }

    // オブジェクトを配置してPrevCamで表示する
    public void CreatePreview(Stage stage)
    {
        if (PrevStage != null) PrevStage.Destroy();
        PrevStage = stage;
        stage.Create();
        PrevCam.transform.position = stage.Start.Position
            + new Vector3(0, GameConst.PLAY_CAMDIST_VER + GameConst.BALL_SCALE, -GameConst.PLAY_CAMDIST_HOR);
    }

    public void DestroyPreview()
    {
        if (PrevStage != null) PrevStage.Destroy();
    }

    // 順番にStructureのPreviewを取得していく
    IEnumerator GetStructPreviews()
    {
        NowLoading.Show(canvas.transform, "Loading assets...");

        for (StructureType type = StructureType.Zero; type < StructureType.NB; ++type)
            for (int texture = 0, cnt = Prefabs.GetTextureCount(type); texture < cnt; ++texture)
            {
                CreateStructPreview(new Structure(type, texture));
                yield return new WaitForEndOfFrame();

                var copy = new RenderTexture(StructPrevRT);
                Graphics.CopyTexture(StructPrevRT, copy);
                Cache.StructPreviews[type].Add(copy);
            }

        //CreateStructPreview(new Structure(StructureType.Chopsticks, 0));

        NowLoading.Close();
    }

    // オブジェクトを配置してStructCamで表示する
    public void CreateStructPreview(Structure str)
    {
        if (PrevStruct != null) PrevStruct.Destroy();
        PrevStruct = str;
        var (pos, rot) = str.SetForPreview();
        str.CreateForPreview();
        StructCam.transform.position = pos;
        StructCam.transform.rotation = rot;
    }

}

public enum MenuPage
{
    Unset,
    Find,
    Create,
    Result,
}