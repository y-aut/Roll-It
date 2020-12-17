using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ResultOperator : MonoBehaviour
{
    public Canvas canvas;
    public TextMeshProUGUI TxtStageName;
    public TextMeshProUGUI TxtAuthorName;
    public Button BtnNext;
    public GameObject BtnGood;
    public GameObject BtnNeutral;
    public GameObject BtnBad;

    public Sprite SprGood;
    public Sprite SprNeutral;
    public Sprite SprBad;
    public Sprite SprGoodGray;
    public Sprite SprNeutralGray;
    public Sprite SprBadGray;

    public static Stage Stage { get; set; }
    private EvaluationEnum Selected;

    private GameObject BtnEvaluation(EvaluationEnum eva)
    {
        if (eva == EvaluationEnum.Good) return BtnGood;
        else if (eva == EvaluationEnum.Neutral) return BtnNeutral;
        else return BtnBad;
    }

    private Sprite SprEvaluation(EvaluationEnum eva)
    {
        if (eva == EvaluationEnum.Good) return SprGood;
        else if (eva == EvaluationEnum.Neutral) return SprNeutral;
        else return SprBad;
    }

    private Sprite SprEvaluationGray(EvaluationEnum eva)
    {
        if (eva == EvaluationEnum.Good) return SprGoodGray;
        else if (eva == EvaluationEnum.Neutral) return SprNeutralGray;
        else return SprBadGray;
    }

    // Start is called before the first frame update
    void Start()
    {
        Selected = EvaluationEnum.None;
        TxtStageName.text = Stage.Name;
        _ = FirebaseIO.IncrementClearCount(Stage.ID);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void BtnGoodClick() => BtnEvaluationClick(EvaluationEnum.Good);
    public void BtnNetralClick() => BtnEvaluationClick(EvaluationEnum.Neutral);
    public void BtnBadClick() => BtnEvaluationClick(EvaluationEnum.Bad);

    public void BtnEvaluationClick(EvaluationEnum eva)
    {
        if (Selected == eva) return;
        BtnEvaluation(eva).GetComponent<Image>().sprite = SprEvaluation(eva);
        if (Selected == EvaluationEnum.None)
            BtnNext.interactable = true;
        else
            BtnEvaluation(Selected).GetComponent<Image>().sprite = SprEvaluationGray(Selected);
        Selected = eva;
    }

    public async void BtnNextClick()
    {
        NowLoading.Show(canvas.transform, "Connecting...");
        if (Selected == EvaluationEnum.Good)
        {
            await FirebaseIO.IncrementPosEvaCount(Stage.ID);
        }
        else if (Selected == EvaluationEnum.Bad)
        {
            await FirebaseIO.IncrementNegEvaCount(Stage.ID);
        }
        NowLoading.Close(() =>
        {
            Scenes.LoadScene(SceneType.Select);
        });
    }

}
