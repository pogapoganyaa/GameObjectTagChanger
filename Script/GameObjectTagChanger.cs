#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PogapogaEditor.Component
{
    public class GameObjectTagChanger : MonoBehaviour
    {
        [HideInInspector] public string tagName = "Untagged";
        public List<GameObject> targetObjects = new List<GameObject>();
        public GameObject rootObject;

        public void ChangeGameObjectTag()
        {
            foreach (GameObject targetObject in targetObjects)
            {
                if (targetObject == null)
                    continue;
                targetObject.tag = tagName;
            }
        }
        public void SearchTagObject()
        {
            targetObjects.Clear();
            targetObjects = rootObject.GetComponentsInChildren<Transform>(true)
                        .Where(t => t.gameObject.tag == tagName)
                        .Select(t => t.gameObject)
                        .ToList();
        }
    }
}
#endif