using UnityEditor;
using UnityEngine;
using Emergentz;
using System.Reflection;

public static class WeaponAimCheck
{
    [MenuItem("EMERGENTZ/Test Rifle Alignment (Play Mode)")]
    static void Check()
    {
        if (!EditorApplication.isPlaying) return;
        var player = PrototypeGameManager.Instance.Survivor;
        var weapon = player.GetComponent<WeaponAim>();
        if (weapon == null || weapon.Muzzle == null) throw new System.Exception("Missing rifle muzzle");
        var controller = player.GetComponent<CharacterController>();
        Vector3 old = player.transform.position;
        var camera = Camera.main;
        try
        {
            for (int i = 0; i < 4; i++)
            {
                Vector3 origin = new Vector3(i * 25, 100, 0);
                Vector3 forward = Quaternion.Euler(0,i * 90,0) * Vector3.forward;
                controller.enabled = false; player.transform.position = origin; controller.enabled = true;
                camera.transform.SetPositionAndRotation(origin - forward * 5, Quaternion.LookRotation(forward));
                var target = new GameObject("Rifle check target"); target.transform.position = origin + forward * 8;
                target.AddComponent<CharacterController>(); var zombie = target.AddComponent<ZombieAgent>(); zombie.Configure(player,1);
                int kills = PrototypeGameManager.Instance.Kills;
                Physics.SyncTransforms();
                Fire(player);
                Require(Vector3.Distance(player.LastShotStart, weapon.Muzzle.position) < 0.001f, "Tracer begins at barrel");
                Require(Vector3.Dot((player.LastShotEnd-player.LastShotStart).normalized, weapon.BarrelDirection) > 0.995f, "Barrel points along tracer");
                Require(Vector3.Dot(player.transform.forward, forward) > 0.999f, "Character faces camera aim");
                Fire(player);
                Require(PrototypeGameManager.Instance.Kills == kills+1, "Target killed in direction " + i);
                Object.DestroyImmediate(target);
            }
            // Obstacle on the barrel path must stop the shot even when camera can see over it.
            controller.enabled = false; player.transform.position = new Vector3(0,100,0); controller.enabled = true;
            camera.transform.SetPositionAndRotation(new Vector3(0,100,-5), Quaternion.identity);
            var target2 = new GameObject("Occlusion target"); target2.transform.position = new Vector3(0,100,8);
            target2.AddComponent<CharacterController>(); var z = target2.AddComponent<ZombieAgent>(); z.Configure(player,1);
            Physics.SyncTransforms(); Fire(player);
            Vector3 middle = Vector3.Lerp(player.LastShotStart, player.LastShotEnd, 0.2f);
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube); wall.name = "Rifle occlusion check"; wall.transform.position = middle; wall.transform.localScale = Vector3.one * 0.25f;
            Physics.SyncTransforms(); int before = PrototypeGameManager.Instance.Kills; Fire(player);
            Require(PrototypeGameManager.Instance.Kills == before, "Barrel obstruction blocks damage");
            Require(Vector3.Distance(player.LastShotEnd, middle) < 0.3f, "Tracer stops at obstruction");
            Object.DestroyImmediate(wall); Object.DestroyImmediate(target2);
            Debug.Log("RIFLE AIM PASS: 4 aim directions, muzzle origin, barrel/tracer alignment, target damage and obstruction.");
        }
        finally { controller.enabled = false; player.transform.position = old; controller.enabled = true; }
    }
    static void Fire(SurvivorController player)
    {
        typeof(SurvivorController).GetField("nextShot",BindingFlags.NonPublic|BindingFlags.Instance).SetValue(player,0f);
        Require(player.TryFire(),"Shot accepted");
    }
    static void Require(bool ok,string message) { if (!ok) throw new System.Exception("RIFLE AIM FAIL: " + message); }
}
