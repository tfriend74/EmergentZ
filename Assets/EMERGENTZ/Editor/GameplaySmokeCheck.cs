using UnityEditor;
using UnityEngine;
using Emergentz;
using System.Reflection;

public static class GameplaySmokeCheck
{
    [MenuItem("EMERGENTZ/Test Combat and Safe Camp (Play Mode)")]
    static void Check()
    {
        if (!EditorApplication.isPlaying) { Debug.LogError("Start a fresh run first."); return; }
        var game = PrototypeGameManager.Instance;
        var player = game.Survivor;
        var camera = Camera.main;
        player.GetComponent<CharacterController>().enabled = false;
        player.transform.position = new Vector3(0,100,0);
        player.GetComponent<CharacterController>().enabled = true;
        camera.transform.SetPositionAndRotation(new Vector3(0,100,-5), Quaternion.identity);
        var body = new GameObject("Smoke test zombie");
        body.transform.position = new Vector3(0,100,5);
        body.AddComponent<CharacterController>();
        var zombie = body.AddComponent<ZombieAgent>(); zombie.Configure(player,1);
        Physics.SyncTransforms();
        int kills = game.Kills;
        float infection = game.Infection.RemainingSeconds;
        Require(player.TryFire(), "Shot accepted");
        typeof(SurvivorController).GetField("nextShot", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(player,0f);
        Require(player.TryFire(), "Second shot accepted");
        Require(game.Kills == kills + 1, "Shots kill target and count once");
        Require(game.Infection.RemainingSeconds > infection, "Kill restores infection time");
        var camp = Object.FindFirstObjectByType<CampfireCheckpoint>();
        player.GetComponent<CharacterController>().enabled = false;
        player.transform.position = camp.transform.position + Vector3.up;
        player.GetComponent<CharacterController>().enabled = true;
        Require(player.IsAtCamp, "Camp recognizes player without trigger callbacks");
        player.TakeDamage(500);
        Require(player.IsAlive, "Camp blocks lethal damage");
        camp.SendMessage("Update");
        Require(player.Health == player.MaxHealth, "Camp heals fully");
        infection = game.Infection.RemainingSeconds; float score = game.RunTime;
        typeof(InfectionClock).GetMethod("Update", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(game.Infection, null);
        typeof(PrototypeGameManager).GetMethod("Update", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(game, null);
        Require(game.Infection.RemainingSeconds == infection && game.RunTime == score, "Camp pauses infection and score");
        Require(!player.TryFire(), "Camp prevents safe-zone shooting");
        player.GetComponent<CharacterController>().enabled = false;
        player.transform.position = new Vector3(0,1,0);
        player.GetComponent<CharacterController>().enabled = true;
        player.TakeDamage(10);
        Require(player.Health == player.MaxHealth - 10, "Damage resumes outside camp");
        player.GetComponent<CharacterController>().enabled = true;
        Debug.Log("EMERGENTZ SMOKE PASS: shooting, kill, time reward, camp detection, damage immunity, healing, paused score/infection, shooting restriction, exit damage.");
    }
    static void Require(bool condition, string check) { if (!condition) throw new System.Exception("SMOKE FAIL: " + check); }
}
