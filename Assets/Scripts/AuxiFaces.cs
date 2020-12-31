using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AuxiFaces
{
    const float AUXIFACE_THICKNESS = 0.03f;

    public Stage Stage { get; private set; }
    public Structure Target { get; private set; }

    private List<GameObject> faces;
    private CubeFace AuxiFaceFace;
    private bool IsPosition2;

    public AuxiFaces(Stage stage, Structure target, CubeFace face, bool isPos2)
    {
        Stage = stage;
        Target = target;
        AuxiFaceFace = face;
        IsPosition2 = isPos2;
        faces = new List<GameObject>();
    }

    public void Destroy()
    {
        faces.ForEach(i => Object.Destroy(i));
    }

    // 補助面を更新
    public void Update()
    {
        faces.ForEach(i => Object.Destroy(i));
        faces.Clear();
        foreach (var xyz in AuxiFaceFace == CubeFace.NB ?
            new XYZEnum[] { XYZEnum.X, XYZEnum.Y, XYZEnum.Z } : new XYZEnum[] { (XYZEnum)((int)AuxiFaceFace % 3) })
        {
            var cri = new int[] {
            (IsPosition2 ? Target.PositionInt2.XYZ(xyz) : Target.PositionInt.XYZ(xyz)) + Target.LocalScaleInt.XYZ(xyz),
            (IsPosition2 ? Target.PositionInt2.XYZ(xyz) : Target.PositionInt.XYZ(xyz)) - Target.LocalScaleInt.XYZ(xyz) };
            var quads = new bool[] { false, false };
            foreach (var str in Stage.Structs)
            {
                if (Target == str) continue; // TODO: 自身のPosition2との比較も行う
                var tar = new int[] {
                str.PositionInt.XYZ(xyz) + str.LocalScaleInt.XYZ(xyz),
                str.PositionInt.XYZ(xyz) - str.LocalScaleInt.XYZ(xyz) };
                for (int i = 0; i < 2; ++i) foreach (var j in tar)
                    {
                        if (cri[i] == j)
                        {
                            quads[i] = true;
                            var face = Object.Instantiate(Prefabs.AuxiFacePrefab);
                            face.transform.position = Structure.ToPositionF(str.PositionInt.NewXYZ(xyz, j));
                            face.transform.localScale = str.LocalScale.NewXYZ(xyz, AUXIFACE_THICKNESS);
                            faces.Add(face);
                        }
                    }
                if (str.HasPosition2)
                {
                    // TODO: LocalScale2が違うときも対応する
                }
            }
            for (int i = 0; i < 2; ++i)
            {
                if (quads[i])
                {
                    var face = Object.Instantiate(Prefabs.AuxiFacePrefab);
                    face.transform.position = Structure.ToPositionF(Target.PositionInt.NewXYZ(xyz, cri[i]));
                    face.transform.localScale = Target.LocalScale.NewXYZ(xyz, AUXIFACE_THICKNESS);
                    faces.Add(face);
                }
            }
        }
    }


}
