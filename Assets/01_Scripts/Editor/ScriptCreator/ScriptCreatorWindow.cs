using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class ScriptCreatorWindow : EditorWindow
{
    private string scriptNames = "NewScript";
    private int selectedTemplateIndex = 0;
    private int previousTemplateIndex = -1;
    private string[] templatePaths;
    private string[] templateNames;
    private string targetFolderPath = "Assets";
    private Vector2 textAreaScroll;
 
    // Dynamic placeholder system
    private Dictionary<string, string> placeholderValues = new Dictionary<string, string>();
    private List<string> currentPlaceholders = new List<string>();
 
    // Default values for known placeholders
    private static readonly Dictionary<string, System.Func<string, string>> placeholderDefaults
        = new Dictionary<string, System.Func<string, string>>()
    {
        { "FILENAME",  (scriptName) => scriptName },
        { "MENUNAME",  (scriptName) => "Game/" + scriptName },
        { "NAMESPACE", (scriptName) => "Game" },
    };
 
    // Auto-located template folder (no hardcoded const)
    private string templateFolderPath;
 
    // Cached reflection
    private static System.Type projectBrowserType;
    private static FieldInfo lastFoldersField;
    private static bool reflectionInitialized = false;
 
    [MenuItem("Tools/スクリプト作成ツール")]
    public static void ShowWindow()
    {
        GetWindow<ScriptCreatorWindow>("スクリプト作成");
    }
 
    private void OnEnable()
    {
        RefreshTemplates();
        InitReflection();
        UpdateTargetFolder();
        EditorApplication.update += OnEditorUpdate;
    }
 
    private void OnDisable()
    {
        EditorApplication.update -= OnEditorUpdate;
    }
 
    private void OnSelectionChange()
    {
        UpdateTargetFolder();
        Repaint();
    }
 
    // Called by Unity when assets are imported, moved, or deleted
    private void OnProjectChange()
    {
        RefreshTemplates();
        previousTemplateIndex = -1; // force placeholder re-scan
        Repaint();
    }
 
    private void OnEditorUpdate()
    {
        string previous = targetFolderPath;
        UpdateTargetFolder();
        if (targetFolderPath != previous)
        {
            Repaint();
        }
    }
 
    // ─────────────────────────────────────────────
    // AUTO-LOCATE TEMPLATE FOLDER
    // ─────────────────────────────────────────────
 
    private string FindTemplateFolderPath()
    {
        List<string> candidates = new List<string>();
 
        // 1. AssetDatabase search (fast, indexed)
        string[] guids = AssetDatabase.FindAssets("ScriptTemplates");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (AssetDatabase.IsValidFolder(path) && Path.GetFileName(path) == "ScriptTemplates")
                candidates.Add(path);
        }
 
        // 2. Filesystem fallback (slower, catches unimported folders)
        if (candidates.Count == 0)
        {
            string[] dirs = Directory.GetDirectories("Assets", "ScriptTemplates", SearchOption.AllDirectories);
            foreach (string dir in dirs)
                candidates.Add(dir.Replace("\\", "/"));
        }
 
        if (candidates.Count == 0) return null;
        if (candidates.Count == 1) return candidates[0];
 
        // Multiple found: prefer one inside an Editor folder
        string editorMatch = candidates.FirstOrDefault(p => p.Contains("/Editor/"));
        if (editorMatch != null) return editorMatch;
 
        // Otherwise, use the saved preference or the first match
        string saved = EditorPrefs.GetString("ScriptCreator_TemplatePath", "");
        if (candidates.Contains(saved)) return saved;
 
        return candidates[0];
    }
 
    // ─────────────────────────────────────────────
    // BATCH NAME PARSING & VALIDATION
    // ─────────────────────────────────────────────
 
    /// Splits the multi-line text area into clean, unique class names
    private string[] ParseScriptNames()
    {
        return scriptNames
            .Split('\n')
            .Select(s => s.Trim().Replace(" ", ""))
            .Where(s => !string.IsNullOrEmpty(s))
            .Distinct()
            .ToArray();
    }
 
    /// Validates all names before any file is created (atomic batch)
    private List<string> ValidateNames(string[] names)
    {
        List<string> errors = new List<string>();
 
        foreach (string name in names)
        {
            if (!Regex.IsMatch(name, @"^[a-zA-Z_][a-zA-Z0-9_]*$"))
            {
                errors.Add($"'{name}' は有効なC#クラス名ではありません。");
            }
            else
            {
                string fullPath = Path.Combine(targetFolderPath, name + ".cs");
                if (File.Exists(fullPath))
                {
                    errors.Add($"'{name}.cs' は既に存在します: {targetFolderPath}");
                }
            }
        }
 
        return errors;
    }
 
    // ─────────────────────────────────────────────
    // TEMPLATE PLACEHOLDER DETECTION
    // ─────────────────────────────────────────────
 
    private void RefreshPlaceholders()
    {
        currentPlaceholders.Clear();
 
        if (templatePaths == null || templatePaths.Length == 0) return;
        if (selectedTemplateIndex >= templatePaths.Length) return;
 
        string content = File.ReadAllText(templatePaths[selectedTemplateIndex]);
 
        var matches = Regex.Matches(content, @"#([A-Z_]+)#");
 
        HashSet<string> seen = new HashSet<string>();
        foreach (Match match in matches)
        {
            string name = match.Groups[1].Value;
 
            // SCRIPTNAME and FILENAME are handled automatically per-script
            if (name == "SCRIPTNAME" || name == "FILENAME") continue;
 
            if (!seen.Add(name)) continue;
 
            currentPlaceholders.Add(name);
 
            if (!placeholderValues.ContainsKey(name))
            {
                if (placeholderDefaults.TryGetValue(name, out var defaultFunc))
                {
                    string firstScript = ParseScriptNames().FirstOrDefault() ?? "NewScript";
                    placeholderValues[name] = defaultFunc(firstScript);
                }
                else
                {
                    placeholderValues[name] = "";
                }
            }
        }
    }
 
    private string PlaceholderToLabel(string placeholder)
    {
        string spaced = Regex.Replace(placeholder, @"([A-Z])(?=[A-Z][a-z])|([a-z])(?=[A-Z])", "$0 ");
        return System.Globalization.CultureInfo.InvariantCulture.TextInfo
            .ToTitleCase(spaced.ToLower());
    }
 
    // ─────────────────────────────────────────────
    // REFLECTION
    // ─────────────────────────────────────────────
 
    private static void InitReflection()
    {
        if (reflectionInitialized) return;
        projectBrowserType = typeof(Editor).Assembly.GetType("UnityEditor.ProjectBrowser");
        if (projectBrowserType != null)
        {
            lastFoldersField = projectBrowserType.GetField(
                "m_LastFolders", BindingFlags.Instance | BindingFlags.NonPublic);
        }
        reflectionInitialized = true;
    }
 
    private void UpdateTargetFolder()
    {
        foreach (Object obj in Selection.GetFiltered(typeof(Object), SelectionMode.Assets))
        {
            string assetPath = AssetDatabase.GetAssetPath(obj);
            if (string.IsNullOrEmpty(assetPath)) continue;
            if (Directory.Exists(assetPath)) { targetFolderPath = assetPath; return; }
            else if (File.Exists(assetPath)) { targetFolderPath = Path.GetDirectoryName(assetPath); return; }
        }
        string browserFolder = GetProjectBrowserFolder();
        if (!string.IsNullOrEmpty(browserFolder)) { targetFolderPath = browserFolder; return; }
        targetFolderPath = "Assets";
    }
 
    private string GetProjectBrowserFolder()
    {
        if (projectBrowserType == null || lastFoldersField == null) return null;
        var browsers = Resources.FindObjectsOfTypeAll(projectBrowserType);
        if (browsers.Length == 0) return null;
        string[] lastFolders = lastFoldersField.GetValue(browsers[0]) as string[];
        if (lastFolders != null && lastFolders.Length > 0 && !string.IsNullOrEmpty(lastFolders[0]))
            return lastFolders[0];
        return null;
    }
 
    private void RefreshTemplates()
    {
        templateFolderPath = FindTemplateFolderPath();
 
        if (string.IsNullOrEmpty(templateFolderPath))
        {
            templateFolderPath = "Assets/Editor/ScriptTemplates";
            Directory.CreateDirectory(templateFolderPath);
            AssetDatabase.Refresh();
        }
 
        templatePaths = Directory.GetFiles(templateFolderPath, "*.txt");
        templateNames = templatePaths.Select(p => Path.GetFileNameWithoutExtension(p)).ToArray();
    }
 
    // ─────────────────────────────────────────────
    // GUI
    // ─────────────────────────────────────────────
 
    private void OnGUI()
    {
        UpdateTargetFolder();
 
        GUILayout.Label("新規スクリプト作成", EditorStyles.boldLabel);
        EditorGUILayout.Space();
 
        // 1. Script Names (multi-line)
        EditorGUILayout.LabelField("スクリプト名（複数対応）");
        textAreaScroll = EditorGUILayout.BeginScrollView(textAreaScroll, GUILayout.MinHeight(60), GUILayout.MaxHeight(120));
        scriptNames = EditorGUILayout.TextArea(scriptNames, GUILayout.ExpandHeight(true));
        EditorGUILayout.EndScrollView();
 
        // 2. Template Selection
        if (templateNames == null || templateNames.Length == 0)
        {
            EditorGUILayout.HelpBox(
                "テンプレートが見つかりません: " + templateFolderPath +
                "\n.txt テンプレートファイルを配置してください。", MessageType.Warning);
            if (GUILayout.Button("更新")) RefreshTemplates();
            return;
        }
 
        selectedTemplateIndex = EditorGUILayout.Popup("テンプレート", selectedTemplateIndex, templateNames);
 
        // Detect template change → re-scan placeholders
        if (selectedTemplateIndex != previousTemplateIndex)
        {
            previousTemplateIndex = selectedTemplateIndex;
            RefreshPlaceholders();
        }
 
        // 3. Dynamic placeholder fields (shared across all scripts)
        if (currentPlaceholders.Count > 0)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("テンプレートパラメーター（全スクリプト共通）", EditorStyles.miniLabel);
 
            foreach (string placeholder in currentPlaceholders)
            {
                string label = PlaceholderToLabel(placeholder);
                string current = placeholderValues.ContainsKey(placeholder)
                    ? placeholderValues[placeholder] : "";
                placeholderValues[placeholder] = EditorGUILayout.TextField(label, current);
            }
        }
 
        // 4. Target Folder
        EditorGUILayout.Space();
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.TextField("作成先フォルダ", targetFolderPath);
        EditorGUI.EndDisabledGroup();
 
        EditorGUILayout.Space();
 
        // 5. Preview & Create Button
        string[] previewNames = ParseScriptNames();
 
        if (previewNames.Length > 1)
        {
            EditorGUILayout.HelpBox(
                $"{previewNames.Length} 個のスクリプトを作成します:\n" +
                string.Join(", ", previewNames),
                MessageType.Info);
        }
 
        string buttonLabel = previewNames.Length > 1
            ? $"作成（{previewNames.Length}個）"
            : "作成";
 
        if (GUILayout.Button(buttonLabel, GUILayout.Height(40)))
        {
            CreateScripts();
        }
    }
 
    // ─────────────────────────────────────────────
    // BATCH SCRIPT CREATION
    // ─────────────────────────────────────────────
 
    private void CreateScripts()
    {
        string[] names = ParseScriptNames();
 
        if (names.Length == 0)
        {
            Debug.LogError("スクリプト名を入力してください。");
            return;
        }
 
        // Validate ALL names before creating ANY (atomic batch)
        List<string> errors = ValidateNames(names);
        if (errors.Count > 0)
        {
            EditorUtility.DisplayDialog("エラー",
                string.Join("\n", errors), "OK");
            return;
        }
 
        // Safety: re-resolve if the cached template path is stale
        if (!File.Exists(templatePaths[selectedTemplateIndex]))
        {
            RefreshTemplates();
            previousTemplateIndex = -1;
            EditorUtility.DisplayDialog("テンプレート更新",
                "テンプレートフォルダが移動されたため、パスを更新しました。\nもう一度「作成」を押してください。", "OK");
            return;
        }
 
        // Read the template once (all scripts share the same template)
        string templateContent = File.ReadAllText(templatePaths[selectedTemplateIndex]);
        List<string> createdPaths = new List<string>();
 
        foreach (string className in names)
        {
            string content = templateContent;
 
            // Per-script automatic replacements
            content = content.Replace("#SCRIPTNAME#", className);
            content = content.Replace("#FILENAME#", className);
 
            // Shared placeholder replacements
            foreach (var kvp in placeholderValues)
            {
                if (kvp.Key == "FILENAME") continue; // already handled per-script
                content = content.Replace($"#{kvp.Key}#", kvp.Value);
            }
 
            string fullPath = Path.Combine(targetFolderPath, className + ".cs");
            File.WriteAllText(fullPath, content);
            createdPaths.Add(fullPath);
        }
 
        // Single AssetDatabase refresh for the entire batch
        AssetDatabase.Refresh();
 
        // Open and select all created scripts
        List<Object> createdAssets = new List<Object>();
        foreach (string path in createdPaths)
        {
            Object asset = AssetDatabase.LoadAssetAtPath<Object>(path);
            if (asset != null)
            {
                createdAssets.Add(asset);
                AssetDatabase.OpenAsset(asset);
            }
        }
 
        // Highlight all in Project window
        Selection.objects = createdAssets.ToArray();
 
        Debug.Log($"{createdPaths.Count} 個のスクリプトを作成しました: {targetFolderPath}");
    }
}