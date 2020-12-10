using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Firebase;
using Firebase.Database;

public class MenuOperator : MonoBehaviour
{
    public Canvas canvas;

    private static bool firstTime = true;   // 起動時に一度だけ処理するため

    private void Awake()
    {
        if (firstTime)
        {
            firstTime = false;
            GameData.Load();

            // SDK で他のメソッドを呼び出す前に Google Play 開発者サービスを確認し、必要であれば、Firebase Unity SDK で必要とされるバージョンに更新します。
            // https://firebase.google.com/docs/unity/setup?hl=ja#prerequisites
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
            {
                var dependencyStatus = task.Result;
                if (dependencyStatus == DependencyStatus.Available)
                {
                    // Create and hold a reference to your FirebaseApp,
                    // where app is a Firebase.FirebaseApp property of your application class.
                    FirebaseIO.Root = FirebaseDatabase.DefaultInstance.RootReference;

                    // Set a flag here to indicate whether Firebase is ready to use by your app.
                    FirebaseIO.Available = true;
                }
                else
                {
                    Debug.LogError(string.Format(
                      "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                    // Firebase Unity SDK is not safe to use here.
                }
            });

            // ユーザー名とIDを決める
            if (GameData.User.Name == "")
            {
                InputBox.ShowDialog(result =>
                {
                    GameData.User.Name = result;
                    FirebaseIO.RegisterUser(GameData.User);
                    GameData.Save();
                }, canvas.transform, "Enter your nickname", allowCancel: false);
            }
            else if (GameData.User.ID == IDType.Empty)
            {
                FirebaseIO.RegisterUser(GameData.User);
            }

        }
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void BtnFindClicked()
    {
        SelectOperator.IsMyStages = false;
        SceneManager.LoadScene("Select Scene");
    }

    public void BtnCreateClicked()
    {
        SelectOperator.IsMyStages = true;
        SceneManager.LoadScene("Select Scene");
    }
}
