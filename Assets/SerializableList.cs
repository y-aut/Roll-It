using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SerializableList<T> : List<T>, ISerializationCallbackReceiver
{
    [SerializeField]
    private List<string> jsons;

    public void OnBeforeSerialize()
    {
        jsons = new List<string>();
        ForEach(i => jsons.Add(JsonUtility.ToJson(i)));
    }

    public void OnAfterDeserialize()
    {
        jsons.ForEach(i => Add(JsonUtility.FromJson<T>(i)));
    }

}
