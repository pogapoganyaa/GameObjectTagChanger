#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace PogapogaEditor.Component
{
    [CustomEditor(typeof(GameObjectTagChanger))]
    public class GameObjectTagChangerEditor : Editor
    {
        ReorderableList _targetObjects;
        SerializedProperty _tag;
        bool _isOpen = false;
        float _spaceHeight = 10;

        public override void OnInspectorGUI()
        {
            GameObjectTagChanger changer = (GameObjectTagChanger)target;

            serializedObject.Update();

            #region // Tagの表示
            _tag = serializedObject.FindProperty("tagName");
            EditorGUILayout.LabelField("List内のGameObjectのTagを変更します");
            changer.tagName = EditorGUILayout.TagField("設定するTagを選択してください", _tag.stringValue);
            #endregion

            #region // Listの表示
            SerializedProperty targetProperty = serializedObject.FindProperty(nameof(changer.targetObjects));
            EditorGUILayout.PropertyField(targetProperty);
            #endregion

            #region // Listの並べ替え
            _isOpen = EditorGUILayout.BeginFoldoutHeaderGroup(_isOpen, "Listの並べ替え");
            if (_isOpen)
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

            serializedObject.ApplyModifiedProperties();

            #region // 実行用ボタン
            EditorGUILayout.Space(_spaceHeight);
            if (GUILayout.Button($"Tagを{changer.tagName}に設定します"))
            {
                if (changer.targetObjects.Count == 0) { Debug.LogWarning("GameObjectが設定されていません"); return; }
                bool dialogFlag = EditorUtility.DisplayDialog("タグの変更", $"Tagを{changer.tagName}に設定します", "処理開始", "キャンセル");
                if (dialogFlag == true)
                {
                    foreach (GameObject targetObject in changer.targetObjects) { if(targetObject != null) { Undo.RecordObject(targetObject, "Tagの変更"); } }
                    changer.ChangeGameObjectTag();
                }
            }
            EditorGUILayout.Space(_spaceHeight);
            if (GUILayout.Button("List内のGameObjectを選択状態にします")) { Selection.objects = changer.targetObjects.ToArray(); }
            EditorGUILayout.Space(_spaceHeight);
            #endregion
        }
    }
}
#endif