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

            RealPuppetDataProvider dataProvider = null;
            var puppet = FindObjectOfType<RealPuppet>();
            if (puppet == null)
            {
                // Drop Area
                var go = PuppetModelDropAreaGUI();
                if (go == null) return;
                if (go.GetComponentInChildren<RealPuppet>() == null)
                    Undo.AddComponent<RealPuppet>(go);
            }
            else
            {
                // Data Provider
                dataProvider = DataProviderGUI();
            }
            
            // RealBody
            if(dataProvider == null) return;
            var realBody = RealBodyGUI(dataProvider);
            
            // RealPuppet
            if(realBody == null) return;
            RealPuppetGUI();
            
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
                            
                            if (PrefabUtility.GetPrefabInstanceStatus(go) == PrefabInstanceStatus.Connected)
                            {
                                // Already on stage
                                return go;
                            }

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
                    break;
            }

            return null;
        }

        private static RealPuppetDataProvider DataProviderGUI()
        {
            var dataProvider = FindObjectOfType<WebsocketDataStream>(); 

            GUILayout.BeginHorizontal();
            GUILayout.Label ("Data Provider", EditorStyles.boldLabel, GUILayout.ExpandWidth(false));
            if (dataProvider != null)
            {
                if (GUILayout.Button(">", GUILayout.Width(30)))
                {
                    Selection.activeGameObject = dataProvider.gameObject;
                }
            }
            GUILayout.EndHorizontal();

            if (dataProvider == null)
            {
                if (GUILayout.Button("Create Data Provider"))
                {
                    var dataProviderGO = new GameObject("WebsocketDataStream");
                    dataProvider = dataProviderGO.AddComponent<WebsocketDataStream>();
                }
            }

            return dataProvider;
        }
        
        private static RealBody RealBodyGUI(RealPuppetDataProvider dataProvider)
        {
            GUILayout.Space(10);
            
            var realBody = FindObjectOfType<RealBody>(); 
            
            GUILayout.BeginHorizontal();
            GUILayout.Label ("Real Body", EditorStyles.boldLabel, GUILayout.ExpandWidth(false));
            if (realBody != null)
            {
                if (GUILayout.Button(">", GUILayout.Width(30)))
                {
                    Selection.activeGameObject = realBody.gameObject;
                }
            }
            GUILayout.EndHorizontal();

            if (realBody == null)
            {
                if (GUILayout.Button("Create RealBody"))
                {
                    var realBodyGO = new GameObject("RealBody");
                    realBody = realBodyGO.AddComponent<RealBody>();
                    realBody.DataProvider = dataProvider;
                }
            }

            return realBody;
        }
        
        private static void RealPuppetGUI()
        {
            GUILayout.Space(10);
            
            var realPuppet = FindObjectOfType<RealPuppet>(); 
            
            GUILayout.BeginHorizontal();
            GUILayout.Label ("Real Puppet", EditorStyles.boldLabel, GUILayout.ExpandWidth(false));
            if (realPuppet != null)
            {
                if (GUILayout.Button(">", GUILayout.Width(30)))
                {
                    Selection.activeGameObject = realPuppet.gameObject;
                }
            }
            GUILayout.EndHorizontal();
        }
    }
}