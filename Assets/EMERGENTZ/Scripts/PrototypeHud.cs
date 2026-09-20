using UnityEngine;

namespace Emergentz
{
    public sealed class PrototypeHud : MonoBehaviour
    {
        GUIStyle title;
        GUIStyle body;
        GUIStyle center;

        void EnsureStyles()
        {
            if (title != null) return;
            title = new GUIStyle(GUI.skin.label) { fontSize = 25, fontStyle = FontStyle.Bold, normal = { textColor = new Color(1f, 0.78f, 0.2f) } };
            body = new GUIStyle(GUI.skin.label) { fontSize = 18, fontStyle = FontStyle.Bold, normal = { textColor = Color.white } };
            center = new GUIStyle(body) { alignment = TextAnchor.MiddleCenter, fontSize = 28 };
        }

        void OnGUI()
        {
            EnsureStyles();
            PrototypeGameManager game = PrototypeGameManager.Instance;
            if (game == null || game.Survivor == null || game.Infection == null) return;

            GUI.Box(new Rect(18, 18, 345, 142), string.Empty);
            GUI.Label(new Rect(34, 28, 320, 32), "EMERGENTZ", title);
            GUI.Label(new Rect(34, 62, 320, 28), $"INFECTION: {game.Infection.RemainingSeconds:0.0}s", body);
            GUI.Label(new Rect(34, 88, 320, 28), $"HEALTH: {game.Survivor.Health:0}   KILLS: {game.Kills}", body);
            GUI.Label(new Rect(34, 114, 320, 28), $"WAVE: {game.Wave}   SURVIVED: {game.RunTime:0}s", body);

            GUI.Label(new Rect(Screen.width / 2f - 14, Screen.height / 2f - 20, 28, 40), "+", center);
            GUI.Label(new Rect(Screen.width - 330, 25, 310, 90), "WASD move  •  Mouse aim\nLeft click shoot  •  R restart", body);

            if (!string.IsNullOrEmpty(game.Message))
                GUI.Label(new Rect(0, Screen.height * 0.18f, Screen.width, 50), game.Message, center);

            if (game.RunEnded)
            {
                GUI.Box(new Rect(Screen.width / 2f - 260, Screen.height / 2f - 110, 520, 220), string.Empty);
                GUI.Label(new Rect(Screen.width / 2f - 240, Screen.height / 2f - 82, 480, 55), game.EndReason, center);
                GUI.Label(new Rect(Screen.width / 2f - 240, Screen.height / 2f - 20, 480, 45), $"{game.Kills} KILLS  •  {game.RunTime:0.0} SECONDS", center);
                GUI.Label(new Rect(Screen.width / 2f - 240, Screen.height / 2f + 45, 480, 38), "PRESS R TO BEGIN AGAIN", center);
            }
        }
    }
}
