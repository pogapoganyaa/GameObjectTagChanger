#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;

namespace PogapogaEditor.Component
{
    public class GameObjectTagChanger : MonoBehaviour
    {
        [HideInInspector] public string tagName = "Untagged";
        public List<GameObject> targetObjects = new List<GameObject>();

        public void ChangeGameObjectTag()
        {
            foreach (GameObject targetObject in targetObjects)
            {
                if (targetObject == null)
                    continue;
                targetObject.tag = tagName;
            }
        }
    }
}
#endif