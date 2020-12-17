using System;
using System.Collections;
using System.Collections.Generic;
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
        "Failed to connect to the server.",
    };

    public GameException() { }
    public GameException(GameExceptionEnum err) : base(ErrorMessages[(int)err]) { }

    public void Show(Transform parent, Action action = null)
    {
        MessageBox.ShowDialog(parent, Message, MessageBoxType.OKOnly, action);
    }

}

public enum GameExceptionEnum
{
    FirebaseUnavailable,        // Firebaseが使用不可
    TimeOut,                    // タイムアウト
}
