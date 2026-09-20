using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using Emergentz;

public static class CharacterUpgrade
{
    [MenuItem("EMERGENTZ/Import Characters and Weapons")]
    public static void Upgrade()
    {
        if (EditorApplication.isPlaying) { Debug.LogError("Stop Play Mode first."); return; }
        if (!AssetDatabase.IsValidFolder("Assets/TextMesh Pro")) TMPro.TMP_PackageResourceImporter.ImportResources(true, false, false);
        var player = Object.FindFirstObjectByType<SurvivorController>();
        var manager = Object.FindFirstObjectByType<PrototypeGameManager>();
        if (player == null || manager == null) return;
        foreach (var root in player.gameObject.scene.GetRootGameObjects())
            if (root.name == "Survivor" && root.GetComponent<SurvivorController>() == null) Object.DestroyImmediate(root);
        const string folder = "Assets/EMERGENTZ/Characters";
        if (!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder("Assets/EMERGENTZ", "Characters");
        var material = AssetDatabase.LoadAssetAtPath<Material>(folder + "/Characters.mat");
        if (material == null) { material = new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(material, folder + "/Characters.mat"); }
        material.mainTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/ThirdParty/Quaternius/Zombie_Atlas.png");
        var survivor = MakeVisual("Survivor", "Idle_Gun", "Run_Gun", material, folder);
        var zombie = MakeVisual("Zombie", "Idle", "Run_Arms", material, folder);
        var existing = player.transform.Find("Survivor Visual");
        if (existing == null)
        {
            var model = (GameObject)PrefabUtility.InstantiatePrefab(survivor, player.transform);
            model.name = "Survivor Visual"; model.transform.localPosition = Vector3.down;
        }
        player.GetComponent<Renderer>().enabled = false;
        foreach (var r in player.GetComponentsInChildren<Renderer>(true))
            if (r.transform != player.transform) r.enabled = r.name == "Matt" || r.name == "Rifle";
        if (player.transform.Find("Weapon") == null)
        {
            var gun = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ThirdParty/KenneyBlasters/Models/FBX format/blaster-a.fbx");
            var weapon = (GameObject)PrefabUtility.InstantiatePrefab(gun, player.transform);
            weapon.name = "Weapon";
            weapon.transform.localPosition = new Vector3(0.4f, 0.4f, 0.5f);
            weapon.transform.localScale = Vector3.one * 1.5f;
            foreach (var renderer in weapon.GetComponentsInChildren<Renderer>()) renderer.sharedMaterial = material;
        }
        player.transform.Find("Weapon").gameObject.SetActive(false);
        var so = new SerializedObject(manager);
        so.FindProperty("zombieVisual").objectReferenceValue = zombie; so.ApplyModifiedProperties();
        foreach (var camp in Object.FindObjectsByType<CampfireCheckpoint>(FindObjectsSortMode.None))
        {
            if (camp.transform.Find("Safe perimeter") != null) continue;
            var ring = new GameObject("Safe perimeter"); ring.transform.SetParent(camp.transform, false);
            var line = ring.AddComponent<LineRenderer>(); line.useWorldSpace = false;
            line.sharedMaterial = material; line.startWidth = line.endWidth = 0.12f;
            line.loop = true; line.positionCount = 64;
            for (int i = 0; i < 64; i++) { float a = i * Mathf.PI * 2 / 64; line.SetPosition(i, new Vector3(Mathf.Cos(a) * 5, 0.1f, Mathf.Sin(a) * 5)); }
        }
        EditorSceneManager.MarkSceneDirty(player.gameObject.scene);
        EditorSceneManager.SaveScene(player.gameObject.scene);
        AssetDatabase.SaveAssets();
        Debug.Log("EMERGENTZ characters, animations, weapon and safe perimeter installed.");
    }
    static GameObject MakeVisual(string name, string idleName, string runName, Material material, string folder)
    {
        string source = "Assets/ThirdParty/Quaternius/" + name + ".fbx";
        var importer = (ModelImporter)AssetImporter.GetAtPath(source);
        importer.animationType = ModelImporterAnimationType.Generic;
        var clips = importer.defaultClipAnimations;
        foreach (var clip in clips) clip.loopTime = true;
        importer.clipAnimations = clips; importer.SaveAndReimport();
        var animations = AssetDatabase.LoadAllAssetsAtPath(source).OfType<AnimationClip>().ToArray();
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(folder + "/" + name + ".controller");
        if (controller == null)
        {
            controller = AnimatorController.CreateAnimatorControllerAtPath(folder + "/" + name + ".controller");
            controller.AddParameter("Moving", AnimatorControllerParameterType.Bool);
            var sm = controller.layers[0].stateMachine;
            var idle = sm.AddState("Idle"); idle.motion = animations.First(x => x.name.EndsWith("|" + idleName) || x.name == idleName);
            var run = sm.AddState("Run"); run.motion = animations.First(x => x.name.EndsWith("|" + runName) || x.name == runName);
            var a = idle.AddTransition(run); a.hasExitTime = false; a.duration = 0.12f; a.AddCondition(AnimatorConditionMode.If, 0, "Moving");
            var b = run.AddTransition(idle); b.hasExitTime = false; b.duration = 0.12f; b.AddCondition(AnimatorConditionMode.IfNot, 0, "Moving");
        }
        var instance = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(source));
        instance.name = name;
        var animator = instance.GetComponent<Animator>();
        if (animator == null) animator = instance.AddComponent<Animator>();
        animator.runtimeAnimatorController = controller; animator.applyRootMotion = false;
        instance.AddComponent<CharacterMotion>();
        foreach (var renderer in instance.GetComponentsInChildren<Renderer>())
        {
            renderer.sharedMaterial = material;
            if (name == "Survivor") renderer.enabled = renderer.name == "Matt" || renderer.name == "Rifle";
        }
        var prefab = PrefabUtility.SaveAsPrefabAsset(instance, folder + "/" + name + ".prefab");
        Object.DestroyImmediate(instance);
        return prefab;
    }
}
