using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static NAK.EditorTools.AvatarScaleTool;
using Object = UnityEngine.Object;

namespace NAK.EditorTools
{
    internal static class AnimationGenerator
    {
        public static List<AnimationClip> CreateAASClips()
        {
            AnimationClip clip = CreateAnimationClip();
            return SaveAASClips(clip);
        }

        public static void ExportAnimationClip()
        {
            AnimationClip clip = CreateAnimationClip();
            SaveAsAnimationClip(clip);
        }

        private static AnimationClip CreateAnimationClip()
        {
            AnimationClip clip = new AnimationClip();

            float initialToMinHeightRatio = GetMinimumHeight() / initialHeight;
            float initialToMaxHeightRatio = GetMaximumHeight() / initialHeight;

            AnimAvatarRoot(ref clip, initialToMinHeightRatio, initialToMaxHeightRatio);

            //Optional Settings
            AnimMotionScale(ref clip);
            AnimAudioSourceRange(ref clip, initialToMinHeightRatio, initialToMaxHeightRatio);

            return clip;
        }

        private static List<AnimationClip> SaveAASClips(AnimationClip clip)
        {
            string folderPath = $"Assets/AdvancedSettings.Generated/{cvrAvatar.name}_AAS/";

            // Create the folder path if it doesn't exist
            string[] folders = folderPath.Split('/');
            string path = "Assets";
            foreach (string folder in folders)
            {
                if (!string.IsNullOrEmpty(folder))
                {
                    string folderPathCandidate = path + "/" + folder;
                    if (!AssetDatabase.IsValidFolder(folderPathCandidate))
                    {
                        AssetDatabase.CreateFolder(path, folder);
                    }
                    path = folderPathCandidate;
                }
            }

            // Create two clips
            AnimationClip clip0 = Object.Instantiate(clip);
            clip0.name = "Anim_AvatarScale_Slider_Min";
            clip0.ClearCurves();

            AnimationClip clip1 = Object.Instantiate(clip);
            clip1.name = "Anim_AvatarScale_Slider_Max";
            clip1.ClearCurves();

            // Set the curves for clip0
            AnimationUtility.GetCurveBindings(clip).ToList().ForEach(binding =>
            {
                AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, binding);
                curve.RemoveKey(1);
                AnimationUtility.SetEditorCurve(clip0, binding, curve);
            });

            // Set the curves for clip1
            AnimationUtility.GetCurveBindings(clip).ToList().ForEach(binding =>
            {
                AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, binding);
                curve.RemoveKey(0);
                AnimationUtility.SetEditorCurve(clip1, binding, curve);
            });

            // Save clip0
            string savePath0 = $"{folderPath}/Anim_AvatarScale_Slider_Min.anim";
            AssetDatabase.CreateAsset(clip0, savePath0);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Avatar scale animation clip 0 created and saved to {savePath0}");

