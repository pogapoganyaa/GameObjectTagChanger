#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using VRC.SDKBase;

namespace PogapogaEditor.Component
{
    /// <summary>
    /// List内のGameObjectのTagをまとめて変更します。
    /// VRChatSDK環境のみを動作対象としています。(IEditorOnlyを使用しているため)
    /// </summary>
    public class GameObjectTagChangerVRChatOnly : MonoBehaviour, IEditorOnly
    {
        #region // Tag変更処理用
        public string[] tagNames = new string[] { "Untagged", "EditorOnly" };
        public GameObject[] targetObjects;
        #endregion

        #region // 設定の対象・対象外処理用
        public Dictionary<GameObject, bool> targetFlagDict = new Dictionary<GameObject, bool>();
        public bool targetFlagAllEnabled = true;
        public GameObject[] flagKeys;
        public bool[] flagValues;
        #endregion

        #region // 検索用
        public GameObject rootObject;
        public string searchTagName = "Untagged";
        public string searchName = "◯◯";
        #endregion

        /// <summary>
        /// Tagの変更処理
        /// </summary>
        public void ChangeGameObjectTag(string _tagName)
        {
            Undo.RecordObjects(targetObjects.ToArray(), "GameObjectTagChangerVRChatOnly");
            foreach (GameObject targetObject in targetObjects)
            {
                if (targetObject == null)
                    continue;
                targetObject.tag = _tagName;
            }
            Debug.Log($"Tagを{_tagName}に設定しました");
        }

        #region 検索処理
        public void SearchTagObject(string _searchTagName)
        {
            Undo.RecordObject(this, "GameObjectTagChangerVRChatOnly");
            targetObjects = rootObject.GetComponentsInChildren<Transform>(true)
                        .Where(t => t.gameObject.tag == _searchTagName)
                        .Select(t => t.gameObject)
                        .ToArray();

            ResetDictionaryFlags();
            Debug.Log($"RootObjectからTagが{searchTagName}のものを取得しました");
        }

        public void SearchContainsNameObject()
        {
            Undo.RecordObject(this, "GameObjectTagChangerVRChatOnly");
            targetObjects = rootObject.GetComponentsInChildren<Transform>(true)
                        .Where(t => t.name.Contains(searchName) == true)
                        .Select(t => t.gameObject)
                        .ToArray();

            ResetDictionaryFlags();
            Debug.Log($"RootObjectから{searchName}を含むGameObjectを取得しました");
        }

        public void SearchStartWishNameObject()
        {
            Undo.RecordObject(this, "GameObjectTagChangerVRChatOnly");
            targetObjects = rootObject.GetComponentsInChildren<Transform>(true)
                        .Where(t => t.name.StartsWith(searchName) == true)
                        .Select(t => t.gameObject)
                        .ToArray();

            ResetDictionaryFlags();
            Debug.Log($"RootObjectから{searchName}で始まるGameObjectを取得しました");
        }
        public void SearchEndsWithNameObject()
        {
            Undo.RecordObject(this, "GameObjectTagChangerVRChatOnly");
            targetObjects = rootObject.GetComponentsInChildren<Transform>(true)
                        .Where(t => t.name.EndsWith(searchName) == true)
                        .Select(t => t.gameObject)
                        .ToArray();

            ResetDictionaryFlags(); 
            Debug.Log($"RootObjectから{searchName}で終わるGameObjectを取得しました");
        }
        public void GetChildObjects()
        {
            Undo.RecordObject(this, "GameObjectTagChangerVRChatOnly");
            List<GameObject> tmpGameObjects = new List<GameObject>();
            for (int i = 0; i < rootObject.transform.childCount; i++)
            {
                tmpGameObjects.Add(rootObject.transform.GetChild(i).gameObject);
            }
            targetObjects = tmpGameObjects.ToArray();
            ResetDictionaryFlags();
            Debug.Log($"RootObjectの子のGameObjectを取得しました");
        }
        public void GetAllObjects()
        {
            Undo.RecordObject(this, "GameObjectTagChangerVRChatOnly");
            List<GameObject> tmpGameObjects;
            tmpGameObjects = rootObject.GetComponentsInChildren<GameObject>(true).ToList();
            tmpGameObjects.Remove(rootObject);
            targetObjects = tmpGameObjects.ToArray();
            ResetDictionaryFlags();
            Debug.Log($"RootObject下の全てのGameObjectを取得しました");
        }
        #endregion

