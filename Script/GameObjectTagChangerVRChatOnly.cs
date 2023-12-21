#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using VRC.SDK3.Dynamics.PhysBone.Components;
using VRC.SDKBase;

namespace PogapogaEditor.PogapogaComponent
{
    /// <summary>
    /// List内のGameObjectのTagをまとめて変更します。
    /// VRChatSDK環境のみを動作対象としています。(IEditorOnlyを使用しているため)
    /// </summary>
    public class GameObjectTagChangerVRChatOnly : MonoBehaviour, IEditorOnly
    {
        #region // Tag変更処理用
        public string[] tagNames = new string[] { "Untagged", "EditorOnly" };
        public GameObject[] targetObjects = new GameObject[0];
        #endregion

        #region // 設定の対象・対象外処理用
        public Dictionary<GameObject, bool> targetFlagDict = new Dictionary<GameObject, bool>();
        public bool targetFlagAllEnabled = true;
        public GameObject[] flagKeys = new GameObject[0];
        public bool[] flagValues = new bool[0];
        #endregion

        #region // 検索用
        public GameObject rootObject;
        public string searchTagName = "Untagged";
        public string searchName = "◯◯";
        public SkinnedMeshRenderer targetSkinnedMeshRenderer;
        public string searchComponentName = "◯◯";
        #endregion

        #region // FoldOut        
        public bool isOpenTargetList = false;
        public bool isOpenSearch = false;
        public bool isOpenSearchTagName = false;
        public bool isOpenSearchObjectName = false;
        public bool isOpenEnabled = false;
        public bool isOpenOther = false;
        public bool isOpenHierarchy = false;
        public bool isOpenSkinnedMeshRenderer = false;
        public bool isOpenCommponent = false;
        #endregion

        [Tooltip("確認用ダイアログの表示を省略する")] public bool skipConfirmation = false;

        /// <summary>
        /// Tagの変更処理
        /// </summary>
        public void ChangeGameObjectTag(string _tagName)
        {
            Undo.RecordObjects(targetObjects, nameof(GameObjectTagChangerVRChatOnly));
            foreach (GameObject targetObject in targetObjects)
            {
                if (targetObject == null) { continue; }
                
                if (targetFlagDict[targetObject] == true)
                {
                    targetObject.tag = _tagName;
                }
            }
            Debug.Log($"Tagを{_tagName}に設定しました");
        }

        #region 検索取得処理
        public void SearchTagObject(string _searchTagName)
        {
            Undo.RecordObject(this, nameof(GameObjectTagChangerVRChatOnly));
            targetObjects = rootObject.GetComponentsInChildren<Transform>(true)
                        .Where(t => t.gameObject.tag == _searchTagName)
                        .Select(t => t.gameObject)
                        .ToArray();

            ResetDictionaryFlags();
            Debug.Log($"RootObjectからTagが{searchTagName}のものを取得しました");
        }

        public void SearchContainsNameObject()
        {
            Undo.RecordObject(this, nameof(GameObjectTagChangerVRChatOnly));
            targetObjects = rootObject.GetComponentsInChildren<Transform>(true)
                        .Where(t => t.name.Contains(searchName) == true)
                        .Select(t => t.gameObject)
                        .ToArray();

            ResetDictionaryFlags();
            Debug.Log($"RootObjectから{searchName}を含むGameObjectを取得しました");
        }

