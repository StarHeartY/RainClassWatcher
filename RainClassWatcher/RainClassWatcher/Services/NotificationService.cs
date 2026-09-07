using System;
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;

namespace RainClassWatcher.Services
{
    internal sealed class NotificationService
    {
        public void NotifyNewExercise(string page)
        {
            var notification = new AppNotificationBuilder()
                .AddText("雨课堂有新习题")
                .AddText($"{page} 刚刚发布，尚未作答，请及时查看。")
                .SetAudioEvent(AppNotificationSoundEvent.Default)
                .BuildNotification();

            AppNotificationManager.Default.Show(notification);
        }
    }
}
