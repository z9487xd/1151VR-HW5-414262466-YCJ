using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Open the homework scene on a fresh checkout instead of showing an empty Untitled scene.
public static class OpenHomeworkScene
{
    [InitializeOnLoadMethod]
    static void Initialize()
    {
        if(Application.isBatchMode)return;
        EditorApplication.delayCall+=()=>{
            var current=EditorSceneManager.GetActiveScene();
            if(!EditorApplication.isPlayingOrWillChangePlaymode && string.IsNullOrEmpty(current.path) && !current.isDirty)
                Open();
        };
    }
    [MenuItem("HW5/Open first-person scene")]
    public static void Open()
    {
        if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
        EditorSceneManager.OpenScene("Assets/Scenes/VerdantValley.unity");
        if(SceneView.lastActiveSceneView)SceneView.lastActiveSceneView.LookAt(new Vector3(0,2,3),Quaternion.Euler(12,0,0),18);
    }
}
