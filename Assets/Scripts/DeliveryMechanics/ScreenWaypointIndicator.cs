using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DeliveryMechanics
{
    public class ScreenWaypointIndicator : MonoBehaviour
    {
        private Camera _camera;
        private RectTransform _canvasRect;
        private RectTransform _rectTransform;
        private TMP_Text _text;

        private Vector3 _worldTarget;
        private float _edgePadding = 50f;
        private float _aboveTargetOffset = 5f;

        public TMP_Text Text => _text;

        public static ScreenWaypointIndicator Create(Camera cam)
        {
            var canvas = FindOverlayCanvas();
            if (canvas == null)
            {
                Debug.LogError("ScreenWaypointIndicator: No Screen Space Overlay canvas found in scene.");
                return null;
            }

            var go = new GameObject("WaypointIndicator");
            go.transform.SetParent(canvas.transform, false);

            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.sizeDelta = new Vector2(400f, 100f);

            var textGo = new GameObject("Text");
            textGo.transform.SetParent(go.transform, false);

            var textRect = textGo.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            var tmp = textGo.AddComponent<TextMeshProUGUI>();
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize = 24;
            tmp.color = Color.white;
            tmp.fontMaterial.SetFloat(ShaderUtilities.ID_OutlineWidth, 0.5f);
            tmp.fontMaterial.SetColor(ShaderUtilities.ID_OutlineColor, Color.black);
            tmp.fontMaterial.SetFloat(ShaderUtilities.ID_UnderlayOffsetX, 0f);
            tmp.fontMaterial.SetFloat(ShaderUtilities.ID_UnderlayOffsetY, 0f);
            tmp.fontMaterial.SetFloat(ShaderUtilities.ID_UnderlayDilate, 0.5f);
            tmp.fontMaterial.SetFloat(ShaderUtilities.ID_UnderlaySoftness, 0.2f);
            tmp.fontMaterial.SetColor(ShaderUtilities.ID_UnderlayColor, Color.black);
            tmp.fontMaterial.EnableKeyword("UNDERLAY_ON");

            var indicator = go.AddComponent<ScreenWaypointIndicator>();
            indicator._camera = cam;
            indicator._canvasRect = canvas.GetComponent<RectTransform>();
            indicator._rectTransform = rect;
            indicator._text = tmp;

            return indicator;
        }

        private static Canvas FindOverlayCanvas()
        {
            foreach (var canvas in FindObjectsByType<Canvas>(FindObjectsSortMode.None))
            {
                if (canvas.renderMode == RenderMode.ScreenSpaceOverlay && canvas.isRootCanvas)
                {
                    return canvas;
                }
            }
            return null;
        }

        public void SetWorldTarget(Vector3 worldPos)
        {
            _worldTarget = worldPos;
            _worldTarget.y += _aboveTargetOffset;
        }

        void LateUpdate()
        {
            if (_camera == null || _canvasRect == null) return;

            Vector3 viewportPos = _camera.WorldToViewportPoint(_worldTarget);
            bool isBehind = viewportPos.z < 0;

            if (isBehind)
            {
                viewportPos.x = 1f - viewportPos.x;
                viewportPos.y = 1f - viewportPos.y;
            }

            bool isOffScreen = isBehind
                || viewportPos.x < 0f || viewportPos.x > 1f
                || viewportPos.y < 0f || viewportPos.y > 1f;

            if (isOffScreen)
            {
                Vector2 dir = new Vector2(viewportPos.x - 0.5f, viewportPos.y - 0.5f);
                float maxAbs = Mathf.Max(Mathf.Abs(dir.x), Mathf.Abs(dir.y));
                if (maxAbs > 0.001f)
                {
                    float scale = 0.5f / maxAbs;
                    dir *= scale;
                }
                viewportPos.x = dir.x + 0.5f;
                viewportPos.y = dir.y + 0.5f;
            }

            Vector2 canvasSize = _canvasRect.sizeDelta;
            Vector2 screenPos = new Vector2(
                viewportPos.x * canvasSize.x - canvasSize.x * 0.5f,
                viewportPos.y * canvasSize.y - canvasSize.y * 0.5f
            );

            if (isOffScreen)
            {
                float halfW = canvasSize.x * 0.5f - _edgePadding;
                float halfH = canvasSize.y * 0.5f - _edgePadding;
                screenPos.x = Mathf.Clamp(screenPos.x, -halfW, halfW);
                screenPos.y = Mathf.Clamp(screenPos.y, -halfH, halfH);
            }

            _rectTransform.anchoredPosition = screenPos;
        }
    }
}
