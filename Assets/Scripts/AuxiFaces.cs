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
        faces.Clear();
    }

    // 補助面を更新
    public void Update()
    {
        Destroy();

        var xyzRange = new List<XYZEnum>();
        if ((AuxiFaceFace & CubeFace.X) != 0) xyzRange.Add(XYZEnum.X);
        if ((AuxiFaceFace & CubeFace.Y) != 0) xyzRange.Add(XYZEnum.Y);
        if ((AuxiFaceFace & CubeFace.Z) != 0) xyzRange.Add(XYZEnum.Z);
        foreach (var xyz in xyzRange)
        {
            var cri = new int[] {
            (IsPosition2 ? Target.PositionInt2.XYZ(xyz) : Target.PositionInt.XYZ(xyz)) + Target.LocalScaleInt.XYZ(xyz),
            (IsPosition2 ? Target.PositionInt2.XYZ(xyz) : Target.PositionInt.XYZ(xyz)) - Target.LocalScaleInt.XYZ(xyz) };
            var quads = new bool[] { false, false };
            var iRange = new List<int>();
            if ((AuxiFaceFace & xyz.ToCubeFace() & CubeFace.P) != 0) iRange.Add(0);
            if ((AuxiFaceFace & xyz.ToCubeFace() & CubeFace.N) != 0) iRange.Add(1);

            foreach (var str in Stage.Structs)
            {
                if (Target == str) continue; // TODO: 自身のPosition2との比較も行う
                var tar = new int[] {
                str.PositionInt.XYZ(xyz) + str.LocalScaleInt.XYZ(xyz),
                str.PositionInt.XYZ(xyz) - str.LocalScaleInt.XYZ(xyz) };
                foreach (var i in iRange) foreach (var j in tar)
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
                if (str.Type.HasPosition2())
                {
                    // TODO: LocalScale2が違うときも対応する
                }
            }
            foreach (var i in iRange)
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
