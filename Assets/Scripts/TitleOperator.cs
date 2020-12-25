using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleOperator : MonoBehaviour
{
    public void BtnStartClicked()
    {
        Scenes.LoadScene(SceneType.Menu);
    }
}
