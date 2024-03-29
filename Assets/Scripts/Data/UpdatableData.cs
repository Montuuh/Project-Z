using UnityEditor;
using UnityEngine;

public class UpdatableData : ScriptableObject
{
    public event System.Action OnValuesUpdated;
    public bool autoUpdate;
    
    protected virtual void OnValidate()
    {
        if (autoUpdate)
            EditorApplication.update += NotifyOfUpdatedValues;
    }

    public void NotifyOfUpdatedValues()
    {
        EditorApplication.update -= NotifyOfUpdatedValues;
        if (OnValuesUpdated != null)
            OnValuesUpdated();
    }
}
