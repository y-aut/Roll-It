using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public static class Cache
{
    // 取得したユーザー情報
    private static List<User> Users = new List<User>();
    // 取得したステージ情報
    private static List<Stage> Stages = new List<Stage>();
    // StructureのPreview画像
    public static EnumCollection<StructureType, List<RenderTexture>> StructPreviews = new EnumCollection<StructureType, List<RenderTexture>>(_ => new List<RenderTexture>());

    // キャッシュがあればキャッシュから取得
    public static async Task<User> GetUser(IDType userID)
    {
        var user = Users.Find(i => i.ID == userID);
        if (user != null) return user;
        else
        {
            try
            {
                user = await FirebaseIO.GetUser(userID).WaitWithTimeOut();
            }
            catch (System.Exception e)
            {
                Debug.Log(e);
                user = User.NotFound(userID);
            }
            Users.Add(user);
            return user;
        }
    }

    public static async Task<Stage> GetStage(IDType stageID)
    {
        var stage = Stages.Find(i => i.ID == stageID);
        if (stage != null) return stage;
        else
        {
            try
            {
                stage = await FirebaseIO.GetStage(stageID).WaitWithTimeOut();
            }
            catch (System.Exception e)
            {
                Debug.Log(e.Message);
                stage = Stage.NotFound(stageID);
            }
            Stages.Add(stage);
            return stage;
        }
    }
}
