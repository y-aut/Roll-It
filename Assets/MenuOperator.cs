using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    public void PlayStages()
    {
        
    }

    public void SelectStages()
    {
        SceneManager.LoadScene("Select Scene");
    }
}
