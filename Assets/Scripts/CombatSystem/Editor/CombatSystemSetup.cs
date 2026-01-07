#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using CombatSystem;

namespace CombatSystem.Editor
{
    /// <summary>
    /// Editor utilities for setting up the combat system.
    /// </summary>
    public class CombatSystemSetup : EditorWindow
    {
        [MenuItem("Tools/Combat System/Setup Window")]
        public static void ShowWindow()
        {
            GetWindow<CombatSystemSetup>("Combat System Setup");
        }
        
        private GameObject selectedCharacter;
        private bool showAnimationRefs = true;
        private bool showSetupChecklist = true;
        
        private void OnGUI()
        {
            GUILayout.Label("Combat System Setup", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            // Character selection
            selectedCharacter = (GameObject)EditorGUILayout.ObjectField(
                "Target Character", 
                selectedCharacter, 
                typeof(GameObject), 
                true
            );
            
            EditorGUILayout.Space();
            
            // Quick actions
            GUILayout.Label("Quick Actions", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Create Upper Body Avatar Mask"))
            {
                CreateUpperBodyMask();
            }
            
            if (GUILayout.Button("Add Combat Components to Selected"))
            {
                AddCombatComponents();
            }
            
            if (GUILayout.Button("Setup Dummy Tag & Layer"))
            {
                SetupDummyTagAndLayer();
            }
            
            EditorGUILayout.Space();
            
            // Setup checklist
            showSetupChecklist = EditorGUILayout.Foldout(showSetupChecklist, "Setup Checklist");
            if (showSetupChecklist)
            {
                DrawChecklist();
            }
            
            EditorGUILayout.Space();
            
            // Animation references help
            showAnimationRefs = EditorGUILayout.Foldout(showAnimationRefs, "Animation Reference Paths");
            if (showAnimationRefs)
            {
                DrawAnimationPaths();
            }
        }
        
        private void CreateUpperBodyMask()
        {
            string path = "Assets/CombatSystem/UpperBodyMask.mask";
            
            // Ensure directory exists
            string dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            
            // Create avatar mask
            AvatarMask mask = new AvatarMask();
            
            // Disable lower body parts
            mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.Root, false);
            mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.LeftLeg, false);
            mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.RightLeg, false);
            mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.LeftFootIK, false);
            mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.RightFootIK, false);
            
            // Enable upper body parts
            mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.Body, true);
            mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.Head, true);
            mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.LeftArm, true);
            mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.RightArm, true);
            mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.LeftHandIK, true);
            mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.RightHandIK, true);
            
            AssetDatabase.CreateAsset(mask, path);
            AssetDatabase.SaveAssets();
            
            Debug.Log($"Created Upper Body Avatar Mask at: {path}");
            EditorGUIUtility.PingObject(mask);
        }
        
        private void AddCombatComponents()
        {
            if (selectedCharacter == null)
            {
                EditorUtility.DisplayDialog("No Character Selected", 
                    "Please select a character GameObject first.", "OK");
                return;
            }
            
            Undo.RegisterCompleteObjectUndo(selectedCharacter, "Add Combat Components");
            
            // Add Animancer if not present
            var animancer = selectedCharacter.GetComponent<Animancer.AnimancerComponent>();
            if (animancer == null)
            {
                animancer = selectedCharacter.AddComponent<Animancer.AnimancerComponent>();
                Debug.Log("Added AnimancerComponent");
            }
            
            // Add Combat Bridge if not present
            var combatBridge = selectedCharacter.GetComponent<CombatSystem.ThirdPersonCombatBridge>();
            if (combatBridge == null)
            {
                combatBridge = selectedCharacter.AddComponent<CombatSystem.ThirdPersonCombatBridge>();
                Debug.Log("Added ThirdPersonCombatBridge");
            }
            
            // Add Sync component
            var sync = selectedCharacter.GetComponent<CombatSystem.ThirdPersonControllerSync>();
            if (sync == null)
            {
                sync = selectedCharacter.AddComponent<CombatSystem.ThirdPersonControllerSync>();
                Debug.Log("Added ThirdPersonControllerSync");
            }
            
            // Add Combat Input
            var combatInput = selectedCharacter.GetComponent<CombatSystem.CombatInputActions>();
            if (combatInput == null)
            {
                combatInput = selectedCharacter.AddComponent<CombatSystem.CombatInputActions>();
                Debug.Log("Added CombatInputActions");
            }
            
            EditorUtility.SetDirty(selectedCharacter);
            
            Debug.Log("Combat components added successfully!");
        }
        
        private void SetupDummyTagAndLayer()
        {
            // Add Dummy tag if it doesn't exist
            SerializedObject tagManager = new SerializedObject(
                AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            
            SerializedProperty tagsProp = tagManager.FindProperty("tags");
            
            bool tagExists = false;
            for (int i = 0; i < tagsProp.arraySize; i++)
            {
                if (tagsProp.GetArrayElementAtIndex(i).stringValue == "Dummy")
                {
                    tagExists = true;
                    break;
                }
            }
            
            if (!tagExists)
            {
                tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
                tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1).stringValue = "Dummy";
                tagManager.ApplyModifiedProperties();
                Debug.Log("Added 'Dummy' tag");
            }
            else
            {
                Debug.Log("'Dummy' tag already exists");
            }
            
            // Add Dummy layer
            SerializedProperty layersProp = tagManager.FindProperty("layers");
            
            bool layerExists = false;
            int emptySlot = -1;
            
            for (int i = 8; i < layersProp.arraySize; i++)
            {
                string layerName = layersProp.GetArrayElementAtIndex(i).stringValue;
                if (layerName == "Dummy")
                {
                    layerExists = true;
                    break;
                }
                if (string.IsNullOrEmpty(layerName) && emptySlot == -1)
                {
                    emptySlot = i;
                }
            }
            
            if (!layerExists && emptySlot != -1)
            {
                layersProp.GetArrayElementAtIndex(emptySlot).stringValue = "Dummy";
                tagManager.ApplyModifiedProperties();
                Debug.Log($"Added 'Dummy' layer at slot {emptySlot}");
            }
            else if (layerExists)
            {
                Debug.Log("'Dummy' layer already exists");
            }
            else
            {
                Debug.LogWarning("No empty layer slots available!");
            }
        }
        
        private void DrawChecklist()
        {
            EditorGUI.indentLevel++;
            
            EditorGUILayout.LabelField("1. Import Animancer Lite from Asset Store", EditorStyles.miniLabel);
            EditorGUILayout.LabelField("2. Add combat components to player (button above)", EditorStyles.miniLabel);
            EditorGUILayout.LabelField("3. Create Upper Body Avatar Mask (button above)", EditorStyles.miniLabel);
            EditorGUILayout.LabelField("4. Assign animations in ThirdPersonCombatBridge", EditorStyles.miniLabel);
            EditorGUILayout.LabelField("5. Assign UpperBodyMask to ThirdPersonCombatBridge", EditorStyles.miniLabel);
            EditorGUILayout.LabelField("6. Setup Dummy tag and layer (button above)", EditorStyles.miniLabel);
            EditorGUILayout.LabelField("7. Add DummyHitReaction to training dummy", EditorStyles.miniLabel);
            EditorGUILayout.LabelField("8. Tag dummy as 'Dummy'", EditorStyles.miniLabel);
            EditorGUILayout.LabelField("9. Add combat inputs to Input Actions asset", EditorStyles.miniLabel);
            
            EditorGUI.indentLevel--;
        }
        
        private void DrawAnimationPaths()
        {
            EditorGUI.indentLevel++;
            
            EditorGUILayout.LabelField("StarterAssets Locomotion:", EditorStyles.boldLabel);
            EditorGUILayout.SelectableLabel("Assets/StarterAssets/ThirdPersonController/Character/Animations/Stand--Idle.anim.fbx");
            EditorGUILayout.SelectableLabel("Assets/StarterAssets/ThirdPersonController/Character/Animations/Locomotion--Walk_N.anim.fbx");
            EditorGUILayout.SelectableLabel("Assets/StarterAssets/ThirdPersonController/Character/Animations/Locomotion--Run_N.anim.fbx");
            
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("Your Combat Animations:", EditorStyles.boldLabel);
            EditorGUILayout.SelectableLabel("Assets/Animations/Ch44_nonPBR@Bouncing Fight Idle.fbx");
            EditorGUILayout.SelectableLabel("Assets/Animations/Ch44_nonPBR@Punching Gab.fbx");
            EditorGUILayout.SelectableLabel("Assets/Animations/Ch44_nonPBR@Punching Cross.fbx");
            EditorGUILayout.SelectableLabel("Assets/Animations/Ch44_nonPBR@Punch Combo.fbx");
            EditorGUILayout.SelectableLabel("Assets/Animations/Ch44_nonPBR@Mma Kick.fbx");
            EditorGUILayout.SelectableLabel("Assets/Animations/Ch44_nonPBR@Standing Dodge Forward.fbx");
            EditorGUILayout.SelectableLabel("Assets/Animations/Ch44_nonPBR@Standing Dodge Backward.fbx");
            EditorGUILayout.SelectableLabel("Assets/Animations/Ch44_nonPBR@Standing Dodge Left.fbx");
            EditorGUILayout.SelectableLabel("Assets/Animations/Ch44_nonPBR@Standing Dodge Right.fbx");
            EditorGUILayout.SelectableLabel("Assets/Animations/Ch44_nonPBR@Hit Reaction.fbx");
            
            EditorGUI.indentLevel--;
        }
    }
}
#endif
