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

        private void Update()
        {
            if (EditorApplication.isPlaying)
                Repaint();
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
            
            GUILayout.BeginVertical("HelpBox");
 
            GUILayout.BeginHorizontal();
            GUILayout.Label ("DATA PROVIDER", EditorStyles.boldLabel, GUILayout.ExpandWidth(false));
            if (dataProvider != null)
            {
                if (GUILayout.Button(">", GUILayout.Width(30)))
                {
                    Selection.activeGameObject = dataProvider.gameObject;
                }
            }
            else
            {
                if (GUILayout.Button("Create"))
                {
                    var dataProviderGO = new GameObject("WebsocketDataStream");
                    dataProvider = dataProviderGO.AddComponent<WebsocketDataStream>();
                }
            }
            GUILayout.EndHorizontal();

            if (dataProvider != null)
            {
                GUILayout.BeginHorizontal();
                GUILayout.BeginVertical("GroupBox");

                dataProvider.WebsocketUri = EditorGUILayout.TextField("Websocket Uri", dataProvider.WebsocketUri);
                dataProvider.OutputData = EditorGUILayout.Toggle("Output Data", dataProvider.OutputData);
 
                GUILayout.EndVertical ();
                GUILayout.EndHorizontal();
            }
 
            GUILayout.EndVertical ();

            return dataProvider;
        }
        
        private static RealBody RealBodyGUI(RealPuppetDataProvider dataProvider)
        {
            GUILayout.Space(10);
            
            var realBody = FindObjectOfType<RealBody>(); 
            
            GUILayout.BeginVertical("HelpBox");
            GUILayout.BeginHorizontal();
            GUILayout.Label ("REAL BODY", EditorStyles.boldLabel, GUILayout.ExpandWidth(false));
            if (realBody != null)
            {
                if (GUILayout.Button(">", GUILayout.Width(30)))
                {
                    Selection.activeGameObject = realBody.gameObject;
                }
            }
            else
            {
                if (GUILayout.Button("Create"))
                {
                    var realBodyGO = new GameObject("RealBody");
                    realBody = realBodyGO.AddComponent<RealBody>();
                    realBody.DataProvider = dataProvider;
                }
            }
            GUILayout.EndHorizontal();
 
            if (realBody != null)
            {
                GUI.enabled = Application.isPlaying;
                GUILayout.BeginHorizontal();
                    GUILayout.BeginVertical("GroupBox");
                        if (GUILayout.Button("Grab TPose"))
                        {
                            realBody.GrabTPose();
                        }
                        GUILayout.Space(10);
                        GUILayout.BeginHorizontal();
                            if (GUILayout.Button("Grab Jaw Closed"))
                            {
                                realBody.GrabJawClosed();
                            }
                            if (GUILayout.Button("Grab Jaw Opened"))
                            {
                                realBody.GrabJawOpened();
                            }
                        GUILayout.EndHorizontal();
                         
                        // Jaw "progress bar"
                        EditorGUI.ProgressBar( 
                            EditorGUILayout.GetControlRect( false, 20 ), 
                            Mathf.InverseLerp(realBody.JawClosed, realBody.JawOpened, realBody.DataProvider.Jaw),
                            $"{realBody.DataProvider.Jaw}" );
                        
                    GUILayout.EndVertical ();
                GUILayout.EndHorizontal();
                GUI.enabled = true;
            }
 
            GUILayout.EndVertical ();
            
            return realBody;
        }
        
        private static void RealPuppetGUI()
        {
            GUILayout.Space(10);
            
            var realPuppet = FindObjectOfType<RealPuppet>();
            
            GUILayout.BeginVertical("HelpBox");
            GUILayout.BeginHorizontal();
            GUILayout.Label ("REAL PUPPET", EditorStyles.boldLabel, GUILayout.ExpandWidth(false));
            if (realPuppet != null)
            {
                if (GUILayout.Button(">", GUILayout.Width(30)))
                {
                    Selection.activeGameObject = realPuppet.gameObject;
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical("GroupBox");
            
            if (realPuppet != null)
            {
                realPuppet.ShoulderJoint = EditorGUILayout.ObjectField("Attach to Shoulder", realPuppet.ShoulderJoint, typeof(Transform), true) as Transform;
                if (realPuppet.ShoulderJoint != null)
                {
                    EditorGUI.indentLevel++;
                    realPuppet.ShoulderOffset = EditorGUILayout.Vector3Field("Offset", realPuppet.ShoulderOffset);
                    EditorGUI.indentLevel--;
                }
                
                realPuppet.ElbowJoint = EditorGUILayout.ObjectField("Attach to Elbow", realPuppet.ElbowJoint, typeof(Transform), true) as Transform;
                if (realPuppet.ElbowJoint != null)
                {
                    EditorGUI.indentLevel++;
                    realPuppet.ElbowOffset = EditorGUILayout.Vector3Field("Offset", realPuppet.ElbowOffset);
                    EditorGUI.indentLevel--;
                }
                
                realPuppet.WristJoint = EditorGUILayout.ObjectField("Attach to Wrist", realPuppet.WristJoint, typeof(Transform), true) as Transform;
                if (realPuppet.WristJoint != null)
                {
                    EditorGUI.indentLevel++;
                    realPuppet.WristOffset = EditorGUILayout.Vector3Field("Offset", realPuppet.WristOffset);
                    EditorGUI.indentLevel--;
                }
            }
            
            if(GUI.changed)
                realPuppet.OnValidate();
 
            GUILayout.EndVertical ();
            GUILayout.EndHorizontal();
            GUILayout.EndVertical ();
        }
    }
}