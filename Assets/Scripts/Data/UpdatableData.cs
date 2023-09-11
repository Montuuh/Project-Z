using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdatableData : ScriptableObject
{
    public event System.Action OnValuesUpdated;
    public bool autoUpdate;

    public void NotifyOfUpdatedValues()
    {
        OnValuesUpdated?.Invoke();
    }
}
