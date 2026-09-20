using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Emergentz;

public static class WeaponSetup
{
    [MenuItem("EMERGENTZ/Configure Rifle Muzzle")]
    static void Configure()
    {
        if (EditorApplication.isPlaying) return;
        var player = Object.FindFirstObjectByType<SurvivorController>();
        var rifle = player.GetComponentsInChildren<MeshFilter>().First(x => x.name == "Rifle");
        var model = player.transform.Find("Survivor Visual");
        var idle = AssetDatabase.LoadAllAssetsAtPath("Assets/ThirdParty/Quaternius/Survivor.fbx").OfType<AnimationClip>().First(x => x.name.EndsWith("|Idle_Gun"));
        AnimationMode.StartAnimationMode();
        AnimationMode.BeginSampling(); AnimationMode.SampleAnimationClip(model.gameObject, idle, 0); AnimationMode.EndSampling();
        var mesh = rifle.sharedMesh;
        Vector3 size = mesh.bounds.size;
        int axis = size.x > size.y && size.x > size.z ? 0 : size.y > size.z ? 1 : 2;
        Vector3 barrel = Vector3.zero; barrel[axis] = 1;
        if (Vector3.Dot(rifle.transform.TransformDirection(barrel), player.transform.forward) < 0) barrel *= -1;
        var vertices = mesh.vertices;
        float maximum = vertices.Max(v => Vector3.Dot(v, barrel));
        var tipVertices = vertices.Where(v => Vector3.Dot(v, barrel) >= maximum - size[axis] * 0.01f).ToArray();
        Vector3 tipPosition = Vector3.zero;
        foreach (var v in tipVertices) tipPosition += v;
        tipPosition /= tipVertices.Length;
        AnimationMode.StopAnimationMode();
        var tip = rifle.transform.Find("Muzzle");
        if (tip == null) { tip = new GameObject("Muzzle").transform; tip.SetParent(rifle.transform,false); }
        tip.localPosition = tipPosition; tip.localRotation = Quaternion.LookRotation(barrel);
        var aim = player.GetComponent<WeaponAim>(); if (aim == null) aim = player.gameObject.AddComponent<WeaponAim>();
        aim.Configure(rifle.transform, tip, barrel);
        EditorUtility.SetDirty(aim);
        EditorSceneManager.MarkSceneDirty(player.gameObject.scene); EditorSceneManager.SaveScene(player.gameObject.scene);
        Debug.Log($"Rifle muzzle configured at {tipPosition}, barrel axis {barrel}; {tipVertices.Length} barrel-tip vertices.");
    }
}
