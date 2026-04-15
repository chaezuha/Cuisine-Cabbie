using UnityEngine;
using UnityEngine.UI;

namespace DeliveryMechanics
{
    public class MinimapIconIndicator : MonoBehaviour
    {
        private Camera _minimapCamera;
        private RectTransform _minimapRect;
        private RectTransform _rectTransform;
        private Image _image;

        private Vector3 _worldTarget;
        private float _edgePadding = 10f;

        public static MinimapIconIndicator Create(Camera minimapCam, Sprite sprite = null, float size = 20f, Color? tint = null)
        {
            var minimapUI = FindMinimapUI();
            if (minimapUI == null)
            {
                Debug.LogError("MinimapIconIndicator: No 'Mini Map UI' RawImage found in scene.");
                return null;
            }

            var go = new GameObject("MinimapIcon");
            go.transform.SetParent(minimapUI.transform.parent, false);
            go.transform.SetSiblingIndex(minimapUI.transform.GetSiblingIndex() + 1);

            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = minimapUI.anchorMin;
            rect.anchorMax = minimapUI.anchorMax;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(size, size);

            var image = go.AddComponent<Image>();
            if (sprite != null)
            {
                image.sprite = sprite;
                image.color = tint ?? Color.white;
            }
            else
            {
                image.color = tint ?? Color.white;
            }

            var indicator = go.AddComponent<MinimapIconIndicator>();
            indicator._minimapCamera = minimapCam;
            indicator._minimapRect = minimapUI;
            indicator._rectTransform = rect;
            indicator._image = image;

            return indicator;
        }

        private static RectTransform FindMinimapUI()
        {
            var go = GameObject.Find("Mini Map UI");
            if (go != null)
                return go.GetComponent<RectTransform>();
            return null;
        }

        public void SetWorldTarget(Vector3 worldPos)
        {
            _worldTarget = worldPos;
        }

        void LateUpdate()
        {
            if (_minimapCamera == null || _minimapRect == null) return;

            Vector3 viewportPos = _minimapCamera.WorldToViewportPoint(_worldTarget);

            float half = 0.5f;
            viewportPos.x = Mathf.Clamp(viewportPos.x, 0f, 1f);
            viewportPos.y = Mathf.Clamp(viewportPos.y, 0f, 1f);

            Vector2 minimapSize = _minimapRect.sizeDelta;
            Vector2 minimapPos = _minimapRect.anchoredPosition;

            Vector2 localPos = new Vector2(
                (viewportPos.x - 0.5f) * minimapSize.x,
                (viewportPos.y - 0.5f) * minimapSize.y
            );

            float halfW = minimapSize.x * 0.5f - _edgePadding;
            float halfH = minimapSize.y * 0.5f - _edgePadding;
            localPos.x = Mathf.Clamp(localPos.x, -halfW, halfW);
            localPos.y = Mathf.Clamp(localPos.y, -halfH, halfH);

            _rectTransform.anchoredPosition = minimapPos + localPos;
        }
    }
}
