#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace SimpleInspectorSearch
{
    [InitializeOnLoad]
    public static class InspectorSearchManager
    {
        private static readonly List<InspectorSearchWrapper> wrappers = new List<InspectorSearchWrapper>();
        private static Type inspectorWindowType;

        static InspectorSearchManager()
        {
            inspectorWindowType = typeof(Editor).Assembly.GetType("UnityEditor.InspectorWindow");
            EditorApplication.update -= UpdateWrappers;
            EditorApplication.update += UpdateWrappers;
        }

        private static void UpdateWrappers()
        {
            var windows = Resources.FindObjectsOfTypeAll(inspectorWindowType);
            if (windows == null) return;

            foreach (EditorWindow window in windows)
            {
                if (!HasWrapper(window)) wrappers.Add(new InspectorSearchWrapper(window));
            }

            for (int i = wrappers.Count - 1; i >= 0; i--)
            {
                if (wrappers[i].IsValid) wrappers[i].Update();
                else
                {
                    wrappers[i].Dispose();
                    wrappers.RemoveAt(i);
                }
            }
        }

        private static bool HasWrapper(EditorWindow window)
        {
            for (int i = 0; i < wrappers.Count; i++)
                if (wrappers[i].Window == window) return true;
            return false;
        }
    }

    public class InspectorSearchWrapper
    {
        public EditorWindow Window { get; private set; }
        public bool IsValid => Window != null;

        private VisualElement rootVisualElement;
        private IMGUIContainer searchBarContainer;
        private IMGUIContainer searchResultsContainer;
        
        // We need to keep a reference to the scrollview to manipulate it
        private ScrollView mainScrollView; 
        private VisualElement mainEditorList;

        private const string InspectorListClassName = "unity-inspector-editors-list"; 
        private const string ScrollViewClassName = "unity-inspector-root-scrollview"; 
        private const string SearchBarName = "SimpleSearchBar";
        private const string SearchResultsName = "SimpleSearchResults";
        private const string WrapperName = "InspectorSearchLayoutWrapper";

        private Object currentTarget;
        private string searchText = "";
        private bool needsSearchUpdate = false;
        
        private struct SearchResult
        {
            public Component Component;
            public SerializedObject SerializedObj;
            public SerializedProperty Property;
            public string PropertyPath; // Stored to help calculation
        }
        private List<SearchResult> foundProperties = new List<SearchResult>();

        public InspectorSearchWrapper(EditorWindow window)
        {
            Window = window;
            rootVisualElement = window.rootVisualElement;
        }

        public void Update()
        {
            if (!IsValid) return;

            // Cache these for the Jump function
            mainEditorList = rootVisualElement.Q(className: InspectorListClassName);
            mainScrollView = rootVisualElement.Q<ScrollView>(className: ScrollViewClassName);

            if (mainEditorList == null || mainScrollView == null) return;

            // --- SAFE INJECTION LOGIC (Wraps the UI) ---
            var parent = mainScrollView.parent;

            if (parent.name == WrapperName)
            {
                if (parent.Q(SearchBarName) == null)
                {
                    CreateSearchBar();
                    parent.Insert(0, searchBarContainer);
                }
            }
            else if (parent.GetType().Name.Contains("SplitView")) 
            {
                var wrapper = new VisualElement { name = WrapperName };
                wrapper.style.flexGrow = 1;
                int index = parent.IndexOf(mainScrollView);
                parent.Remove(mainScrollView);
                parent.Insert(index, wrapper);
                wrapper.Add(mainScrollView);
                CreateSearchBar();
                wrapper.Insert(0, searchBarContainer);
            }
            else 
            {
                if (parent.Q(SearchBarName) == null)
                {
                    CreateSearchBar();
                    int scrollIndex = parent.IndexOf(mainScrollView);
                    parent.Insert(scrollIndex, searchBarContainer);
                }
            }

            // --- RESULTS LOGIC ---
            if (mainEditorList.Q(SearchResultsName) == null)
            {
                CreateResultsContainer();
                mainEditorList.Insert(0, searchResultsContainer);
            }

            if (Selection.activeObject != currentTarget)
            {
                currentTarget = Selection.activeObject;
                needsSearchUpdate = true;
            }

            if (needsSearchUpdate)
            {
                PerformSearch();
                needsSearchUpdate = false;
                UpdateVisibility(mainEditorList);
                if (searchResultsContainer != null) searchResultsContainer.MarkDirtyRepaint();
            }

            if (Application.isPlaying && !string.IsNullOrEmpty(searchText) && searchResultsContainer != null)
            {
                searchResultsContainer.MarkDirtyRepaint();
            }
        }

        public void Dispose()
        {
            if (searchBarContainer != null) searchBarContainer.RemoveFromHierarchy();
            if (searchResultsContainer != null) searchResultsContainer.RemoveFromHierarchy();
        }

        private void CreateSearchBar()
        {
            searchBarContainer = new IMGUIContainer(DrawSearchBar) { name = SearchBarName };
            searchBarContainer.style.flexShrink = 0; 
            searchBarContainer.style.height = 32f; 
            //searchBarContainer.style.padding = 4f;
            searchBarContainer.style.backgroundColor = new StyleColor(new Color(0.2f, 0.2f, 0.2f, 1f));
            searchBarContainer.style.borderBottomWidth = 1f;
            searchBarContainer.style.borderBottomColor = new StyleColor(Color.black);
        }

        private void CreateResultsContainer()
        {
            searchResultsContainer = new IMGUIContainer(DrawSearchResults) { name = SearchResultsName };
            searchResultsContainer.style.flexGrow = 1;
        }

        private void DrawSearchBar()
        {
            GUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            string newText = EditorGUILayout.TextField(searchText, EditorStyles.toolbarSearchField);
            if (EditorGUI.EndChangeCheck())
            {
                searchText = newText;
                needsSearchUpdate = true;
            }

            if (!string.IsNullOrEmpty(searchText))
            {
                if (GUILayout.Button("X", EditorStyles.toolbarButton, GUILayout.Width(20)))
                {
                    searchText = "";
                    needsSearchUpdate = true;
                    GUI.FocusControl(null);
                }
            }
            else
            {
                GUILayout.Space(20);
            }
            GUILayout.EndHorizontal();
        }

        private void DrawSearchResults()
        {
            if (string.IsNullOrWhiteSpace(searchText)) return;

            if (foundProperties.Count == 0)
            {
                GUILayout.Label("No matching parameters found.", EditorStyles.centeredGreyMiniLabel);
                return;
            }

            Component lastComp = null;
            GUILayout.Space(5);
            
            foreach (var result in foundProperties)
            {
                if (result.Component == null || result.SerializedObj == null || result.SerializedObj.targetObject == null) 
                    continue;

                result.SerializedObj.Update(); 

                if (result.Component != lastComp)
                {
                    if (lastComp != null) GUILayout.Space(10);
                    GUILayout.BeginHorizontal();
                    GUIContent headerContent = EditorGUIUtility.ObjectContent(result.Component, result.Component.GetType());
                    if (headerContent.image != null)
                        GUILayout.Label(headerContent.image, GUILayout.Width(16), GUILayout.Height(16));
                    GUILayout.Label(result.Component.GetType().Name, EditorStyles.boldLabel);
                    GUILayout.EndHorizontal();
                    lastComp = result.Component;
                }

                GUILayout.BeginHorizontal(); 
                EditorGUI.indentLevel++;
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(result.Property, true);
                if (EditorGUI.EndChangeCheck())
                {
                    result.SerializedObj.ApplyModifiedProperties();
                }
                EditorGUI.indentLevel--;

                // --- JUMP BUTTON ---
                if (GUILayout.Button(EditorGUIUtility.IconContent("d_scenepicking_pickable_hover"), GUILayout.Width(24), GUILayout.Height(18)))
                {
                    // Pass the specific result logic
                    JumpToLocation(result.Component, result.PropertyPath);
                }
                GUILayout.EndHorizontal(); 
            }
            GUILayout.Space(10);
        }

        // --- NEW: LOGIC TO JUMP TO EXACT LOCATION ---
        private void JumpToLocation(Component comp, string propertyPath)
        {
            // 1. Clear Search to show the normal Inspector
            searchText = "";
            needsSearchUpdate = true;
            GUI.FocusControl(null);
            
            // 2. Expand the component so the variable is visible
            InternalEditorUtility.SetIsInspectorExpanded(comp, true);
            EditorGUIUtility.PingObject(comp);

            // 3. We must wait one frame (DelayCall).
            // Why? Because we just set searchText="", which triggers 'UpdateVisibility'.
            // The VisualElements are currently "display:none". They become "display:flex" in the next repaint.
            // We cannot calculate layout positions until they are visible.
            EditorApplication.delayCall += () =>
            {
                if (mainScrollView == null || mainEditorList == null) return;

                // Find the index of this component in the GameObject
                // The Inspector UI List usually matches the GetComponents order
                Component[] components = comp.gameObject.GetComponents<Component>();
                int compIndex = -1;
                for (int i = 0; i < components.Length; i++)
                {
                    if (components[i] == comp)
                    {
                        compIndex = i;
                        break;
                    }
                }

                if (compIndex != -1 && compIndex < mainEditorList.childCount)
                {
                    // Get the VisualElement for the whole component
                    VisualElement componentElement = mainEditorList[compIndex]; 
                    
                    // -- ESTIMATE PROPERTY OFFSET --
                    // We can't ask Unity "Where is property X?" directly.
                    // But we can count how many properties are above it.
                    float estimatedOffset = CalculatePropertyOffset(comp, propertyPath);

                    // -- APPLY SCROLL --
                    // layout.y gives the Y position of the Component relative to the list
                    // estimatedOffset moves us down into the component
                    float targetY = componentElement.layout.y + estimatedOffset;

                    // Scroll!
                    mainScrollView.scrollOffset = new Vector2(0, targetY);
                }
            };
        }

        private float CalculatePropertyOffset(Component comp, string targetPath)
        {
            // Base offset for the Component Header (Script field, Icon, etc)
            float offset = 56f; // Rough pixel height of the component header
            float lineHeight = 21.9f; // Standard single line height in Inspector

            SerializedObject so = new SerializedObject(comp);
            SerializedProperty iter = so.GetIterator();
            bool enterChildren = true;

            // Iterate through properties just like the Inspector does
            while (iter.NextVisible(enterChildren))
            {
                enterChildren = false;

                // If we found our target, stop counting
                if (iter.propertyPath == targetPath)
                {
                    break;
                }

                // Add height for every property before the target
                // If it's a simple type (float/int), it's usually 1 line.
                // If it's a Vector3, it takes up more space if expanded (but usually 1 line in default view).
                // Getting exact height (EditorGUI.GetPropertyHeight) requires drawing, which we can't do here.
                // So we estimate:
                
                if (iter.isExpanded) 
                {
                    // If a foldout is open above us, we add a bit more arbitrary height
                    offset += lineHeight * 2; 
                }
                else
                {
                    offset += lineHeight;
                }
            }
            
            // Subtract a little buffer so the variable isn't hugigng the very top edge
            return Mathf.Max(0, offset - 40f); 
        }

        private void PerformSearch()
        {
            foundProperties.Clear();
            if (string.IsNullOrWhiteSpace(searchText) || currentTarget is not GameObject go) return;

            string searchRaw = searchText.Replace(" ", "").ToLower(); 

            Component[] components = go.GetComponents<Component>();

            foreach (Component comp in components)
            {
                if (!comp) continue;
                bool componentNameMatch = comp.GetType().Name.ToLower().Contains(searchRaw);

                SerializedObject so = new SerializedObject(comp);
                SerializedProperty iterator = so.GetIterator();
                bool enterChildren = true; 

                while (iterator.NextVisible(enterChildren))
                {
                    enterChildren = false; 
                    string displayRaw = iterator.displayName.Replace(" ", "").ToLower();
                    string nameRaw = iterator.name.Replace(" ", "").ToLower();

                    if (displayRaw.Contains(searchRaw) || nameRaw.Contains(searchRaw) || componentNameMatch)
                    {
                        foundProperties.Add(new SearchResult
                        {
                            Component = comp,
                            SerializedObj = so,
                            Property = iterator.Copy(),
                            PropertyPath = iterator.propertyPath // Save path for Jump
                        });
                    }
                }
            }
        }

        private void UpdateVisibility(VisualElement list)
        {
            bool isSearching = !string.IsNullOrWhiteSpace(searchText);
            // Hide normal children when searching
            for (int i = 0; i < list.childCount; i++)
            {
                var child = list[i];
                // Don't hide our search results container!
                if (child.name == SearchResultsName) continue;
                child.style.display = isSearching ? DisplayStyle.None : DisplayStyle.Flex;
            }

            if (searchResultsContainer != null)
                searchResultsContainer.style.display = isSearching ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}
#endif