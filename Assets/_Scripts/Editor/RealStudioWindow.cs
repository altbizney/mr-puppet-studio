using UnityEditor;
using UnityEngine;

namespace Thinko
{
    public class RealStudioWindow : EditorWindow
    {
        public static bool ShowHelp = false;
        
        private static GUIStyle _headerStyle;

        private SerializedObject _realPuppetSerializedObject;
        private SerializedProperty _shoulderJointProperty;
        private SerializedProperty _shoulderOffsetProperty;
        private SerializedProperty _elbowJointProperty;
        private SerializedProperty _elbowOffsetProperty;
        private SerializedProperty _wristJointProperty;
        private SerializedProperty _wristOffsetProperty;
        
        [MenuItem("Thinko/Real Studio")]
        public static void ShowWindow()
        {
            GetWindow<RealStudioWindow>(false, "Real Studio").Show();
        }

        private void OnGUI()
        {
            CreateStyles();
            RealPuppetDataProvider dataProvider = null;
            var puppet = FindObjectOfType<RealPuppet>();
            
            // Header
            EditorGUILayout.LabelField("Real Studio", _headerStyle);
            
            // Help button
            var defColor = GUI.color;
            GUI.color = ShowHelp ? Color.green : defColor;
            var rect = GUILayoutUtility.GetLastRect();
            if (GUI.Button(new Rect(rect.width - 30, rect.y, 30, 30), "?"))
            {
                ShowHelp = !ShowHelp;
                var editors = Resources.FindObjectsOfTypeAll<EditorWindow>();
                foreach (var editor in editors)
                    editor.Repaint();
            }
            GUI.color = defColor;
            
            // Serialized properties
            if (puppet != null)
            {
                _realPuppetSerializedObject = new SerializedObject(puppet);
                _shoulderJointProperty = _realPuppetSerializedObject.FindProperty("ShoulderJoint");
                _shoulderOffsetProperty = _realPuppetSerializedObject.FindProperty("ShoulderOffset");
                _elbowJointProperty = _realPuppetSerializedObject.FindProperty("ElbowJoint");
                _elbowOffsetProperty = _realPuppetSerializedObject.FindProperty("ElbowOffset");
                _wristJointProperty = _realPuppetSerializedObject.FindProperty("WristJoint");
                _wristOffsetProperty = _realPuppetSerializedObject.FindProperty("WristOffset");
            }
            
            // RealPuppet
            if (puppet == null)
            {
                // Drop Area
                if(ShowHelp)
                    EditorGUILayout.HelpBox("Step 1: Drag your model or prefab to the area below.", MessageType.Info);
                
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
            
            if(dataProvider == null && ShowHelp)
                EditorGUILayout.HelpBox("Step 2: Create the websocket data stream reader.", MessageType.Info);
            
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
            
            if(realBody == null && ShowHelp)
                EditorGUILayout.HelpBox("Step 3: Create the RealBody component which handles the conversion of the websocket data into an usable format and applies it to a simulated arm.", MessageType.Info);
            
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
                GUILayout.BeginHorizontal();
                    GUILayout.BeginVertical("GroupBox");
                        
                        if(ShowHelp)
                            EditorGUILayout.HelpBox("During Play mode, click the buttons below to record the default poses.", MessageType.Info);
                        
                        GUI.enabled = Application.isPlaying;
                        
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
        
        private void RealPuppetGUI()
        {
            GUILayout.Space(10);
            
            var realPuppet = FindObjectOfType<RealPuppet>();
            
            if(realPuppet != null && ShowHelp)
                EditorGUILayout.HelpBox("The RealPuppet component handles the look and behaviour of the puppet 3D model and grabs its movement information from the RealBody component.", MessageType.Info);
            
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
                // Joints
                GUILayout.Label("JOINTS", EditorStyles.boldLabel);
                
                if(ShowHelp)
                    EditorGUILayout.HelpBox("Assign the puppet bones that you want to attach to the corresponding joint.", MessageType.Info);
                
                RealPuppetEditor.JointGUI(_realPuppetSerializedObject, _shoulderJointProperty, _shoulderOffsetProperty);
                RealPuppetEditor.JointGUI(_realPuppetSerializedObject, _elbowJointProperty, _elbowOffsetProperty);
                RealPuppetEditor.JointGUI(_realPuppetSerializedObject, _wristJointProperty, _wristOffsetProperty);
                
                // Jaw
                GUILayout.Space(10);
                GUILayout.Label("JAW", EditorStyles.boldLabel);
                
                if(ShowHelp)
                    EditorGUILayout.HelpBox("Toggle the jaw animation. You need to edit the RealPuppet component directly to configure it.", MessageType.Info);
                
                realPuppet.AnimateJaw = EditorGUILayout.Toggle("Animate Jaw", realPuppet.AnimateJaw);
                
                // Blink
                GUILayout.Space(10);
                GUILayout.Label("BLINK", EditorStyles.boldLabel);
                
                if(ShowHelp)
                    EditorGUILayout.HelpBox("Add or remove one of the various blinking methods.", MessageType.Info);
                
                if (realPuppet.GetComponentInChildren<Blink>() == null)
                {
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("Blink w/ Driven Keys"))
                    {
                        realPuppet.gameObject.AddComponent<BlinkDrivenKeys>();
                    }
                    if (GUILayout.Button("Blink w/ Animator"))
                    {
                        realPuppet.gameObject.AddComponent<BlinkAnimator>();
                    }
                    if (GUILayout.Button("Blink w/ Blendshape"))
                    {
                        realPuppet.gameObject.AddComponent<BlinkBlendshape>();
                    }
                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    if (GUILayout.Button("Remove Blink"))
                    {
                        var blink = realPuppet.gameObject.GetComponentInChildren<Blink>();
                        Animation anim = null;
                        DrivenKeys drivenKeys = null;
                        if (blink != null)
                        {
                            anim = blink.GetComponent<Animation>();
                            drivenKeys = blink.GetComponent<DrivenKeys>();
                        }
                        DestroyImmediate(blink);
                        if(anim != null) DestroyImmediate(anim);
                        if(drivenKeys != null) DestroyImmediate(drivenKeys);
                    }
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