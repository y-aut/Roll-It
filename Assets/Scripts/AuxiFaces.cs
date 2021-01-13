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
            int[] cri;
            if (IsPosition2)
            {
                cri = new int[] {
                    Target.PositionInt2.XYZ(xyz) + Target.LocalScaleInt2.XYZ(xyz),
                    Target.PositionInt2.XYZ(xyz) - Target.LocalScaleInt2.XYZ(xyz) };
            }
            else
            {
                cri = new int[] {
                    Target.PositionInt.XYZ(xyz) + Target.LocalScaleInt.XYZ(xyz),
                    Target.PositionInt.XYZ(xyz) - Target.LocalScaleInt.XYZ(xyz) };
            }
            var quads = new bool[] { false, false };
            var iRange = new List<int>();
            if ((AuxiFaceFace & xyz.ToCubeFace() & CubeFace.P) != 0) iRange.Add(0);
            if ((AuxiFaceFace & xyz.ToCubeFace() & CubeFace.N) != 0) iRange.Add(1);

            foreach (var str in Stage.Structs)
            {
                List<(int Value, bool IsPos2)> tar;
                if (Target == str && !IsPosition2) {
                    // 自身のPosition2との比較を行う
                    if (!str.Type.HasPosition2()) continue;
                    tar = new List<(int Value, bool IsPos2)>() {
                        (str.PositionInt2.XYZ(xyz) + str.LocalScaleInt2.XYZ(xyz), true),
                        (str.PositionInt2.XYZ(xyz) - str.LocalScaleInt2.XYZ(xyz), true) };
                }
                else
                {
                    tar = new List<(int Value, bool IsPos2)>() {
                        (str.PositionInt.XYZ(xyz) + str.LocalScaleInt.XYZ(xyz), false),
                        (str.PositionInt.XYZ(xyz) - str.LocalScaleInt.XYZ(xyz), false) };
                    if (str.Type.HasPosition2() && Target != str)
                    {
                        tar.AddRange(new List<(int Value, bool IsPos2)>() {
                            (str.PositionInt2.XYZ(xyz) + str.LocalScaleInt2.XYZ(xyz), true),
                            (str.PositionInt2.XYZ(xyz) - str.LocalScaleInt2.XYZ(xyz), true) });
                    }
                }
                foreach (var i in iRange) foreach (var j in tar)
                    {
                        if (cri[i] == j.Value)
                        {
                            quads[i] = true;
                            var face = Object.Instantiate(Prefabs.AuxiFacePrefab);
                            face.transform.position = Structure.ToPositionF(str.PositionIntOr2(j.IsPos2).NewXYZ(xyz, j.Value));
                            face.transform.localScale = str.LocalScaleOr2(j.IsPos2).NewXYZ(xyz, AUXIFACE_THICKNESS);
                            faces.Add(face);
                        }
                    }
            }
            foreach (var i in iRange)
            {
                if (quads[i])
                {
                    // 面を追加
                    var face = Object.Instantiate(Prefabs.AuxiFacePrefab);
                    face.transform.position = Structure.ToPositionF(Target.PositionIntOr2(IsPosition2).NewXYZ(xyz, cri[i]));
                    face.transform.localScale = Target.LocalScaleOr2(IsPosition2).NewXYZ(xyz, AUXIFACE_THICKNESS);
                    faces.Add(face);
                }
            }
        }
    }


}
