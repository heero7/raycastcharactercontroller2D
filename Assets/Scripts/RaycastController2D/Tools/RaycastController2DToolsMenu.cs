using UnityEditor;
using UnityEngine;

namespace RaycastController2D.Tools
{
    public static class RaycastController2DToolsMenu
    {
        private const string TAGS_PROPERTY_NAME = "tags";
        
        private const string THROUGH_TAG = "ThroughPlatform";

        private const string TAG_MANAGER_ASSET_PATH = "ProjectSettings/TagManager.asset";
        
        
        [MenuItem("Tools/RaycastController2D/Create Necessary Game Tags")]
        private static void CreateNecessaryGameTags()
        {
            var tagManager =
                new SerializedObject(AssetDatabase.LoadAllAssetsAtPath(TAG_MANAGER_ASSET_PATH)[0]);
            var tagsProperty = tagManager.FindProperty(TAGS_PROPERTY_NAME);
            
            var found = false;
            for (var i = 0; i < tagsProperty.arraySize; i++)
            {
                var tagProperty = tagsProperty.GetArrayElementAtIndex(i);
                if (!tagProperty.stringValue.Equals(RaycastControllerConstants.ThroughTag)) continue;
                found = true;
                break;
            }

            if (found) return;
            
            tagsProperty.InsertArrayElementAtIndex(0);
            var newTagProperty = tagsProperty.GetArrayElementAtIndex(0);
            newTagProperty.stringValue = THROUGH_TAG;
            
            tagManager.ApplyModifiedProperties();
        }

        [MenuItem("Tools/RaycastController2D/Set Project Settings")]
        private static void EnableRaycastControllerProjectSettings()
        {
            if (!Physics2D.autoSyncTransforms)
            {
                Physics2D.autoSyncTransforms = true;
            }

            if (!Physics2D.reuseCollisionCallbacks)
            {
                Physics2D.reuseCollisionCallbacks = true;
            }
        }
    }
}
