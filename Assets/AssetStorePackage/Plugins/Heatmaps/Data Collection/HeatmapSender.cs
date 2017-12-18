/// <summary>
/// Send Heatmap event based on VideoPlayer.
/// </summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityAnalytics360VideoHeatmap;

namespace UnityAnalytics360VideoHeatmap
{
    public class HeatmapSender : MonoBehaviour
    {
        public VideoPlayer videoPlayer;

        private int k_currentSendEventRatio;
        private bool k_eventSender;

        private int k_eventLimit = 5000;
        private float k_eventInterval = 1.0f;
        private WaitForSeconds k_waitForSecond;
        private Dictionary<string, object> k_clipDict;

        void Start()
        {
            if (videoPlayer == null)
            {
                videoPlayer = FindObjectOfType<VideoPlayer>();
            }

            EnsureRatio();
            k_eventInterval = DetermineEventInterval(k_eventLimit);
            k_waitForSecond = new WaitForSeconds(k_eventInterval);

#if UNITY_EDITOR
            StartCoroutine(SendHeatmapEvent());
#else
            if (k_eventSender)
                StartCoroutine(SendHeatmapEvent());
#endif
        }

        /// <summary>
        /// Make sure the ratio is most updated.
        /// </summary>
        private void EnsureRatio()
        {
            k_eventSender = PlayerPrefs.GetInt("_360VideoHeatMap_EventSender", 0) == 0 ? true : false;
            k_currentSendEventRatio = PlayerPrefs.GetInt("_360VideoHeatMap_EventRatio", 10);

            int m_newSendEventRatio = RemoteSettings.GetInt("heatmaps_sample_rate", 10);

            // If the ratio has changed or the role has not been assigned before, reassign the user.
            if (!PlayerPrefs.HasKey("_360VideoHeatMap_EventSender") || k_currentSendEventRatio != m_newSendEventRatio)
            {
                k_currentSendEventRatio = m_newSendEventRatio;
                EnsureUserRole();
                PlayerPrefs.SetInt("_360VideoHeatMap_EventSender", k_eventSender == true ? 0 : 1);
                PlayerPrefs.SetInt("_360VideoHeatMap_EventRatio", k_currentSendEventRatio);
            }
        }

        /// <summary>
        /// Check if the user is allowed to send event based on the RemoteSetting key.
        /// </summary>
        private void EnsureUserRole()
        {
            if (k_currentSendEventRatio >= 100)
            {
                k_eventSender = true;
                return;
            }

            if (k_currentSendEventRatio <= 0)
            {
                k_eventSender = false;
                return;
            }

            int m_random = Random.Range(0, 100);
            if (m_random <= k_currentSendEventRatio)
            {
                k_eventSender = true;
                return;
            }

            k_eventSender = false;
        }

        /// <summary>
        /// Send the event only when VideoPlayer, clip exist and the video is playing.
        /// </summary>
        IEnumerator SendHeatmapEvent()
        {
            if (k_clipDict == null)
                k_clipDict = new Dictionary<string, object>();

            while (true)
            {
                if (videoPlayer != null && videoPlayer.clip != null && videoPlayer.isPlaying)
                {
                    float m_standardTime = videoPlayer.frame / (float)videoPlayer.frameCount;
                    k_clipDict["clipName"] = videoPlayer.clip.name;
                    HeatmapEvent.Send("PlayerLook", this.transform, m_standardTime, k_clipDict);
                }

                yield return k_waitForSecond;
            }
        }

        /// <summary>
        /// Determine what's the time interval for event sending to not exceed the event limit.
        /// </summary>
        float DetermineEventInterval(int eventLimit)
        {
            float m_minimumInterval = 3600 / (float)eventLimit;
            return Mathf.Ceil(m_minimumInterval);
        }
    }
}
