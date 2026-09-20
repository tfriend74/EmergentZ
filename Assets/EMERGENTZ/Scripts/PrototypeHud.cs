using UnityEngine;
using TMPro;

namespace Emergentz
{
    public sealed class PrototypeHud : MonoBehaviour
    {
        Canvas canvas;
        TMP_FontAsset font;
        TextMeshProUGUI health, infection, score, message, result;
        UnityEngine.UI.Image healthBar, infectionBar;

        void Start()
        {
            var root = new GameObject("Survival HUD", typeof(RectTransform), typeof(Canvas), typeof(UnityEngine.UI.CanvasScaler));
            root.transform.SetParent(transform, false);
            canvas = root.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 100;
            var scaler = root.GetComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1100, 620); scaler.matchWidthOrHeight = 0.5f;
            font = TMP_Settings.defaultFontAsset;
            var panel = Box(root.transform, "Vitals", new Vector2(0,1), new Vector2(20,-20), new Vector2(345,225), new Color(0.025f,0.035f,0.045f,0.95f));
            Label(panel.transform, "EMERGENTZ", new Vector2(15,-10), new Vector2(315,36), 28);
            health = Label(panel.transform, "HEALTH", new Vector2(15,-50), new Vector2(315,30), 23);
            healthBar = Box(panel.transform, "Health", new Vector2(0,1), new Vector2(15,-83), new Vector2(315,18), new Color(0.2f,0.9f,0.35f));
            infection = Label(panel.transform, "INFECTION", new Vector2(15,-109), new Vector2(315,30), 23);
            infectionBar = Box(panel.transform, "Infection", new Vector2(0,1), new Vector2(15,-143), new Vector2(315,18), new Color(1,0.5f,0.1f));
            score = Label(panel.transform, "", new Vector2(15,-182), new Vector2(315,30), 19);
            message = Label(root.transform, "", new Vector2(0,-270), new Vector2(1000,80), 27);
            message.rectTransform.anchorMin = message.rectTransform.anchorMax = new Vector2(0.5f,1);
            message.rectTransform.pivot = new Vector2(0.5f,1); message.alignment = TextAlignmentOptions.Center;
            var cross = Label(root.transform, "+", Vector2.zero, new Vector2(40,40), 32);
            cross.rectTransform.anchorMin = cross.rectTransform.anchorMax = cross.rectTransform.pivot = new Vector2(0.5f,0.5f);
            cross.alignment = TextAlignmentOptions.Center;
            var controls = Label(root.transform, "WASD move / Mouse aim\nHold left click to fire\nR restarts after death", new Vector2(-20,-20), new Vector2(340,100), 21);
            controls.rectTransform.anchorMin = controls.rectTransform.anchorMax = controls.rectTransform.pivot = Vector2.one;
            controls.alignment = TextAlignmentOptions.TopRight;
            result = Label(root.transform, "", new Vector2(0,-40), new Vector2(950,150), 34);
            result.rectTransform.anchorMin = result.rectTransform.anchorMax = result.rectTransform.pivot = new Vector2(0.5f,0.5f);
            result.alignment = TextAlignmentOptions.Center;
        }
        void Update()
        {
            var game = PrototypeGameManager.Instance;
            if (game == null || health == null) return;
            health.text = $"HEALTH  {game.Survivor.Health:0} / {game.Survivor.MaxHealth:0}";
            infection.text = $"INFECTED  {game.Infection.RemainingSeconds:0.0}s left";
            healthBar.rectTransform.sizeDelta = new Vector2(315 * Mathf.Clamp01(game.Survivor.Health / game.Survivor.MaxHealth),18);
            infectionBar.rectTransform.sizeDelta = new Vector2(315 * Mathf.Clamp01(game.Infection.RemainingSeconds / game.Infection.MaximumSeconds),18);
            score.text = $"WAVE {game.Wave}   KILLS {game.Kills}   TIME {game.RunTime:0}s";
            message.text = game.Survivor.IsAtCamp ? "SAFE CAMP\nHealed / Infection and score paused" : game.Message;
            result.text = game.RunEnded ? $"{game.EndReason}\n{game.Kills} kills / {game.RunTime:0.0} seconds\nPress R to restart" : "";
        }
        TextMeshProUGUI Label(Transform parent, string text, Vector2 position, Vector2 size, int fontSize)
        {
            var go = new GameObject("Label", typeof(RectTransform)); go.transform.SetParent(parent,false);
            var rect = go.GetComponent<RectTransform>(); rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0,1); rect.anchoredPosition = position; rect.sizeDelta = size;
            var label = go.AddComponent<TextMeshProUGUI>(); label.font = font; label.text = text; label.fontSize = fontSize; label.color = Color.white; label.raycastTarget = false;
            return label;
        }
        static UnityEngine.UI.Image Box(Transform parent, string name, Vector2 anchor, Vector2 position, Vector2 size, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(UnityEngine.UI.Image)); go.transform.SetParent(parent,false);
            var rect = go.GetComponent<RectTransform>(); rect.anchorMin = rect.anchorMax = rect.pivot = anchor; rect.anchoredPosition = position; rect.sizeDelta = size;
            var image = go.GetComponent<UnityEngine.UI.Image>(); image.color = color; image.raycastTarget = false; return image;
        }
    }
}
