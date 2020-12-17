using UnityEditor;
using UnityEditor.SceneManagement;

public static class SceneLauncher
{
	[MenuItem("シーン/Menu Scene", priority = 0)]
	public static void OpenMenuScene()
	{
		EditorSceneManager.OpenScene("Assets/Scenes/Menu Scene.unity", OpenSceneMode.Single);
	}

	[MenuItem("シーン/Create Scene", priority = 0)]
	public static void OpenCreateScene()
	{
		EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
		EditorSceneManager.OpenScene("Assets/Scenes/Create Scene.unity", OpenSceneMode.Single);
	}

	[MenuItem("シーン/Select Scene", priority = 0)]
	public static void OpenSelectScene()
	{
		EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
		EditorSceneManager.OpenScene("Assets/Scenes/Select Scene.unity", OpenSceneMode.Single);
	}

	[MenuItem("シーン/Play Scene", priority = 0)]
	public static void OpenPlayScene()
	{
		EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
		EditorSceneManager.OpenScene("Assets/Scenes/Play Scene.unity", OpenSceneMode.Single);
	}

	[MenuItem("シーン/Result Scene", priority = 0)]
	public static void OpenResultScene()
	{
		EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
		EditorSceneManager.OpenScene("Assets/Scenes/Result Scene.unity", OpenSceneMode.Single);
	}

}