            // Ping the saved clip0
            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<AnimationClip>(savePath0));

            // Save clip1
            string savePath1 = $"{folderPath}/Anim_AvatarScale_Slider_Max.anim";
            AssetDatabase.CreateAsset(clip1, savePath1);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Avatar scale animation clip 1 created and saved to {savePath1}");

            // Ping the saved clip1
            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<AnimationClip>(savePath1));

            // Return the generated clips as a list
            return new List<AnimationClip>() { clip0, clip1 };
        }

        private static void SaveAsAnimationClip(AnimationClip clip)
        {
            string clipName = $"AvatarScale_{cvrAvatar.name}";
            string savePath = EditorUtility.SaveFilePanelInProject("Save Animation Clip", clipName, "anim",
                "Please enter a file name to save the animation clip to.");

            if (!string.IsNullOrEmpty(savePath))
            {
                // Check if the file extension is .anim
                if (!savePath.EndsWith(".anim"))
                {
                    savePath += ".anim";
                }

                if (splitAnimationClip)
                {
                    // Create two clips
                    AnimationClip clip0 = Object.Instantiate(clip);
                    clip0.name = clipName + "_0";
                    clip0.ClearCurves();

                    AnimationClip clip1 = Object.Instantiate(clip);
                    clip1.name = clipName + "_1";
                    clip1.ClearCurves();

                    // Set the curves for clip0
                    AnimationUtility.GetCurveBindings(clip).ToList().ForEach(binding =>
                    {
                        AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, binding);
                        curve.RemoveKey(1);
                        AnimationUtility.SetEditorCurve(clip0, binding, curve);
                    });

                    // Set the curves for clip1
                    AnimationUtility.GetCurveBindings(clip).ToList().ForEach(binding =>
                    {
                        AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, binding);
                        curve.RemoveKey(0);
                        AnimationUtility.SetEditorCurve(clip1, binding, curve);
                    });

                    // Create the assets for the two clips
                    AssetDatabase.CreateAsset(clip0, savePath.Replace(".anim", "_0.anim"));
                    AssetDatabase.CreateAsset(clip1, savePath.Replace(".anim", "_1.anim"));
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    Debug.Log($"Avatar scale animation clips created and saved to {savePath}");

                    // Ping the saved animation clips
                    EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<AnimationClip>(savePath.Replace(".anim", "_0.anim")));
                    EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<AnimationClip>(savePath.Replace(".anim", "_1.anim")));
                }
                else
                {
                    // Create a single clip
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

                    // Ping the saved animation clip
                    EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<AnimationClip>(savePath));
                }
            }
        }

        private static void AnimAvatarRoot(ref AnimationClip clip, float initialToMinHeightRatio, float initialToMaxHeightRatio)
        {
            // Animate modifiers for X, Y, and Z scale of avatar root
            Vector3 initialScale = cvrAvatar.transform.localScale;
            Vector3 minScale = initialScale * initialToMinHeightRatio;
            Vector3 maxScale = initialScale * initialToMaxHeightRatio;

            AnimateVector3Property(ref clip, cvrAvatar.transform, "localScale", minScale, maxScale);
        }

        private static void AnimMotionScale(ref AnimationClip clip)
        {
            if (!motionScaleFloat) return;

            // Animate modifier for locomotion animation speed float
            Animator animator = cvrAvatar.GetComponent<Animator>();
            float minLocoSpeed = AvatarScaleTool.referenceAvatarHeight / GetMinimumHeight();
            float maxLocoSpeed = AvatarScaleTool.referenceAvatarHeight / GetMaximumHeight();

            AnimateFloatProperty(ref clip, "", animator, "#MotionScale", minLocoSpeed, maxLocoSpeed);
        }

        private static void AnimAudioSourceRange(ref AnimationClip clip, float initialToMinHeightRatio, float initialToMaxHeightRatio)
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

        private static float GetMinimumHeight()
        {
            return useGlobalScaleSettings ? 0.25f : minimumHeight;
        }

        private static float GetMaximumHeight()
        {
            return useGlobalScaleSettings ? 2f : maximumHeight;
        }

        private static void AnimateVector3Property(ref AnimationClip clip, Component target, string propertyName, Vector3 minValue, Vector3 maxValue)
        {
            string path = AnimationUtility.CalculateTransformPath(target.transform, cvrAvatar.transform);
            AnimateFloatProperty(ref clip, path, target, $"{propertyName}.x", minValue.x, maxValue.x);
            AnimateFloatProperty(ref clip, path, target, $"{propertyName}.y", minValue.y, maxValue.y);
            AnimateFloatProperty(ref clip, path, target, $"{propertyName}.z", minValue.z, maxValue.z);
        }

        private static void AnimateFloatProperty(ref AnimationClip clip, string path, Component target, string propertyName, float minValue, float maxValue)
        {
            float duration = 0.0333333333333333f;
            Keyframe keyframe1 = new Keyframe(0f, minValue);
            Keyframe keyframe2 = new Keyframe(duration, maxValue);

            // Calculate the linear tangents
            float deltaValue = maxValue - minValue;
            float deltaTime = duration;
            float tangent = deltaValue / deltaTime;

            // Set the linear tangents
            keyframe1.outTangent = tangent;
            keyframe2.inTangent = tangent;

            Keyframe[] keyframes = new Keyframe[] { keyframe1, keyframe2 };

            AnimationCurve curve = new AnimationCurve(keyframes);
            clip.SetCurve(path, target.GetType(), propertyName, curve);
        }
    }
}