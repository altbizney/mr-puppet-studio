using UnityEditor;
using UnityEngine;

namespace Thinko
{
    public class RealStudioWindow : EditorWindow
    {
        private static GUIStyle _headerStyle;

        [MenuItem("Thinko/Real Studio")]
        public static void ShowWindow()
        {
            GetWindow(typeof(RealStudioWindow), false, "Real Studio");
        }

        private void OnGUI()
        {
            CreateStyles();

            // Header
            EditorGUILayout.LabelField("Real Studio", _headerStyle);

            // Drop Area
            var go = DropAreaGUI();

            // Add the RealPuppet editor
            if (go == null) return;
            if (go.GetComponentInChildren<RealPuppet>() == null)
                go.AddComponent<RealPuppet>();
        }

        private void CreateStyles()
        {
            if (_headerStyle == null)
            {
                _headerStyle = new GUIStyle(EditorStyles.helpBox)
                {
                    fontSize = 22,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.UpperCenter,
                    normal = new GUIStyleState()
                    {
                        textColor = Color.yellow
                    }
                };
            }
        }

        private static GameObject DropAreaGUI()
        {
            var evt = Event.current;
            var dropArea = GUILayoutUtility.GetRect(0.0f, 50.0f, GUILayout.ExpandWidth(true));
            var style = new GUIStyle("box");
            if (EditorGUIUtility.isProSkin)
                style.normal.textColor = Color.white;
            GUI.Box(dropArea, "\nDROP FBX HERE", style);

            switch (evt.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (!dropArea.Contains(evt.mousePosition))
                        return null;

                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (evt.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();

                        foreach (var draggedObject in DragAndDrop.objectReferences)
                        {
                            // Is it a GameObject?
                            if (!(draggedObject is GameObject go)) continue;
                            
                            // Does the GameObject have a SkinnedMeshRenderer?
                            if(!go.GetComponentInChildren<SkinnedMeshRenderer>()) continue;
                                
                            // If the GameObject a model?
                            if (PrefabUtility.GetPrefabAssetType(go) != PrefabAssetType.Model) continue;
                            
                            if (PrefabUtility.GetPrefabInstanceStatus(go) == PrefabInstanceStatus.Connected)
                            {
                                // Already on stage
                                return go;
                            }
                            else
                            {
                                // On project, instantiate
                                var prefab = PrefabUtility.InstantiatePrefab(go);
                                prefab.name = go.name;
                                var gameObject = prefab as GameObject;
                                gameObject.transform.localScale = Vector3.one;
                                return gameObject;
                            }
                        }
                    }
                    break;
            }

            return null;
        }
    }
}