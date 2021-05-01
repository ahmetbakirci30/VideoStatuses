using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;
using AndroidX.Work;
using MahwousWeb.Shared.Repositories;
using Xamarin.Essentials;
using Notification = MahwousWeb.Shared.Models.Notification;

namespace VideoStatuses
{
    public class NotificationWorker : Worker
    {
        #region Variables
        private static readonly string CHANNEL_ID = "location_notification_id";
        #endregion

        public NotificationWorker(Context context, WorkerParameters workerParameters) : base(context, workerParameters)
        {
            CreateNotificationChannel();
        }

        public override Result DoWork()
        {
            CheckNotifications();

            return Result.InvokeSuccess();
        }

        private int LastNotificationId
        {
            get => Preferences.Get("last_notification_id", -1);
            set => Preferences.Set("last_notification_id", value);
        }

        private async void CheckNotifications()
        {
            try
            {
                Notification notification = await new MahwousRepositories().NotificationsRepository.GetLastNotification(AppInfo.PackageName);

                if (notification != null)
                {
                    if (notification.Id != LastNotificationId || LastNotificationId == -1)
                    {
                        ShowNotification(notification);
                        await new MahwousRepositories().NotificationsRepository.IncrementRecived(notification.Id);
                        LastNotificationId = notification.Id;
                        return;
                    }
                }
            }
            catch
            {
                return;
            }
        }

        private void ShowNotification(Notification notification)
        {
            Intent intent = new Intent(Application.Context, typeof(MainActivity));
            intent.PutExtra("notification_id", notification.Id);

            NotificationCompat.Builder builder = new NotificationCompat.Builder(Application.Context, CHANNEL_ID)
                .SetAutoCancel(true)
                .SetSmallIcon(Resource.Drawable.notification)
                .SetContentTitle(notification.Title)
                .SetContentText(notification.Description)
                .SetPriority(NotificationCompat.PriorityHigh)
                .SetCategory(NotificationCompat.CategoryMessage)
                .SetStyle(new NotificationCompat.BigTextStyle()
                .SetBigContentTitle(notification.Title)
                .BigText(notification.Description))
                .SetContentIntent(PendingIntent.GetActivity(Application.Context, 0, intent, PendingIntentFlags.OneShot));

            NotificationManagerCompat.From(Application.Context).Notify(notification.Id, builder.Build());
        }

        private void CreateNotificationChannel()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                string name = Application.Context.GetString(Resource.String.channel_name);
                string description = Application.Context.GetString(Resource.String.channel_description);
                NotificationChannel channel = new NotificationChannel(CHANNEL_ID, name, NotificationImportance.High)
                {
                    Description = description
                };

                NotificationManager notificationManager = (NotificationManager)Application.Context.GetSystemService(Context.NotificationService);
                notificationManager.CreateNotificationChannel(channel);
            }
        }
    }
}