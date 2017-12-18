using System;

namespace UnityAnalytics360VideoHeatmap
{
    // Note variance from C# code standard. We need these to match consts on server.
    public enum UnityAnalyticsEventType
    {
        appStart,
        appRunning,
        custom,
        transaction,
        userInfo,
        deviceInfo,
    }
}
