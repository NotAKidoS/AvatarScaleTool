#if CVR_CCK_EXISTS
using ABI.CCK.Components;
using ABI.CCK.Scripts;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Animations;
using UnityEngine;

namespace NAK.EditorTools
{
    public static class AvatarScaleTool
    {
        public static float referenceAvatarHeight = 1.8f;
        public static float locomotionSpeedModifier = 1f;

        public static CVRAvatar cvrAvatar;
        public static AnimatorController customController;
        public static float initialHeight = 1.0f;
        public static float minimumHeight = 0.5f;
        public static float maximumHeight = 2.0f;
        public static bool motionScaleFloat = true;
        public static bool scaleDynamicBone = false;
        public static bool scaleAudioSources = true;
        public static bool splitAnimationClip = false;
        public static bool useGlobalScaleSettings = false;

        public static bool useCustomController;
        public static bool showGizmos;

        public static void AddToAvatarAdvancedSettings()
        {
            List<AnimationClip> clips = AnimationGenerator.CreateAASClips();
            cvrAvatar.avatarUsesAdvancedSettings = true;
            string settingName = "AvatarScale";
            float defaultValue = Mathf.InverseLerp(minimumHeight, maximumHeight, initialHeight);
            defaultValue = Mathf.Round(defaultValue * 100f) / 100f;

            // Try to find an existing slider setting with the name or machine name of "AvatarScale"
            CVRAdvancedSettingsEntry existingEntry = cvrAvatar.avatarSettings.settings
                .FirstOrDefault(x => x.name == settingName || x.machineName == settingName);

            CVRAdvancesAvatarSettingSlider existingSliderSetting = null;

            if (existingEntry != null && existingEntry.type == CVRAdvancedSettingsEntry.SettingsType.Slider)
            {
                existingSliderSetting = (CVRAdvancesAvatarSettingSlider)existingEntry.setting;
                existingSliderSetting.defaultValue = defaultValue;
                existingSliderSetting.useAnimationClip = true;
                existingSliderSetting.minAnimationClip = clips[0];
                existingSliderSetting.maxAnimationClip = clips[1];
            }
            else
            {
                // If the slider setting does not exist, create a new one
                existingSliderSetting = new CVRAdvancesAvatarSettingSlider();
                existingSliderSetting.defaultValue = defaultValue;
                existingSliderSetting.useAnimationClip = true;
                existingSliderSetting.minAnimationClip = clips[0];
                existingSliderSetting.maxAnimationClip = clips[1];

                // Create a new advanced settings entry and add it to the avatar settings
                CVRAdvancedSettingsEntry newEntry = new CVRAdvancedSettingsEntry();
                newEntry.name = settingName;
                newEntry.machineName = settingName;
                newEntry.type = CVRAdvancedSettingsEntry.SettingsType.Slider;
                newEntry.setting = existingSliderSetting;

                cvrAvatar.avatarSettings.settings.Add(newEntry);
            }
        }

        public static void SetupCustomController()
        {
            //TODO
        }

        public static void ExportClips()
        {
            AnimationGenerator.ExportAnimationClip();
        }
    }
}
#endif