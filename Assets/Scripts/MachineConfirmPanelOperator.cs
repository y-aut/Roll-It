using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MachineConfirmPanelOperator : MonoBehaviour
{
    public Popup popup;
    private MachineType Type;
    private Transform parent;

    public static void ShowDialog(Transform parent, MachineType type)
    {
        var panel = Instantiate(Prefabs.MachineConfirmPanelPrefab, parent, false);
        var script = panel.GetComponent<MachineConfirmPanelOperator>();

        script.parent = parent;
        script.Type = type;

        script.Initialize();
        script.popup.Open();
    }

    private void Initialize()
    {
        
    }

    public void BtnStartClicked()
    {

    }

    public void BtnCloseClicked()
    {
        popup.CloseAndDestroy();
    }

}

public enum MachineType
{
    Texture, Structure
}