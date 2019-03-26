using UnityEditor;
using UnityEngine;

namespace Thinko
{
    [CustomEditor(typeof(RealPuppet))]
    public class RealPuppetEditor : Editor
    {
        private RealPuppet _realPuppet;

        private Transform[] _childNodes;

        private void OnEnable()
        {
            _realPuppet = target as RealPuppet; 
        }

        private void OnSceneGUI()
        {
            if (!_realPuppet.enabled)
                return;


            // Draw child and root nodes
            if (_realPuppet.RootNode)
            {
                if(_childNodes == null)
                    _childNodes = _realPuppet.RootNode.GetComponentsInChildren<Transform>();
                
                var handleSize = HandleUtility.GetHandleSize(Vector3.zero) * .1f;
                foreach (var child in _childNodes)
                {
                    Handles.color = child == _realPuppet.RootNode ? Color.magenta : Color.yellow;
                    Handles.CircleHandleCap(0, child.position, Quaternion.identity, handleSize, EventType.Repaint);
                }
            }

            // Draw main nodes
            if (_realPuppet.HeadNode)
                DrawMainNode(_realPuppet.HeadNode, "Head", Color.yellow);

            if (_realPuppet.ButtNode)
                DrawMainNode(_realPuppet.ButtNode, "Butt", Color.yellow);

            if (_realPuppet.JawNode)
                DrawMainNode(_realPuppet.JawNode, "Jaw", Color.yellow);
        }

        private void DrawMainNode(Transform node, string nodeName, Color color)
        {
            var position = node.position;

            // Label
            GUI.contentColor = color;
            Handles.Label(position, nodeName, new GUIStyle()
            {
                fontSize = 20,
                alignment = TextAnchor.MiddleLeft,
                contentOffset = new Vector2(15, -10),
                normal = new GUIStyleState()
                {
                    textColor = Color.yellow
                }
            });

            // Move handle
            EditorGUI.BeginChangeCheck();
            var newPos = Handles.FreeMoveHandle(
                position,
                Quaternion.identity,
                HandleUtility.GetHandleSize(Vector3.zero) * .1f,
                Vector3.zero,
                Handles.SphereHandleCap);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(node, "Move");
                node.position = newPos;
            }
        }
    }
}