        #region 変更対象管理用Dictionaryの更新
        public void ResetDictionaryFlags()
        {
            targetFlagAllEnabled = true;
            targetFlagDict.Clear();
            for (int i = 0; i < targetObjects.Length; i++)
            {
                if (targetFlagDict.ContainsKey(targetObjects[i]) == true) { continue; }
                targetFlagDict.Add (targetObjects[i].gameObject, true);
            }
            SaveDictionary();
        }

        public void UpdateDictionaryFlags()
        {
            for (int i = 0; i < flagKeys.Length; i++)
            {
                if (targetObjects.Contains(flagKeys[i]) == false)
                {
                    targetFlagDict.Remove(flagKeys[i]);
                }
            }

            for (int i = 0;i < targetObjects.Length; i++)
            {
                if(targetFlagDict.ContainsKey(targetObjects[i]) == true) { continue; }
                targetFlagDict.Add(targetObjects[i].gameObject, true);
            }
            SaveDictionary();
        }
        #endregion

        #region // Dictionaryに関する処理
        public void SaveDictionary()
        {
            flagKeys = targetFlagDict.Keys.ToArray();
            flagValues = targetFlagDict.Values.ToArray();
        }
        public void LoadDictionary()
        {
            targetFlagDict.Clear();
            for (int i = 0; i<flagKeys.Length; i++)
            {
                targetFlagDict.Add(flagKeys[i], flagValues[i]);
            }
        }
        #endregion

        private void OnValidate()
        {
            UpdateDictionaryFlags();
        }
    }

    [CustomEditor(typeof(GameObjectTagChangerVRChatOnly))]
    public class GameObjectTagChangerVRChatOnlyEditor : Editor
    {
        private GameObjectTagChangerVRChatOnly tagChanger;

        ReorderableList _targetObjects;
        SerializedProperty _tagNames;
        SerializedProperty _searchTagName;

        private float _spaceHeight = 10;
        private float _toggleWidh = 20;

        private bool _isOpenTargetList = false;
        private bool _isOpenSearch;
        private bool _isOpenSearchTagName;
        private bool _isOpenSearchObjectName;
        private bool _isOpenEnabled;
        private bool _isOpenOther;
        private bool _isOpenHierarchy;
        private bool _targetFlagAllEnabled;

        public void OnEnable()
        {
            tagChanger = (GameObjectTagChangerVRChatOnly)target;
            // Dictionaryの復元処理
            tagChanger.LoadDictionary();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            _tagNames = serializedObject.FindProperty("tagNames");
            _searchTagName = serializedObject.FindProperty("searchTagName");

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((MonoBehaviour)target), typeof(MonoScript), false);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.LabelField("List内のGameObjectのTagを変更します", EditorStyles.boldLabel);


            #region // Tagの表示
            for (int i = 0; i < tagChanger.tagNames.Length; i++)
            {
                tagChanger.tagNames[i] = EditorGUILayout.TagField($"設定するTag{i}を選択してください", _tagNames.GetArrayElementAtIndex(i).stringValue);
            }
            #endregion

            #region // Listの表示
            SerializedProperty targetProperty = serializedObject.FindProperty(nameof(tagChanger.targetObjects));
            EditorGUILayout.PropertyField(targetProperty);
            #endregion

