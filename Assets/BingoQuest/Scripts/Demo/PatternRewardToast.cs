using System.Collections.Generic;
using BingoQuest.Gameplay.Bingo;
using UnityEngine;

namespace BingoQuest.Demo
{
    /// <summary>
    /// Shows fading toast notifications for Bingo pattern rewards.
    /// Call AddToast(message) from DemoBootstrap when patterns complete.
    /// Also subscribes to BingoSystem events for square completions.
    /// </summary>
    public sealed class PatternRewardToast : MonoBehaviour
    {
        private struct Toast
        {
            public string Message;
            public float TimeLeft;
            public Color AccentColor;
        }

        private readonly List<Toast> _toasts = new();
        private const float ToastDuration  = 3.5f;
        private const float FadeStartFrac  = 0.35f;
        private static readonly Color DefaultAccent = new Color(1f, 0.85f, 0.2f);

        public void AddToast(string message, Color? accent = null)
        {
            _toasts.Add(new Toast
            {
                Message    = message,
                TimeLeft   = ToastDuration,
                AccentColor = accent ?? DefaultAccent
            });

            // Cap queue at 5
            while (_toasts.Count > 5)
                _toasts.RemoveAt(0);
        }

        private void Update()
        {
            for (int i = _toasts.Count - 1; i >= 0; i--)
            {
                var t = _toasts[i];
                t.TimeLeft -= Time.deltaTime;
                if (t.TimeLeft <= 0f)
                    _toasts.RemoveAt(i);
                else
                    _toasts[i] = t;
            }
        }

        private void OnGUI()
        {
            if (_toasts.Count == 0) return;

            float toastW = 480f;
            float toastH = 44f;
            float gap    = 8f;
            float startY = Screen.height * 0.35f;
            float cx     = Screen.width * 0.5f;

            for (int i = 0; i < _toasts.Count; i++)
            {
                var toast = _toasts[i];
                float alpha = toast.TimeLeft / ToastDuration;
                if (alpha < FadeStartFrac)
                    alpha = alpha / FadeStartFrac; // fade out

                float y = startY + i * (toastH + gap);
                var bgRect = new Rect(cx - toastW * 0.5f, y, toastW, toastH);

                // Background
                GUI.color = new Color(0.06f, 0.07f, 0.10f, 0.88f * alpha);
                GUI.DrawTexture(bgRect, Texture2D.whiteTexture);

                // Accent left bar
                GUI.color = new Color(toast.AccentColor.r, toast.AccentColor.g, toast.AccentColor.b, alpha);
                GUI.DrawTexture(new Rect(bgRect.x, bgRect.y, 4f, toastH), Texture2D.whiteTexture);

                // Message text
                GUI.color = new Color(1f, 1f, 1f, alpha);
                var style = new GUIStyle(GUI.skin.label)
                {
                    fontSize  = 14,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleLeft,
                    wordWrap  = false,
                    normal    = { textColor = Color.white }
                };
                GUI.Label(new Rect(bgRect.x + 12f, bgRect.y, bgRect.width - 16f, toastH), toast.Message, style);

                GUI.color = Color.white;
            }
        }
    }
}
