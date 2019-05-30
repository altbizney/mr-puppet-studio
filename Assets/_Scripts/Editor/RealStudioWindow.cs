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
            GetWindow<RealStudioWindow>(false, "Real Studio").Show();
        }

        private void OnGUI()
        {
            CreateStyles();

            // Header
            EditorGUILayout.LabelField("Real Studio", _headerStyle);

            var puppet = FindObjectOfType<RealPuppet>();

            if (puppet == null)
            {
                // Drop Area
                var go = PuppetModelDropAreaGUI();
                if (go == null) return;
                if (go.GetComponentInChildren<RealPuppet>() == null)
                    go.AddComponent<RealPuppet>();
            }
            else
            {
                // Data Provider
                DataProviderGUI();
            }
            
            
            // Repaint
            if (GUI.changed) Repaint();
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

        private static GameObject PuppetModelDropAreaGUI()
        {
            var evt = Event.current;
            var dropArea = GUILayoutUtility.GetRect(0.0f, 50.0f, GUILayout.ExpandWidth(true));
            var style = new GUIStyle("box");
            if (EditorGUIUtility.isProSkin)
                style.normal.textColor = Color.white;
            GUI.Box(dropArea, "\nDROP PUPPET MODEL HERE", style);

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
                                var gameObject = Instantiate(go);
                                gameObject.name = go.name;
                                gameObject.transform.localScale = Vector3.one;
                                
                                // Some models might import cameras, so we just make sure to get rid of them
                                foreach (var cam in gameObject.GetComponentsInChildren<Camera>())
                                    DestroyImmediate(cam.gameObject);
                                
                                return gameObject;
                            }
                        }
                    }
                    break;
            }

            return null;
        }

        private static void DataProviderGUI()
        {
            GUILayout.Label ("Data Provider", EditorStyles.boldLabel);

            var dataProvider = FindObjectOfType<WebsocketDataStream>(); 
            if (dataProvider == null)
            {
                if (GUILayout.Button("Create Data Provider"))
                {
                    var dataProviderGO = new GameObject("WebsocketDataStream");
                    dataProviderGO.AddComponent<WebsocketDataStream>();
                }
            }
            else
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Select DataProvider", GUILayout.Width(120)))
                {
                    Selection.activeGameObject = dataProvider.gameObject;
                }

                dataProvider.WebsocketUri = EditorGUILayout.TextField("Websocket Uri", dataProvider.WebsocketUri, GUILayout.ExpandWidth(false));
                dataProvider.OutputData = EditorGUILayout.Toggle("Output Data", dataProvider.OutputData, GUILayout.ExpandWidth(false));
                GUILayout.EndHorizontal();
            }
        }
    }
}