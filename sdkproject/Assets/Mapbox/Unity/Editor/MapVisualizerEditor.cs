namespace Mapbox.Editor
{
	using UnityEngine;
	using UnityEditor;
	using Mapbox.Unity.MeshGeneration.Factories;
	using Mapbox.Editor.NodeEditor;
	using Mapbox.Unity.Map;
	using System;
	using System.Collections.Generic;

	[CustomEditor(typeof(MapVisualizer))]
	public class MapVisualizerEditor : UnityEditor.Editor
	{
		private MonoScript script;
		private List<Editor> _editors;

		private void OnEnable()
		{
			script = MonoScript.FromScriptableObject((MapVisualizer)target);

			_editors = new List<Editor>();
			var facs = serializedObject.FindProperty("Factories");
			for (int i = 0; i < facs.arraySize; i++)
			{
				var fac = new SerializedObject(facs.GetArrayElementAtIndex(i).objectReferenceValue);
				var ed = CreateEditor(fac.targetObject);
				if (ed != null)
					_editors.Add(ed);
			}
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			GUI.enabled = false;
			script = EditorGUILayout.ObjectField("Script", script, typeof(MonoScript), false) as MonoScript;
			GUI.enabled = true;

			//var texture = serializedObject.FindProperty("_loadingTexture");
			//EditorGUILayout.ObjectField(texture, typeof(Texture2D));

			//EditorGUILayout.Space();
			//EditorGUILayout.LabelField("Factories");
			var facs = serializedObject.FindProperty("Factories");
			for (int i = 0; i < facs.arraySize; i++)
			{
				EditorGUILayout.Space();
				EditorGUILayout.Space();
				EditorGUILayout.Space();
				EditorGUILayout.Space();
				var ind = i;
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.BeginVertical();
				GUILayout.Space(5);
				facs.GetArrayElementAtIndex(ind).objectReferenceValue = EditorGUILayout.ObjectField(facs.GetArrayElementAtIndex(i).objectReferenceValue, typeof(AbstractTileFactory), false) as ScriptableObject;

				
				EditorGUILayout.EndVertical();
				if (GUILayout.Button(NodeBasedEditor.magnifierTexture, (GUIStyle)"minibuttonleft", GUILayout.Width(30)))
				{
					ScriptableCreatorWindow.Open(typeof(AbstractTileFactory), facs, ind);
				}
				if (GUILayout.Button(new GUIContent("-"), (GUIStyle)"minibuttonright", GUILayout.Width(30), GUILayout.Height(22)))
				{
					facs.DeleteArrayElementAtIndex(ind);
				}
				EditorGUILayout.EndHorizontal();

				var ele = facs.GetArrayElementAtIndex(i);
				if (ele != null & ele.objectReferenceValue != null)
				{
					var fac = new SerializedObject(facs.GetArrayElementAtIndex(i).objectReferenceValue);
					var test = DrawHeader(fac.targetObject.GetType().Name, facs.GetArrayElementAtIndex(i), fac.FindProperty("Active"), null);
					if (test)
					{
						EditorGUI.indentLevel++;
						EditorGUILayout.Space();
						_editors[i].OnInspectorGUI();
						EditorGUILayout.Space();
						EditorGUILayout.Space();
						EditorGUI.indentLevel--;
					}
				}
			}

			EditorGUILayout.Space();
			EditorGUILayout.Space();
			EditorGUILayout.Space();
			EditorGUILayout.Space();
			EditorGUILayout.Space();
			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button(new GUIContent("Add New Empty Factory Slot"), (GUIStyle)"minibuttonleft"))
			{
				facs.arraySize++;
				facs.GetArrayElementAtIndex(facs.arraySize - 1).objectReferenceValue = null;
			}
			if (GUILayout.Button(new GUIContent("Find Factory Asset"), (GUIStyle)"minibuttonright"))
			{
				ScriptableCreatorWindow.Open(typeof(AbstractTileFactory), facs);
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.Space();
			EditorGUILayout.Space();

			//for (int i = 0; i < facs.arraySize; i++)
			//{
			//	var fac = new SerializedObject(facs.GetArrayElementAtIndex(i).objectReferenceValue);
			//	var test = DrawHeader(fac.targetObject.GetType().Name, facs.GetArrayElementAtIndex(i), fac.FindProperty("Active"), null);
			//	if (test)
			//	{
			//		//EditorGUI.indentLevel++;
			//		EditorGUILayout.Space();
			//		_editors[i].OnInspectorGUI();
			//		EditorGUILayout.Space();
			//		EditorGUILayout.Space();
			//		//EditorGUI.indentLevel--;
			//	}
			//}

			serializedObject.ApplyModifiedProperties();
		}

		public static bool DrawHeader(string title, SerializedProperty group, SerializedProperty activeField, Action removeAction)
		{
			var backgroundRect = GUILayoutUtility.GetRect(1f, 17f);

			var labelRect = backgroundRect;
			labelRect.xMin += 16f;
			labelRect.xMax -= 20f;

			var toggleRect = backgroundRect;
			toggleRect.y += 2f;
			toggleRect.width = 13f;
			toggleRect.height = 13f;

			var menuIcon = EditorGUIUtility.isProSkin
				? Styling.paneOptionsIconDark
				: Styling.paneOptionsIconLight;

			var menuRect = new Rect(labelRect.xMax + 4f, labelRect.y + 4f, menuIcon.width, menuIcon.height);

			// Background rect should be full-width
			backgroundRect.xMin = 0f;
			backgroundRect.width += 4f;

			// Background
			float backgroundTint = EditorGUIUtility.isProSkin ? 0.1f : 1f;
			EditorGUI.DrawRect(backgroundRect, new Color(backgroundTint, backgroundTint, backgroundTint, 0.2f));

			// Title
			using (new EditorGUI.DisabledScope(!activeField.boolValue))
				EditorGUI.LabelField(labelRect, new GUIContent(title), EditorStyles.boldLabel);

			// Active checkbox
			activeField.serializedObject.Update();
			activeField.boolValue = GUI.Toggle(toggleRect, activeField.boolValue, GUIContent.none, Styling.smallTickbox);
			activeField.serializedObject.ApplyModifiedProperties();

			// Dropdown menu icon
			GUI.DrawTexture(menuRect, menuIcon);

			// Handle events
			var e = Event.current;

			if (e.type == EventType.MouseDown)
			{
				if (menuRect.Contains(e.mousePosition))
				{
					//ShowHeaderContextMenu(new Vector2(menuRect.x, menuRect.yMax), target, resetAction, removeAction);
					e.Use();
				}
				else if (labelRect.Contains(e.mousePosition))
				{
					if (e.button == 0)
					{
						group.isExpanded = !group.isExpanded;
					}
					else
					{
						//ShowHeaderContextMenu(e.mousePosition, target, resetAction, removeAction);
					}

						e.Use();
				}
			}

			return group.isExpanded;
		}
	}

	public static class Styling
	{
		public static readonly GUIStyle smallTickbox;
		public static readonly GUIStyle miniLabelButton;

		public static readonly Texture2D paneOptionsIconDark;
		public static readonly Texture2D paneOptionsIconLight;

		public static readonly GUIStyle labelHeader;

		public static readonly GUIStyle wheelLabel;
		public static readonly GUIStyle wheelThumb;
		public static readonly Vector2 wheelThumbSize;

		public static readonly GUIStyle preLabel;

		static Texture2D m_TransparentTexture;
		public static Texture2D transparentTexture
		{
			get
			{
				if (m_TransparentTexture == null)
				{
					m_TransparentTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
					m_TransparentTexture.SetPixel(0, 0, Color.clear);
					m_TransparentTexture.Apply();
				}

				return m_TransparentTexture;
			}
		}

		static Styling()
		{
			smallTickbox = new GUIStyle("ShurikenCheckMark");

			miniLabelButton = new GUIStyle(EditorStyles.miniLabel);
			miniLabelButton.normal = new GUIStyleState
			{
				background = transparentTexture,
				scaledBackgrounds = null,
				textColor = Color.grey
			};
			var activeState = new GUIStyleState
			{
				background = transparentTexture,
				scaledBackgrounds = null,
				textColor = Color.white
			};
			miniLabelButton.active = activeState;
			miniLabelButton.onNormal = activeState;
			miniLabelButton.onActive = activeState;

			paneOptionsIconDark = (Texture2D)EditorGUIUtility.Load("Builtin Skins/DarkSkin/Images/pane options.png");
			paneOptionsIconLight = (Texture2D)EditorGUIUtility.Load("Builtin Skins/LightSkin/Images/pane options.png");

			labelHeader = new GUIStyle(EditorStyles.miniLabel);

			wheelThumb = new GUIStyle("ColorPicker2DThumb");

			wheelThumbSize = new Vector2(
				!Mathf.Approximately(wheelThumb.fixedWidth, 0f) ? wheelThumb.fixedWidth : wheelThumb.padding.horizontal,
				!Mathf.Approximately(wheelThumb.fixedHeight, 0f) ? wheelThumb.fixedHeight : wheelThumb.padding.vertical
			);

			wheelLabel = new GUIStyle(EditorStyles.miniLabel);

			preLabel = new GUIStyle("ShurikenLabel");
		}
	}
}
