using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Goose2Client
{
    public class DebugWindow : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI fpsText;
        [SerializeField] private TextMeshProUGUI versionText;

        public static int FramesPerSecond { get; private set; }

        private float frequency = 0.5f;

        private void Start()
        {
            versionText.text = Application.version;

            StartCoroutine(FPS());
        }

        private IEnumerator FPS()
        {
            for (; ; )
            {
                int lastFrameCount = Time.frameCount;
                float lastTime = Time.realtimeSinceStartup;
                yield return new WaitForSeconds(frequency);

                float timeSpan = Time.realtimeSinceStartup - lastTime;
                int frameCount = Time.frameCount - lastFrameCount;

                FramesPerSecond = Mathf.RoundToInt(frameCount / timeSpan);
                fpsText.text = $"{FramesPerSecond} fps";
            }
        }
    }
}