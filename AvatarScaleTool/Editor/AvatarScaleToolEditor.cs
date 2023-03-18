#if CVR_CCK_EXISTS
using ABI.CCK.Components;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class AvatarScaleTool : EditorWindow
{
    public static float referenceAvatarHeight = 1.8f;
    public static float locomotionSpeedModifier = 1f;

    public float initialHeight = 1.0f;
    public float minimumHeight = 0.5f;
    public float maximumHeight = 2.0f;
    CVRAvatar cvrAvatar;
    bool showGizmos;

    [MenuItem("NotAKid/Avatar Scale Tool")]
    public static void ShowWindow()
    {
        GetWindow<AvatarScaleTool>("Avatar Scale Tool");
    }

    private void OnEnable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    private void OnGUI()
    {
        DrawAvatarSelection();
        if (cvrAvatar == null)
        {
            DrawAvatarSelectionHint();
            return;
        }

        DrawScaleSettings();
        DrawLocomotionSpeedInfo();
        DrawGenerateScaleAnimationButton();
        DrawSeparator();
        DrawSettingsField();
    }

    private void DrawAvatarSelection()
    {
        cvrAvatar = (CVRAvatar)EditorGUILayout.ObjectField(
            "Selected Avatar", cvrAvatar, typeof(CVRAvatar), true);
        if (GUILayout.Button("Use Selection"))
        {
            cvrAvatar = Selection.gameObjects
                .Select(obj => obj.GetComponentInParent<CVRAvatar>())
                .FirstOrDefault();
        }
    }

    private void DrawAvatarSelectionHint()
    {
        EditorGUILayout.HelpBox("Please select an Avatar first.", MessageType.Info);
    }

    private void DrawScaleSettings()
    {
        GUIStyle box = GUI.skin.GetStyle("box");
        using (new GUILayout.VerticalScope(box))
        {
            GUILayout.Label("Scale Settings", EditorStyles.boldLabel);
            DrawInitialHeightField();
            DrawMinimumHeightField();
            DrawMaximumHeightField();
            DrawReferenceAvatarHeightField();
        }
    }

    private void DrawInitialHeightField()
    {
        initialHeight = cvrAvatar.viewPosition.y;
    }

    private void DrawMinimumHeightField()
    {
        // Display the minimum Height field
        minimumHeight = EditorGUILayout.FloatField("Minimum Height", minimumHeight);

        // Make sure the minimum Height is not greater than the initial Height
        if (minimumHeight > initialHeight)
        {
            minimumHeight = initialHeight;
        }

        // Make sure the minimum Height cannot go to zero
        if (minimumHeight < 0)
        {
            minimumHeight = 0.05f;
        }
    }

    private void DrawMaximumHeightField()
    {
        // Display the maximum Height field
        maximumHeight = EditorGUILayout.FloatField("Maximum Height", maximumHeight);

        // Make sure the maximum Height is not less than the initial Height 
        if (maximumHeight < initialHeight)
        {
            maximumHeight = initialHeight;
        }
    }

    private void DrawReferenceAvatarHeightField()
    {
        // Reference Avatar Height
        EditorGUILayout.BeginHorizontal();
        AvatarScaleTool.referenceAvatarHeight = EditorGUILayout.FloatField("Reference Avatar Height", AvatarScaleTool.referenceAvatarHeight);
        if (GUILayout.Button("Reset"))
        {
            AvatarScaleTool.referenceAvatarHeight = 1.8f;
        }
        EditorGUILayout.EndHorizontal();
    }

    private void DrawGenerateScaleAnimationButton()
    {
        if (GUILayout.Button("Generate Scale Animation"))
        {
            CreateAvatarScaleAnimation();
        }
    }

    private (float, float, float) CalculateSpeedInfo()
    {
        float heightRatio = AvatarScaleTool.referenceAvatarHeight / initialHeight;
        float minSpeed = AvatarScaleTool.referenceAvatarHeight / minimumHeight;
        float maxSpeed = AvatarScaleTool.referenceAvatarHeight / maximumHeight;

        return (heightRatio, minSpeed, maxSpeed);
    }

    private void DrawLocomotionSpeedInfo()
    {
        (float heightRatio, float minSpeed, float maxSpeed) = CalculateSpeedInfo();
        float locomotionSpeedModifier = AvatarScaleTool.locomotionSpeedModifier;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Avatar Scale Information", EditorStyles.boldLabel);

        float heightPercentage = Mathf.InverseLerp(minimumHeight, maximumHeight, initialHeight);
        EditorGUILayout.LabelField("Initial Height Percentage:", $"{heightPercentage:F2}%");

        EditorGUILayout.LabelField("Viewpoint Height:", $"{initialHeight}m (x{heightRatio:F2})");
        EditorGUILayout.LabelField("Minimum Height:", $"{minimumHeight}m (x{minSpeed:F2})");
        EditorGUILayout.LabelField("Maximum Height:", $"{maximumHeight}m (x{maxSpeed:F2})");

        string locomotionSpeedInfo = "The right-hand number is the ratio of the reference avatar height to the set minimum and maximum heights. It's used to adjust the speed of locomotion animations to match the avatar's scale.";
        EditorGUILayout.HelpBox(locomotionSpeedInfo, MessageType.Info);
    }

    private void DrawSettingsField()
    {
        showGizmos = EditorGUILayout.Toggle("Show Gizmos", showGizmos);
    }

    private void OnSceneGUI(SceneView v)
    {
        if (cvrAvatar == null || !showGizmos) return;

        Transform avatarRoot = cvrAvatar.transform;

        Handles.matrix = Matrix4x4.TRS(avatarRoot.position, avatarRoot.rotation, Vector3.one);

        DrawGizmoBackgroundRect(maximumHeight);

        DrawGizmoLine(initialHeight, Color.blue);
        DrawGizmoLine(minimumHeight, Color.green);
        DrawGizmoLine(maximumHeight, Color.red);

        DrawHeightLabel("Initial Height", initialHeight);
        DrawHeightLabel("Minimum Height", minimumHeight);
        DrawHeightLabel("Maximum Height", maximumHeight);

        Handles.matrix = Matrix4x4.identity;
    }

    private void DrawGizmoBackgroundRect(float maxHeight)
    {
        Handles.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);

        Vector3 topLeft = new Vector3(-0.5f, 0f, 0f);
        Vector3 bottomLeft = new Vector3(-0.5f, maxHeight, 0f);
        Vector3 topRight = new Vector3(0.5f, 0f, 0f);
        Vector3 bottomRight = new Vector3(0.5f, maxHeight, 0f);

        Handles.DrawSolidRectangleWithOutline(
            new Vector3[] { topLeft, bottomLeft, bottomRight, topRight },
            new Color(0.5f, 0.5f, 0.5f, 0.5f),
            new Color(0f, 0f, 0f, 0f)
        );
    }

    private void DrawGizmoLine(float height, Color color)
    {
        Handles.color = color;
        Vector3 startPos = new Vector3(0.5f, height, 0f);
        Vector3 endPos = new Vector3(-0.5f, height, 0f);
        Handles.DrawLine(startPos, endPos);
    }

    private void DrawHeightLabel(string heightText, float height)
    {
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.white;
        style.alignment = TextAnchor.MiddleCenter;
        style.fontSize = 16;

        string text = $"{heightText}\n{height}";
        Vector3 pos = new Vector3(-0.5f, height + 0.05f, 0f);
        Handles.Label(pos, text, style);
    }

    private void CreateAvatarScaleAnimation()
    {
        AnimationClip clip = new AnimationClip();
        float initialToMinHeightRatio = minimumHeight / initialHeight;
        float initialToMaxHeightRatio = maximumHeight / initialHeight;
        float minLocoSpeed = AvatarScaleTool.referenceAvatarHeight / minimumHeight;
        float maxLocoSpeed = AvatarScaleTool.referenceAvatarHeight / maximumHeight;

        for (int i = 0; i < 3; i++)
        {
            float initialScale = cvrAvatar.transform.localScale[i];
            float minScale = initialScale * initialToMinHeightRatio;
            float maxScale = initialScale * initialToMaxHeightRatio;

            Keyframe[] keys = new Keyframe[]
            {
                new Keyframe(0f, minScale),
                new Keyframe(0.0333333333333333f, maxScale),
            };

            AnimationCurve curve = new AnimationCurve(keys);
            clip.SetCurve("", typeof(Transform), $"localScale.{TransformPropertyNames[i]}", curve);
        }

        Keyframe[] motionKeys = new Keyframe[]
        {
            new Keyframe(0f, minLocoSpeed),
            new Keyframe(0.0333333333333333f, maxLocoSpeed),
        };
        AnimationCurve motionScaleCurve = new AnimationCurve(motionKeys);
        clip.SetCurve("", typeof(Animator), "#MotionScale", motionScaleCurve);

        string clipName = $"AvatarScale_{cvrAvatar.name}";
        string savePath = EditorUtility.SaveFilePanelInProject("Save Animation Clip", clipName, "anim",
            "Please enter a file name to save the animation clip to.");

        if (!string.IsNullOrEmpty(savePath))
        {
            AssetDatabase.CreateAsset(clip, savePath);
            AssetDatabase.SaveAssets();
            Debug.Log($"Avatar scale animation clip created and saved to {savePath}");
        }
    }

    private static readonly string[] TransformPropertyNames = { "x", "y", "z" };

    //Nicked from commissioned script by Dreadrith
    private static void DrawSeparator(int thickness = 2, int padding = 10)
    {
        Rect rect = EditorGUILayout.GetControlRect(GUILayout.Height(thickness + padding));
        rect.height = thickness;
        rect.y += padding / 2f;
        rect.x -= 2;
        rect.width += 6;

        Color lineColor = EditorGUIUtility.isProSkin ? new Color32(89, 89, 89, 255) : new Color32(133, 133, 133, 255);
        EditorGUI.DrawRect(rect, lineColor);
    }
}

#endif