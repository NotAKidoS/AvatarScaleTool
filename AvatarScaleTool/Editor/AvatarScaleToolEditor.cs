#if CVR_CCK_EXISTS
using ABI.CCK.Components;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.Animations;
using UnityEngine.Assertions;

public class AvatarScaleTool : EditorWindow
{
    
    public static float referenceAvatarHeight = 1.8f;
    public static float locomotionSpeedModifier = 1f;

    public CVRAvatar cvrAvatar;
    public AnimatorController customController;
    public float initialHeight = 1.0f;
    public float minimumHeight = 0.5f;
    public float maximumHeight = 2.0f;
    public bool motionScaleFloat = true;
    public bool scaleDynamicBone = false;
    public bool scaleAudioSources = true;

    bool useCustomController;
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
        DrawOptionalScaleSettings();
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
            GUILayout.Label("Scale Settings (Meters)", EditorStyles.boldLabel);
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

    private void DrawOptionalScaleSettings()
    {
        EditorGUILayout.Space();
        GUILayout.Label("Optional Settings", EditorStyles.boldLabel);
        GUIStyle box = GUI.skin.GetStyle("box");
        using (new GUILayout.VerticalScope(box))
        {
            motionScaleFloat = EditorGUILayout.Toggle("#MotionScale Float", motionScaleFloat);
            scaleDynamicBone = EditorGUILayout.Toggle("Scale Dynamic Bones", scaleDynamicBone);
            scaleAudioSources = EditorGUILayout.Toggle("Scale Audio Sources", scaleAudioSources);
        }
    }

    private void DrawGenerateScaleAnimationButton()
    {
        EditorGUILayout.Space();

        string helpText = null;

        if (useCustomController)
        {
            customController = (AnimatorController)EditorGUILayout.ObjectField("Custom Controller", customController, typeof(AnimatorController), false);

            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(customController == null);
            if (GUILayout.Button("Setup Controller", GUILayout.Width(EditorGUIUtility.currentViewWidth * 0.65f))) CreateAvatarScaleAnimation(false);
            EditorGUI.EndDisabledGroup();
            helpText = "Setup Controller will create the layer, parameter, and clip for you using the selected controller in the slot above.";
        }
        else
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Create Animator Layer", GUILayout.Width(EditorGUIUtility.currentViewWidth * 0.65f))) CreateAvatarScaleAnimation(false);
            helpText = "Create Animator Layer will create the layer, parameter, and clip for you using the selected controller in your CVRAvatar overrides slot.";
        }

        if (GUILayout.Button("Export Clip", GUILayout.Width(EditorGUIUtility.currentViewWidth * 0.35f))) CreateAvatarScaleAnimation(true);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.HelpBox(helpText, MessageType.Info);
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

        GUIStyle box = GUI.skin.GetStyle("box");
        using (new GUILayout.VerticalScope(box))
        {
            float heightPercentage = Mathf.InverseLerp(minimumHeight, maximumHeight, initialHeight);
            EditorGUILayout.LabelField("Initial Height Percentage:", $"{heightPercentage:F2}%");

            EditorGUILayout.LabelField("Viewpoint Height:", $"{initialHeight}m (x{heightRatio:F2})");
            EditorGUILayout.LabelField("Minimum Height:", $"{minimumHeight}m (x{minSpeed:F2})");
            EditorGUILayout.LabelField("Maximum Height:", $"{maximumHeight}m (x{maxSpeed:F2})");
        }