        public void SearchStartWishNameObject()
        {
            Undo.RecordObject(this, nameof(GameObjectTagChangerVRChatOnly));
            targetObjects = rootObject.GetComponentsInChildren<Transform>(true)
                        .Where(t => t.name.StartsWith(searchName) == true)
                        .Select(t => t.gameObject)
                        .ToArray();

            ResetDictionaryFlags();
            Debug.Log($"RootObjectから{searchName}で始まるGameObjectを取得しました");
        }
        public void SearchEndsWithNameObject()
        {
            Undo.RecordObject(this, nameof(GameObjectTagChangerVRChatOnly));
            targetObjects = rootObject.GetComponentsInChildren<Transform>(true)
                        .Where(t => t.name.EndsWith(searchName) == true)
                        .Select(t => t.gameObject)
                        .ToArray();

            ResetDictionaryFlags(); 
            Debug.Log($"RootObjectから{searchName}で終わるGameObjectを取得しました");
        }
        public void GetChildObjects()
        {
            Undo.RecordObject(this, nameof(GameObjectTagChangerVRChatOnly));
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
            Undo.RecordObject(this, nameof(GameObjectTagChangerVRChatOnly));
            List<GameObject> tmpGameObjects;
            tmpGameObjects = rootObject.GetComponentsInChildren<Transform>(true).Select(t => t.gameObject).ToList();
            tmpGameObjects.Remove(rootObject);
            targetObjects = tmpGameObjects.ToArray();
            ResetDictionaryFlags();
            Debug.Log($"RootObject下の全てのGameObjectを取得しました");
        }
        public void GetSkinnedMeshRendererBonesObjects()
        {
            Undo.RecordObject(this, nameof(GameObjectTagChangerVRChatOnly));
            targetObjects = targetSkinnedMeshRenderer.bones.Select(b => b.gameObject).ToArray(); 
            ResetDictionaryFlags();
            Debug.Log($"SkinnedMeshRendererのBonesのGameObjectを取得しました");
        }
        public void SearchComponentName(string componentName)
        {
            Undo.RecordObject(this, nameof(GameObjectTagChangerVRChatOnly));
            targetObjects = rootObject.GetComponentsInChildren<Component>(true).Where(c => c.GetType().Name == componentName).Select(c => c.gameObject).ToArray();
            ResetDictionaryFlags();
            Debug.Log($"{componentName}のGameObjectを取得しました");
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
            Undo.RecordObject(this, nameof(GameObjectTagChangerVRChatOnly));
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

        // 初回にOnInspectorGUIよりも先に処理が走る
        private void OnValidate()
        {
            LoadDictionary();
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

        private bool _isOpenTargetList;
        private bool _isOpenSearch;
        private bool _isOpenSearchTagName;
        private bool _isOpenSearchObjectName;
        private bool _isOpenEnabled;
        private bool _isOpenOther;
        private bool _isOpenHierarchy;
        private bool _targetFlagAllEnabled;
        private bool _isOpenCommponent;
        private bool _skipConfirmation;

        public void OnEnable()
        {
            tagChanger = (GameObjectTagChangerVRChatOnly)target;
            // Dictionaryの復元処理
            tagChanger.LoadDictionary();
            if (tagChanger.rootObject == null ) 
            {
                tagChanger.rootObject = tagChanger.gameObject;
            }
        }

        private bool DisplayDialog(string message)
        {
            if (tagChanger.skipConfirmation == true)
            {
                return true;
            }

            bool dialogFlag = EditorUtility.DisplayDialog(nameof(GameObjectTagChangerVRChatOnly), message, "処理開始", "キャンセル");
            return dialogFlag;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            _tagNames = serializedObject.FindProperty("tagNames");
            _searchTagName = serializedObject.FindProperty("searchTagName");

            EditorGUILayout.LabelField("List内のGameObjectのTagを変更するツールです", EditorStyles.boldLabel);

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

            # if UNITY_2019
            #region // Listの並べ替え
            _isOpenTargetList = EditorGUILayout.BeginFoldoutHeaderGroup(_isOpenTargetList, "Listの並べ替え");
            if (_isOpenTargetList == true)
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
            #endif

            #region // 実行用ボタン
            EditorGUILayout.Space(_spaceHeight);
            for (int i = 0; i < tagChanger.tagNames.Length; i++)
            {
                if (GUILayout.Button($"Tagを{tagChanger.tagNames[i]}に設定する"))
                {
                    bool dialogFlag = this.DisplayDialog($"Tagを{tagChanger.tagNames[i]}に設定する");
                    if (dialogFlag == true)
                    {
                        tagChanger.ChangeGameObjectTag(tagChanger.tagNames[i]);
                    }
                }
            }
            #endregion

            #region // 検索に関する項目
            tagChanger.isOpenSearch = EditorGUILayout.Foldout(tagChanger.isOpenSearch, "GameObjectの検索・取得に関する項目");
            if(tagChanger.isOpenSearch == true)
            {
                // 検索用のRootObjectの設定
                tagChanger.rootObject = EditorGUILayout.ObjectField("検索対象のRootObject", tagChanger.rootObject, typeof(GameObject), true) as GameObject;

                #region // Tagで検索
                EditorGUI.indentLevel++;
                tagChanger.isOpenSearchTagName = EditorGUILayout.Foldout(tagChanger.isOpenSearchTagName, "Tagで検索・取得");
                if (tagChanger.isOpenSearchTagName == true)
                {
                    tagChanger.searchTagName = EditorGUILayout.TagField("検索・取得するTagを選択してください", _searchTagName.stringValue);

                    if (GUILayout.Button($"RootObjectからTagが{tagChanger.searchTagName}のものを検索・取得"))
                    {
                        if (tagChanger.rootObject == null) { Debug.LogWarning("RootObjectが設定されていません"); return; }

                        bool dialogFlag = this.DisplayDialog($"RootObjectからTagが{tagChanger.searchTagName}のものを検索・取得");
                        if (dialogFlag == true)
                        {
                            tagChanger.SearchTagObject(tagChanger.searchTagName);
                            EditorUtility.SetDirty(tagChanger);
                        }
                    }
                }
                EditorGUI.indentLevel--;
                #endregion

                #region // 文字列で検索
                EditorGUI.indentLevel++;
                tagChanger.isOpenSearchObjectName = EditorGUILayout.Foldout(tagChanger.isOpenSearchObjectName, "文字列で検索・取得");
                if (tagChanger.isOpenSearchObjectName == true)
                {
                    string _searchName = EditorGUILayout.TextField("検索用文字列", tagChanger.searchName);
                    if (_searchName != tagChanger.searchName) 
                    {
                        tagChanger.searchName = _searchName;
                        EditorUtility.SetDirty(tagChanger);
                    }
                    if (GUILayout.Button($"RootObjectから{tagChanger.searchName}を含むGameObjectを検索・取得"))
                    {
                        if (tagChanger.rootObject == null) { Debug.LogWarning("RootObjectが設定されていません"); return; }
                        bool dialogFlag = this.DisplayDialog($"RootObjectから{tagChanger.searchName}を含むGameObjectを検索・取得");
                        if (dialogFlag == true)
                        {
                            tagChanger.SearchContainsNameObject();
                            EditorUtility.SetDirty(tagChanger);
                        }
                    }
                    if (GUILayout.Button($"RootObjectから{tagChanger.searchName}で始まるGameObjectを検索・取得"))
                    {
                        if (tagChanger.rootObject == null) { Debug.LogWarning("RootObjectが設定されていません"); return; }
                        bool dialogFlag = this.DisplayDialog($"RootObjectから{tagChanger.searchName}で始まるGameObjectを検索・取得");
                        if (dialogFlag == true)
                        {
                            tagChanger.SearchStartWishNameObject();
                            EditorUtility.SetDirty(tagChanger);
                        }
                    }
                    if (GUILayout.Button($"RootObjectから{tagChanger.searchName}で終わるGameObjectを検索・取得"))
                    {
                        if (tagChanger.rootObject == null) { Debug.LogWarning("RootObjectが設定されていません"); return; }
                        bool dialogFlag = this.DisplayDialog($"RootObjectから{tagChanger.searchName}で終わるGameObjectを検索・取得");
                        if (dialogFlag == true)
                        {
                            tagChanger.SearchEndsWithNameObject(); 
                            EditorUtility.SetDirty(tagChanger);
                        }
                    }
                }
                #region // 階層で取得
                tagChanger.isOpenHierarchy = EditorGUILayout.Foldout(tagChanger.isOpenHierarchy, "階層で取得");
                if (tagChanger.isOpenHierarchy == true)
                {
                    EditorGUI.indentLevel++;
                    if (GUILayout.Button("RootObjectの子のGameObjectを取得"))
                    {
                        bool dialogFlag = this.DisplayDialog("RootObjectの子のGameObjectを取得");
                        if (dialogFlag == true)
                        {
                            if (tagChanger.rootObject == null) { Debug.LogWarning("RootObjectが設定されていません"); return; }
                            tagChanger.GetChildObjects();
                            EditorUtility.SetDirty(tagChanger);

                        }
                    }
                    if (GUILayout.Button("RootObject下の全てのGameObjectを取得"))
                    {
                        bool dialogFlag = this.DisplayDialog("RootObject下の全てのGameObjectを取得");
                        if (dialogFlag == true)
                        {
                            if (tagChanger.rootObject == null) { Debug.LogWarning("RootObjectが設定されていません"); return; }
                            tagChanger.GetAllObjects();
                            EditorUtility.SetDirty(tagChanger);
                        }
                    }
                    EditorGUI.indentLevel--;
                }
                
                #endregion
                #region // SkinnedMeshrendereのBonesを取得
                tagChanger.isOpenSkinnedMeshRenderer = EditorGUILayout.Foldout(tagChanger.isOpenSkinnedMeshRenderer, "SkinnedMeshRendererのBonesを取得");
                if (tagChanger.isOpenSkinnedMeshRenderer == true)
                {
                    SkinnedMeshRenderer skinnedMeshRenderer;
                    skinnedMeshRenderer = EditorGUILayout.ObjectField(nameof(SkinnedMeshRenderer), tagChanger.targetSkinnedMeshRenderer, typeof(SkinnedMeshRenderer), true) as SkinnedMeshRenderer;
                    if (skinnedMeshRenderer != tagChanger.targetSkinnedMeshRenderer)
                    {
                        Undo.RecordObject(tagChanger, nameof(GameObjectTagChangerVRChatOnly));
                        tagChanger.targetSkinnedMeshRenderer = skinnedMeshRenderer;
                        EditorUtility.SetDirty(tagChanger);
                    }
                    if (GUILayout.Button("SkinnedMeshRendererのBonesを取得"))
                    {
                        if (tagChanger.targetSkinnedMeshRenderer == null) { Debug.LogWarning("SkinnedMeshRendererが設定されていません"); return; }
                        tagChanger.GetSkinnedMeshRendererBonesObjects();
                        EditorUtility.SetDirty(tagChanger);
                    }
                }

                #endregion
                #region // Commponentで検索
                tagChanger.isOpenCommponent = EditorGUILayout.Foldout(tagChanger.isOpenCommponent, "Commponentで検索・取得");
                if (tagChanger.isOpenCommponent == true)
                {
                    string _searchComponentName = EditorGUILayout.TextField("検索用文字列", tagChanger.searchComponentName);
                    if (_searchComponentName != tagChanger.searchComponentName)
                    {
                        Undo.RecordObject(tagChanger, nameof(GameObjectTagChangerVRChatOnly));
                        tagChanger.searchComponentName = _searchComponentName;
                        EditorUtility.SetDirty(tagChanger); 
                    }
                    if (GUILayout.Button($"RootObjectから{tagChanger.searchComponentName}を含むGameObjectを検索・取得"))
                    {
                        if (tagChanger.rootObject == null) { Debug.LogWarning("RootObjectが設定されていません"); return; }
                        bool dialogFlag = this.DisplayDialog($"RootObjectから{tagChanger.searchComponentName}を含むGameObjectを検索・取得する");
                        if (dialogFlag == true)
                        {
                            Undo.RecordObject(this, nameof(GameObjectTagChangerVRChatOnly));
                            tagChanger.SearchComponentName(nameof(VRCPhysBone));
                            tagChanger.ResetDictionaryFlags();
                            Debug.Log($"RootObjectから{nameof(VRCPhysBone)}を含むGameObjectを取得しました");
                            EditorUtility.SetDirty(tagChanger);
                        }
                    }
                    if (GUILayout.Button($"RootObjectから{nameof(VRCPhysBone)}を含むGameObjectを検索・取得"))
                    {
                        if (tagChanger.rootObject == null) { Debug.LogWarning("RootObjectが設定されていません"); return; }
                        bool dialogFlag = this.DisplayDialog($"RootObjectから{nameof(VRCPhysBone)}を含むGameObjectを検索・取得する");
                        if (dialogFlag == true)
                        {
                            Undo.RecordObject(this, nameof(GameObjectTagChangerVRChatOnly));
                            tagChanger.SearchComponentName(nameof(VRCPhysBone));
                            tagChanger.ResetDictionaryFlags();
                            Debug.Log($"RootObjectから{nameof(VRCPhysBone)}を含むGameObjectを取得しました");
                            EditorUtility.SetDirty(tagChanger);                            
                        }
                    }

                    if (GUILayout.Button($"RootObjectから{nameof(VRCPhysBoneCollider)}を含むGameObjectを検索・取得"))
                    {
                        if (tagChanger.rootObject == null) { Debug.LogWarning("RootObjectが設定されていません"); return; }
                        bool dialogFlag = this.DisplayDialog($"RootObjectから{nameof(VRCPhysBoneCollider)}を含むGameObjectを検索・取得する");
                        if (dialogFlag == true)
                        {
                            Undo.RecordObject(this, nameof(GameObjectTagChangerVRChatOnly));
                            tagChanger.SearchComponentName(nameof(VRCPhysBoneCollider));
                            Debug.Log($"RootObjectから{nameof(VRCPhysBone)}を含むGameObjectを取得しました");
                            EditorUtility.SetDirty(tagChanger);
                        }
                    }

                    if (GUILayout.Button($"RootObjectから{nameof(ParticleSystem)}を含むGameObjectを検索・取得"))
                    {
                        if (tagChanger.rootObject == null) { Debug.LogWarning("RootObjectが設定されていません"); return; }
                        bool dialogFlag = this.DisplayDialog($"RootObjectから{nameof(ParticleSystem)}を含むGameObjectを検索・取得する");
                        if (dialogFlag == true)
                        {
                            Undo.RecordObject(this, nameof(GameObjectTagChangerVRChatOnly));
                            tagChanger.SearchComponentName(nameof(ParticleSystem));
                            Debug.Log($"RootObjectから{nameof(ParticleSystem)}を含むGameObjectを取得しました");
                            EditorUtility.SetDirty(tagChanger);
                        }
                        var testtt = tagChanger.GetComponentsInChildren<ParticleSystem>(true);
                        for (int i = 0; i < testtt.Length; i++)
                        {
                            Debug.Log(testtt[i].name);
                        }
                    }
                }
                #endregion
                EditorGUI.indentLevel--;
                #endregion

            }
            #endregion

            #region // 対象・対象外設定
            tagChanger.isOpenEnabled = EditorGUILayout.Foldout(tagChanger.isOpenEnabled, "Tagの変更処理の対象・対象外設定");
            if (tagChanger.isOpenEnabled == true)
            {
                if (tagChanger.targetObjects.Length == 0)
                {
                    EditorGUILayout.HelpBox("対象となるGameObjectが設定されていません", MessageType.Warning);
                }

                for (int i = 0; i < tagChanger.targetObjects.Length; i++)
                {
                    GameObject keyObject;
                    keyObject = tagChanger.targetObjects[i];

                    if (i == 0)
                    {
                        // 一括変更処理
                        _targetFlagAllEnabled = EditorGUILayout.Toggle(tagChanger.targetFlagAllEnabled, GUILayout.Width(_toggleWidh));
                        if (_targetFlagAllEnabled != tagChanger.targetFlagAllEnabled)
                        {
                            Undo.RecordObject(tagChanger, nameof(GameObjectTagChangerVRChatOnly));
                            tagChanger.targetFlagAllEnabled = _targetFlagAllEnabled;
                            GameObject[] keyObjects = tagChanger.targetFlagDict.Keys.ToArray();
                            for (int ki = 0; ki < keyObjects.Length; ki++)
                            {
                                tagChanger.targetFlagDict[keyObjects[ki]] = tagChanger.targetFlagAllEnabled;
                            }
                            tagChanger.SaveDictionary();
                            EditorUtility.SetDirty(tagChanger);
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
                        if (tmpFlag != tagChanger.targetFlagDict[keyObject])
                        {
                            Undo.RecordObject(tagChanger, nameof(GameObjectTagChangerVRChatOnly));
                            tagChanger.targetFlagDict[keyObject] = tmpFlag;

                            tagChanger.SaveDictionary();
                            EditorUtility.SetDirty(tagChanger);
                        }
                        EditorGUILayout.ObjectField(keyObject, typeof(GameObject), true);
                    }
                }
            }
            #endregion

            #region // その他
            tagChanger.isOpenOther = EditorGUILayout.Foldout(tagChanger.isOpenOther, "その他");
            if (tagChanger.isOpenOther == true)
            {     
                _skipConfirmation = EditorGUILayout.ToggleLeft("確認用ダイアログの表示を省略する", tagChanger.skipConfirmation);
                if (_skipConfirmation != tagChanger.skipConfirmation)
                {
                    Undo.RecordObject(tagChanger, nameof(GameObjectTagChangerVRChatOnly));
                    tagChanger.skipConfirmation = _skipConfirmation;
                    EditorUtility.SetDirty(tagChanger);
                }

                if (tagChanger.targetObjects.Length == 0)
                {
                    EditorGUILayout.HelpBox("対象となるGameObjectが設定されていません", MessageType.Warning);
                }
                else
                {
                    if (GUILayout.Button("List内のGameObjectを選択状態にする")) 
                    {
                        Selection.objects = tagChanger.targetObjects.ToArray();
                        Debug.Log("List内のGameObjectを選択状態にしました");
                    }

                    if (GUILayout.Button("処理対象のGameObjectをActiveにする"))
                    {
                        bool dialogFlag = this.DisplayDialog("処理対象のGameObjectをActiveにする");
                        if (dialogFlag == true)
                        {
                            Undo.RecordObjects(tagChanger.targetObjects, nameof(GameObjectTagChangerVRChatOnly));
                            foreach (GameObject targetObject in tagChanger.targetObjects)
                            {
                                targetObject.SetActive(true);
                            }
                        }
                        Debug.Log("処理対象のGameObjectをActiveにしました");
                    }
                    if (GUILayout.Button("処理対象のGameObjectを非Activeにする"))
                    {
                        bool dialogFlag = this.DisplayDialog("処理対象のGameObjectを非Activeにする");
                        if (dialogFlag == true)
                        {
                            Undo.RecordObjects(tagChanger.targetObjects, nameof(GameObjectTagChangerVRChatOnly));
                            foreach (GameObject targetObject in tagChanger.targetObjects)
                            {
                                targetObject.SetActive(false);
                            }
                        }
                        Debug.Log("処理対象のGameObjectを非Activeにしました");
                    }
                }
            }
            #endregion
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif