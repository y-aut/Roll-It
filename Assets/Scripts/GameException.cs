using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

// エラーを管理する

[Serializable]
public class GameException : Exception
{
    // デバッグ用
    public static Exception Unreachable = new Exception("Assertion Failed: Unreachable");

    private static List<string> ErrorMessages = new List<string>()
    {
        "Failed to connect to the server.",
        "The request has timed out.",
        "Unable to connect to the network.",
    };

    public static Exception FirebaseUnavailable = new GameException(GameExceptionEnum.FirebaseUnavailable);
    public static Exception TimeOut = new GameException(GameExceptionEnum.TimeOut);
    public static Exception Offline = new GameException(GameExceptionEnum.Offline);

    public GameException() { }
    public GameException(GameExceptionEnum err) : base(ErrorMessages[(int)err]) { }

    // ネットワークに接続されていなければ例外をスロー
    public static void CheckNetwork()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
            throw Offline;
    }

}

public enum GameExceptionEnum
{
    FirebaseUnavailable,        // Firebaseが使用不可
    TimeOut,                    // タイムアウト
    Offline,                    // 接続不可能
}

public static partial class AddMethod
{
    public const int TIMEOUT_SECONDS = 10;

    // 一定時間が経過するとタイムアウトし、例外をスローする
    public static async Task<T> WaitWithTimeOut<T>(this Task<T> task)
    {
        var timeout = TimeSpan.FromSeconds(TIMEOUT_SECONDS);
        if (await Task.WhenAny(task, Task.Delay(timeout)) == task)
        {
            await task;     // 例外がスローされていればここで再スローする
            return task.Result;
        }
        else
            throw GameException.TimeOut;
    }

    public static async Task WaitWithTimeOut(this Task task)
    {
        var timeout = TimeSpan.FromSeconds(TIMEOUT_SECONDS);
        if (await Task.WhenAny(task, Task.Delay(timeout)) == task)
        {
            await task;     // 例外がスローされていればここで再スローする
            return;
        }
        else
            throw GameException.TimeOut;
    }

    public static void Show(this Exception e, Transform parent, Action action = null)
    {
        if (e.IsNotFoundException())
        {
            MessageBox.ShowDialog(parent, "The stage is not found.", MessageBoxType.OKOnly, action);
        }
        else
        {
            if (e.InnerException != null) e.InnerException.Show(parent, action);
            else MessageBox.ShowDialog(parent, e.Message, MessageBoxType.OKOnly, action);
        }
    }

    // Documentが見つからない例外
    public static bool IsNotFoundException(this Exception e)
    {
        const string MSG_BEG = "No document";
        if (e == null || e.Message.Length < MSG_BEG.Length) return false;
        else return e.Message.Substring(0, MSG_BEG.Length) == MSG_BEG;
    }

}