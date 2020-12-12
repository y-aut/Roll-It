using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NowLoading : MonoBehaviour
{
    public Image ImgIcon;
    public TextMeshProUGUI TxtMessage;
    public Popup popup;

    private int generation = 0;
    private string message;
    private static GameObject instance = null;

    public static bool Visible { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        TxtMessage.text = message;
    }

    // Update is called once per frame
    void Update()
    {
        if (++generation % 50 == 0)
            ImgIcon.transform.rotation = Quaternion.Euler(0, 0, -generation / 50 * 45);
    }

    public static void Show(Transform parent, string msg)
    {
        Visible = true;
        instance = Instantiate(Prefabs.NowLoadingPrefab, parent, false);
        var script = instance.GetComponent<NowLoading>();
        script.message = msg;
        script.popup.Open();
    }

    public static void Close(Action after = null)
    {
        after = after ?? (() => { });
        Visible = false;
        var script = instance.GetComponent<NowLoading>();
        script.popup.Close();
        script.StartCoroutine(script.WaitClose(after));
    }

    private IEnumerator WaitClose(Action after)
    {
        while (popup.IsClosing) yield return new WaitForSeconds(0.1f);
        after();
        Destroy(instance);
    }
}