            #region // Listの並べ替え
            _isOpenTargetList = EditorGUILayout.BeginFoldoutHeaderGroup(_isOpenTargetList, "Listの並べ替え");
            if (_isOpenTargetList)
            {
                if (_targetObjects == null)
                {
                    _targetObjects = new ReorderableList(serializedObject, targetProperty);
                }

                _targetObjects.drawHeaderCallback = rect => EditorGUI.LabelField(rect, "編集対象のGameObject");
                _targetObjects.drawElementCallback = (rect, index, isActive, isFcused) =>
                {
                    SerializedProperty elementProperty = targetProperty.GetArrayElementAtIndex(index);
                    rect.height = EditorGUIUtility.singleLineHeight;
                    EditorGUI.PropertyField(rect, elementProperty, new GUIContent(""));
                };
                _targetObjects.DoLayoutList();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            #endregion


            #region // 実行用ボタン
            EditorGUILayout.Space(_spaceHeight);
            for (int i = 0; i < tagChanger.tagNames.Length; i++)
            {
                if (GUILayout.Button($"Tagを{tagChanger.tagNames[i]}に設定します"))
                {
                    bool dialogFlag = EditorUtility.DisplayDialog("タグの変更", $"Tagを{tagChanger.tagNames[i]}に設定します", "処理開始", "キャンセル");
                    if (dialogFlag == true)
                    {
                        foreach (GameObject targetObject in tagChanger.targetObjects) { if (targetObject != null) { Undo.RecordObject(targetObject, "Tagの変更"); } }
                        tagChanger.ChangeGameObjectTag(tagChanger.tagNames[i]);
                    }
                }
            }

            #endregion

            #region // 検索に関する項目
            _isOpenSearch = EditorGUILayout.Foldout(_isOpenSearch, "GameObjectの検索・取得に関する項目");
            if(_isOpenSearch)
            {
                // 検索用のRootObjectの設定
                tagChanger.rootObject = EditorGUILayout.ObjectField("検索対象のRootObject", tagChanger.rootObject, typeof(GameObject), true) as GameObject;

                #region // Tagで検索
                EditorGUI.indentLevel++;
                _isOpenSearchTagName = EditorGUILayout.Foldout(_isOpenSearchTagName, "Tagで検索");
                if (_isOpenSearchTagName == true)
                {
                    tagChanger.searchTagName = EditorGUILayout.TagField("検索するTagを選択してください", _searchTagName.stringValue);

                    if (GUILayout.Button($"RootObjectからTagが{tagChanger.searchTagName}のものを取得します"))
                    {
                        if (tagChanger.rootObject == null) { Debug.LogWarning("RootObjectが設定されていません"); return; }
                        bool dialogFlag = EditorUtility.DisplayDialog("GameObjectの取得", $"RootObjectからTagが{tagChanger.searchTagName}のものを取得します", "処理開始", "キャンセル");
                        if (dialogFlag == true)
                        {
                            tagChanger.SearchTagObject(tagChanger.searchTagName);
                        }
                    }
                }
                EditorGUI.indentLevel--;
                #endregion

                #region // 文字列で検索
                EditorGUI.indentLevel++;
                _isOpenSearchObjectName = EditorGUILayout.Foldout(_isOpenSearchObjectName, "文字列で検索");
                if (_isOpenSearchObjectName == true)
                {
                    EditorGUI.indentLevel++;
                    tagChanger.searchName = EditorGUILayout.TextField("検索用文字列", tagChanger.searchName);
                    if (GUILayout.Button($"RootObjectから{tagChanger.searchName}を含むGameObjectを検索"))
                    {
                        if (tagChanger.rootObject == null) { Debug.LogWarning("RootObjectが設定されていません"); return; }
                        bool dialogFlag = EditorUtility.DisplayDialog("GameObjectの取得", $"RootObjectから{tagChanger.searchName}を含むGameObjectを取得します", "処理開始", "キャンセル");
                        if (dialogFlag == true)
                        {
                            tagChanger.SearchContainsNameObject();
                        }
                    }
                    if (GUILayout.Button($"RootObjectから{tagChanger.searchName}で始まるGameObjectを検索"))
                    {
                        if (tagChanger.rootObject == null) { Debug.LogWarning("RootObjectが設定されていません"); return; }
                        bool dialogFlag = EditorUtility.DisplayDialog("GameObjectの取得", $"RootObjectから{tagChanger.searchName}で始まるGameObjectを取得します", "処理開始", "キャンセル");
                        if (dialogFlag == true)
                        {
                            tagChanger.SearchStartWishNameObject();
                        }
                    }
                    if (GUILayout.Button($"RootObjectから{tagChanger.searchName}で終わるGameObjectを検索"))
                    {
                        if (tagChanger.rootObject == null) { Debug.LogWarning("RootObjectが設定されていません"); return; }
                        bool dialogFlag = EditorUtility.DisplayDialog("GameObjectの取得", $"RootObjectから{tagChanger.searchName}で終わるGameObjectを取得します", "処理開始", "キャンセル");
                        if (dialogFlag == true)
                        {
                            tagChanger.SearchEndsWithNameObject(); 
                            EditorUtility.SetDirty(tagChanger);
                        }
                    }
                    EditorGUI.indentLevel--;
                }
                #region // 階層で取得
                _isOpenHierarchy = EditorGUILayout.Foldout(_isOpenHierarchy, "階層で取得");
                if (_isOpenHierarchy == true)
                {
                    EditorGUI.indentLevel++;
                    if (GUILayout.Button("RootObjectの子のGameObjectを取得"))
                    {
                        tagChanger.GetChildObjects();
                    }
                    if (GUILayout.Button("RootObject下の全てのGameObjectを取得"))
                    {
                        tagChanger.GetAllObjects();
                    }
                    EditorGUI.indentLevel--;
                }
                EditorGUI.indentLevel--;
                #endregion


            }
            #endregion

            #region // 対象・対象外設定
            _isOpenEnabled = EditorGUILayout.Foldout(_isOpenEnabled, "Tagの変更処理の対象・対象外設定");
            if (_isOpenEnabled)
            {
                if (tagChanger.targetObjects.Length == 0)
                {
                    EditorGUILayout.HelpBox("対象となるGameObjectが設定されていません", MessageType.Warning);
                }

                EditorGUI.BeginChangeCheck();
                for (int i = 0; i < tagChanger.targetObjects.Length; i++)
                {
                    GameObject keyObject;
                    keyObject = tagChanger.targetObjects[i];

                    if (i == 0)
                    {
                        _targetFlagAllEnabled = EditorGUILayout.Toggle(tagChanger.targetFlagAllEnabled, GUILayout.Width(_toggleWidh));
                        if (_targetFlagAllEnabled != tagChanger.targetFlagAllEnabled)
                        {
                            tagChanger.targetFlagAllEnabled = _targetFlagAllEnabled;
                            GameObject[] keyObjects = tagChanger.targetFlagDict.Keys.ToArray();
                            for (int ki = 0; ki < keyObjects.Length; ki++)
                            {
                                tagChanger.targetFlagDict[keyObjects[ki]] = tagChanger.targetFlagAllEnabled;
                            }
                        }
                    }

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        if (keyObject == null) { continue; }
                        if (tagChanger.targetFlagDict.ContainsKey(keyObject) == false)
                        {
                            continue;
                        }

                        bool tmpFlag = EditorGUILayout.Toggle(tagChanger.targetFlagDict[keyObject], GUILayout.Width(_toggleWidh));
                        tagChanger.targetFlagDict[keyObject] = tmpFlag;
                        EditorGUILayout.ObjectField(keyObject, typeof(GameObject), true);
                    }
                }
                if (EditorGUI.EndChangeCheck())
                {
                    tagChanger.SaveDictionary();
                    EditorUtility.SetDirty(tagChanger);
                }
            }
            #endregion

            #region // その他
            _isOpenOther = EditorGUILayout.Foldout(_isOpenOther, "その他");
            if ( _isOpenOther )
            {
                if (tagChanger.targetObjects.Length == 0)
                {
                    EditorGUILayout.HelpBox("対象となるGameObjectが設定されていません", MessageType.Warning);
                }
                else
                {
                    if (GUILayout.Button("List内のGameObjectを選択状態にします")) 
                    {
                        Selection.objects = tagChanger.targetObjects.ToArray();
                        Debug.Log("List内のGameObjectを選択状態にしました");
                    }
                }
            }
            #endregion
            serializedObject.ApplyModifiedProperties();
        }  
    }
}
#endif