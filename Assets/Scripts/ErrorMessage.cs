using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// エラーメッセージを表示する
public static class ErrorMessage
{
    // Firebaseが使用不可能である
    public static void FirebaseUnavailable(Transform parent)
    {
        MessageBox.ShowDialog(parent, "Failed to connect to the server.", MessageBoxType.OKOnly, () => { });
    }
}