        string locomotionSpeedInfo = "The right-hand number is the ratio of the reference avatar height to the set minimum and maximum heights. It's used to adjust the speed of locomotion animations to match the avatar's scale.";
        EditorGUILayout.HelpBox(locomotionSpeedInfo, MessageType.Info);
    }

    private void DrawSettingsField()
    {
        useCustomController = EditorGUILayout.Toggle("Use Custom Controller", useCustomController);
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

        string text = $"{heightText}\n{height}m";
        Vector3 pos = new Vector3(-0.5f, height + 0.05f, 0f);
        Handles.Label(pos, text, style);
    }

    private void AnimAvatarRoot(ref AnimationClip clip, float initialToMinHeightRatio, float initialToMaxHeightRatio)
    {
        // Animate modifiers for X, Y, and Z scale of avatar root
        Vector3 initialScale = cvrAvatar.transform.localScale;
        Vector3 minScale = initialScale * initialToMinHeightRatio;
        Vector3 maxScale = initialScale * initialToMaxHeightRatio;

        AnimateVector3Property(ref clip, cvrAvatar.transform, "localScale", minScale, maxScale);
    }

    private void AnimMotionScale(ref AnimationClip clip)
    {
        if (!motionScaleFloat) return;
        // Animate modifier for locomotion animation speed float
        Animator animator = cvrAvatar.GetComponent<Animator>();
        float minLocoSpeed = AvatarScaleTool.referenceAvatarHeight / minimumHeight;
        float maxLocoSpeed = AvatarScaleTool.referenceAvatarHeight / maximumHeight;

        AnimateFloatProperty(ref clip, "", animator, "#MotionScale", minLocoSpeed, maxLocoSpeed);
    }

    private void AnimDynamicBones(ref AnimationClip clip, float initialToMinHeightRatio, float initialToMaxHeightRatio)
    {
        if (!scaleDynamicBone) return;
        Type boneType = GetTypeFromName("DynamicBone");
        if (boneType != null)
        {
            // Get all dynamic bones, including disabled ones
            var dynamicBones = cvrAvatar.gameObject.GetComponentsInChildren(boneType, true); 
            foreach (var dynamicBone in dynamicBones)
            {
                // Get force and gravity properties using reflection
                PropertyInfo forceProp = boneType.GetProperty("m_Force");
                PropertyInfo gravityProp = boneType.GetProperty("m_Gravity");

                if (forceProp != null && gravityProp != null)
                {
                    // Get force and gravity values
                    Vector3 force = (Vector3)forceProp.GetValue(dynamicBone);
                    Vector3 gravity = (Vector3)gravityProp.GetValue(dynamicBone);

                    // Animate force for dynamic bones
                    Vector3 minForce = force * initialToMinHeightRatio;
                    Vector3 maxForce = force * initialToMaxHeightRatio;
                    AnimateVector3Property(ref clip, dynamicBone, "m_Force", minForce, maxForce);

                    // Animate gravity for dynamic bones
                    Vector3 minGravity = gravity * initialToMinHeightRatio;
                    Vector3 maxGravity = gravity * initialToMaxHeightRatio;
                    AnimateVector3Property(ref clip, dynamicBone, "m_Gravity", minGravity, maxGravity);
                }
                else
                {
                    Debug.LogError("<color=blue>PumkinsAvatarTools</color>: Failed to access force or gravity properties of DynamicBone.");
                }
            }
        }
    }

    private void AnimAudioSourceRange(ref AnimationClip clip, float initialToMinHeightRatio, float initialToMaxHeightRatio)
    {
        if (!scaleAudioSources) return;
        // Get all audio sources, including disabled ones
        var audioSources = cvrAvatar.gameObject.GetComponentsInChildren<AudioSource>(true); 
        foreach (var audioSource in audioSources)
        {
            // Calculate min and max range for audio sources
            float minMinRange = audioSource.minDistance * initialToMinHeightRatio;
            float maxMinRange = audioSource.minDistance * initialToMaxHeightRatio;
            // max distance
            float minMaxRange = audioSource.maxDistance * initialToMinHeightRatio;
            float maxMaxRange = audioSource.maxDistance * initialToMaxHeightRatio;

            // Animate the range of the audio source
            string path = AnimationUtility.CalculateTransformPath(audioSource.transform, cvrAvatar.transform);
            AnimateFloatProperty(ref clip, path, audioSource, "MinDistance", minMinRange, maxMinRange);
            AnimateFloatProperty(ref clip, path, audioSource, "MaxDistance", minMaxRange, maxMaxRange);
        }
    }

    private void AnimateVector3Property(ref AnimationClip clip, Component target, string propertyName, Vector3 minValue, Vector3 maxValue)
    {
        string path = AnimationUtility.CalculateTransformPath(target.transform, cvrAvatar.transform);
        AnimateFloatProperty(ref clip, path, target, $"{propertyName}.x", minValue.x, maxValue.x);
        AnimateFloatProperty(ref clip, path, target, $"{propertyName}.y", minValue.y, maxValue.y);
        AnimateFloatProperty(ref clip, path, target, $"{propertyName}.z", minValue.z, maxValue.z);
    }

    private void AnimateFloatProperty(ref AnimationClip clip, string path, Component target, string propertyName, float minValue, float maxValue)
    {
        Keyframe[] keyframes = new Keyframe[]
        {
            new Keyframe(0f, minValue, 0.0f, 0.0f),
            new Keyframe(0.0333333333333333f, maxValue, 0.0f, 0.0f),
        };

        AnimationCurve curve = new AnimationCurve(keyframes);
        clip.SetCurve(path, target.GetType(), propertyName, curve);
    }

    private AnimationClip SaveAnimationClip(ref AnimationClip clip, string savePath = "")
    {
        string clipName = $"AvatarScale_{cvrAvatar.name}";

        if (string.IsNullOrEmpty(savePath))
        {
            savePath = EditorUtility.SaveFilePanelInProject("Save Animation Clip", clipName, "anim",
            "Please enter a file name to save the animation clip to.");
        }
        else
        {
            savePath = savePath + "/" + clipName + ".anim";
        }

        if (!string.IsNullOrEmpty(savePath))
        {
            // Check if an animation clip already exists at savePath
            AnimationClip existingClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(savePath);
            if (existingClip != null)
            {
                // Keep the GUID of the existing animation clip
                string guid = AssetDatabase.AssetPathToGUID(savePath);

                // Edit the existing animation clip
                EditorUtility.CopySerialized(clip, existingClip);
                AssetDatabase.ImportAsset(savePath);
                AssetDatabase.Refresh();
                AssetDatabase.AssetPathToGUID(savePath);
                Debug.Log($"Avatar scale animation clip edited and saved to {savePath} with the same GUID {guid}");
            }
            else
            {
                // Create a new animation clip
                AssetDatabase.CreateAsset(clip, savePath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log($"Avatar scale animation clip created and saved to {savePath}");
            }
        }

        return AssetDatabase.LoadAssetAtPath<AnimationClip>(savePath);
    }

    private void CreateAvatarScaleAnimation(bool isExport)
    {
        AnimationClip clip = new AnimationClip();

        float initialToMinHeightRatio = minimumHeight / initialHeight;
        float initialToMaxHeightRatio = maximumHeight / initialHeight;

        AnimAvatarRoot(ref clip, initialToMinHeightRatio, initialToMaxHeightRatio);
        AnimMotionScale(ref clip);

        AnimDynamicBones(ref clip, initialToMinHeightRatio, initialToMaxHeightRatio);
        AnimAudioSourceRange(ref clip, initialToMinHeightRatio, initialToMaxHeightRatio);

        if (isExport)
        {
            EditorGUIUtility.PingObject(SaveAnimationClip(ref clip));
            return;
        }

        AnimatorController controller = customController;
        if (!useCustomController)
        {
            controller = (AnimatorController)cvrAvatar.overrides.runtimeAnimatorController;
        }

        string savePath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(cvrAvatar.overrides));
        AnimationClip savedClip = SaveAnimationClip(ref clip, savePath);
        SetupAnimationController(ref savedClip, ref controller);
        EditorGUIUtility.PingObject(savedClip);
    }

    private void SetupAnimationController(ref AnimationClip clip, ref AnimatorController controller)
    {
        AnimatorControllerLayer layer = controller.layers.FirstOrDefault(l => l.name == "AvatarScale");

        // Create the layer and state machine if it doesn't exist
        if (layer == null)
        {
            layer = new AnimatorControllerLayer
            {
                name = "AvatarScale",
                blendingMode = AnimatorLayerBlendingMode.Override
            };

            AnimatorStateMachine stateMachine = new AnimatorStateMachine
            {
                name = "AvatarScale"
            };
            AssetDatabase.AddObjectToAsset(stateMachine, controller);
            layer.stateMachine = stateMachine;
            layer.defaultWeight = 1f;

            // Add the created layer to the controller
            var layers = new List<AnimatorControllerLayer>(controller.layers) { layer };
            controller.layers = layers.ToArray();
        }

        // Create the state inside the layer if it doesn't exist
        AnimatorState state = layer.stateMachine.states.FirstOrDefault(s => s.state.name == "AvatarScale").state;
        if (state == null)
        {
            state = layer.stateMachine.AddState("AvatarScale", new Vector3(200, 0, 0));
            state.writeDefaultValues = false;
        }

        // Always set the animation clip into the motion field
        state.motion = clip;

        // Check if a parameter called AvatarScale already exists
        AnimatorControllerParameter parameter = controller.parameters.FirstOrDefault(p => p.name == "AvatarScale");

        // Create the parameter if it doesn't exist
        if (parameter == null)
        {
            parameter = new AnimatorControllerParameter
            {
                name = "AvatarScale",
                type = AnimatorControllerParameterType.Float
            };

            // Add the parameter to the controller
            var parameters = new List<AnimatorControllerParameter>(controller.parameters) { parameter };
            controller.parameters = parameters.ToArray();
        }

        // Ensure the time settings are set
        state.timeParameterActive = true;
        state.timeParameter = parameter.name;
    }

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

    //Nicked from PumkinTools
    public static Type GetTypeFromName(string typeName)
    {
        var type = Type.GetType(typeName);
        if(type != null)
            return type;
        foreach(var a in AppDomain.CurrentDomain.GetAssemblies())
        {
            type = a.GetType(typeName);
            if(type != null)
                return type;
        }
        return null;
    }
}

#endif
