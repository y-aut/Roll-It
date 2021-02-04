using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

public static class CreateMesh
{
	public static Vector3 NewY2(this Vector3 vec, float y) => new Vector3(vec.x, y, vec.z);

	[MenuItem("Extensions/Create Funnel Mesh", priority = 0)]
	public static void CreateConeMesh()
	{
		// Funnelのメッシュを作成
		// 側面数(n)
		const int SIDE_COUNT = 64;
		// 厚み
		const float THICKNESS = 0.1f;
		// 上側半径
		const float UPPER_R = 0.5f;
		// 下側半径
		const float LOWER_R = 0.4f;

		Mesh mesh = new Mesh();

		// 上面内側,下面内側,上面外側,下面外側
		var vertices = new Vector3[SIDE_COUNT * 4];
		for (int i = 0; i < SIDE_COUNT; ++i)
        {
			var theta = 2 * Mathf.PI * i / SIDE_COUNT;
			var e = new Vector3(Mathf.Cos(theta), 0, Mathf.Sin(theta));
			vertices[i] = (e * UPPER_R).NewY2(0.5f);
			vertices[SIDE_COUNT + i] = (e * LOWER_R).NewY2(-0.5f);
			vertices[SIDE_COUNT * 2 + i] = (e * (UPPER_R + THICKNESS)).NewY2(0.5f);
			vertices[SIDE_COUNT * 3 + i] = (e * (LOWER_R + THICKNESS)).NewY2(-0.5f);
		}
		mesh.vertices = vertices;

		var triangles = new List<int>();
		for (int i = 0; i < SIDE_COUNT; ++i)
        {
			var nexti = (i + 1) % SIDE_COUNT;
			triangles.AddRange(new int[] {
				// 側面内側
				i, SIDE_COUNT + i, SIDE_COUNT + nexti, nexti, i, SIDE_COUNT + nexti,
				// 側面外側
				SIDE_COUNT * 2 + i, SIDE_COUNT * 3 + nexti, SIDE_COUNT * 3 + i, SIDE_COUNT * 2 + i, SIDE_COUNT * 2 + nexti, SIDE_COUNT * 3 + nexti,
				// 上面
				i, SIDE_COUNT * 2 + nexti, SIDE_COUNT * 2 + i, i, nexti, SIDE_COUNT * 2 + nexti,
				// 下面
				SIDE_COUNT + i, SIDE_COUNT * 3 + i, SIDE_COUNT * 3 + nexti, SIDE_COUNT + nexti, SIDE_COUNT + i, SIDE_COUNT * 3 + nexti, });
		}
		mesh.triangles = triangles.ToArray();

		mesh.RecalculateBounds();
		mesh.RecalculateNormals();

		AssetDatabase.CreateAsset(mesh, "Assets/Primitives/Funnel/mesh.mesh");
		AssetDatabase.SaveAssets();
	}

}
