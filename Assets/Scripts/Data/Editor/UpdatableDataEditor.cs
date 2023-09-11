using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UpdatableData), true)]
public class UpdatableDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        UpdatableData data = (UpdatableData)target;

        if (DrawDefaultInspector() && data.autoUpdate)
        {
            Debug.Log("AutoUpdate");
            data.NotifyOfUpdatedValues();
        }
        
        if (GUILayout.Button("Update"))
        {
            Debug.Log("Update");
            data.NotifyOfUpdatedValues();
        }
    }
}
