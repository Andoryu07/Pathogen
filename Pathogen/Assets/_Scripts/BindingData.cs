using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "BindingData", menuName = "Pathogen/BindingData")]
public class BindingData : ScriptableObject
{
    [System.Serializable]
    public class ActionBinding
    {
        public string actionName; 
        public KeyCode defaultKey;
    }

    public List<ActionBinding> defaultBindings;
}