using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Ads;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.Core.App;
using Bumptech.Glide;
using System;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Essentials;
using static Android.Media.MediaPlayer;
using Environment = Android.OS.Environment;
using Permission = Android.Content.PM.Permission;
using Uri = Android.Net.Uri;

namespace VideoStatuses
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar")]
    public class PlayVideoActivity : AppCompatActivity, IOnPreparedListener, IOnCompletionListener, IOnErrorListener
    {
        #region Variables
        private ProgressBar progress;
        private ImageView imgCover, imgLike;
        private string statusTitle, videoPath;
        private int ID;
        private readonly string storagePath = Environment.GetExternalStoragePublicDirectory(Environment.DirectoryDownloads).AbsolutePath;
        #endregion

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_play_video);

            Init();
        }

        private void Init()
        {
            try
            {
                #region initialize
                statusTitle = Intent.GetStringExtra("title");
                videoPath = Intent.GetStringExtra("videoUrl");
                ID = Intent.GetIntExtra("videoId", -1);

                TextView videoTitle = FindViewById<TextView>(Resource.Id.toolbar_title);
                VideoView videoView = FindViewById<VideoView>(Resource.Id.full_screen_video);
                LinearLayout btnShareVideo = FindViewById<LinearLayout>(Resource.Id.btn_share);
                LinearLayout btnShareVideoWithWhatsapp = FindViewById<LinearLayout>(Resource.Id.btn_share_whatsapp);
                LinearLayout btnShareVideoWithInstagram = FindViewById<LinearLayout>(Resource.Id.btn_share_instagram);
                LinearLayout btnShareVideoWithFacebook = FindViewById<LinearLayout>(Resource.Id.btn_share_facebook);
                LinearLayout btnDownloadVideo = FindViewById<LinearLayout>(Resource.Id.btn_download);
                LinearLayout btnLikeVideo = FindViewById<LinearLayout>(Resource.Id.btn_like);
                progress = FindViewById<ProgressBar>(Resource.Id.loading);
                imgLike = FindViewById<ImageView>(Resource.Id.img_like);
                imgCover = FindViewById<ImageView>(Resource.Id.img_cover);

                videoTitle.Text = statusTitle;
                Glide.With(this).Load(Intent.GetStringExtra("coverUrl")).Into(imgCover);

                if (Preferences.ContainsKey("favorites", ID.ToString()))
                    imgLike.SetImageResource(Resource.Drawable.heart);

                videoView.SetVideoPath(Intent.GetStringExtra("videoUrl"));
                videoView.SetMediaController(new MediaController(this));
                videoView.SetOnPreparedListener(this);
                videoView.SetOnCompletionListener(this);
                videoView.SetOnErrorListener(this);

                LoadAds();

                int catId = Intent.GetIntExtra("catId", -1);
                SupportFragmentManager.BeginTransaction().Replace(Resource.Id.frameLayout, new VideosFragment(catId != -1 ? catId : 3)).Commit();
                #endregion

                #region Buttons Click Events
                btnShareVideoWithWhatsapp.Click += BtnShareOnWhatsapp_Click;
                btnShareVideoWithInstagram.Click += BtnShareOnInstagram_Click;
                btnShareVideoWithFacebook.Click += BtnShareOnFacebook_Click;
                btnShareVideo.Click += BtnShareVideo_Click;
                btnDownloadVideo.Click += BtnDownloadVideo_Click;
                btnLikeVideo.Click += BtnLikeVideo_Click;
                #endregion
            }
            catch
            {
                Toast.MakeText(this, GetString(Resource.String.error), ToastLength.Short).Show();
            }
        }

        private void LoadAds()
        {
            try
            {
                FindViewById<AdView>(Resource.Id.adView).LoadAd(new AdRequest.Builder().Build());
            }
            catch { }
        }

        public void OnPrepared(MediaPlayer mp)
        {
            try
            {
                progress.Visibility = ViewStates.Gone;
                imgCover.Visibility = ViewStates.Gone;
                mp.Start();
            }
            catch { }
        }

        public void OnCompletion(MediaPlayer mp)
        {
            mp.Start();
        }

        public bool OnError(MediaPlayer mp, [GeneratedEnum] MediaError what, int extra)
        {
            try
            {
                mp.Start();
            }
            catch { }

            return true;
        }

        private void BtnLikeVideo_Click(object sender, EventArgs e)
        {
            if (Preferences.ContainsKey("favorites", ID.ToString()))
            {
                imgLike.SetImageResource(Resource.Drawable.empty_heart);

                Preferences.Remove("favorites", ID.ToString());
                Toast.MakeText(this, GetString(Resource.String.remove_from_favorites), ToastLength.Short).Show();
            }
            else
            {
                imgLike.SetImageResource(Resource.Drawable.heart);

                Preferences.Set("favorites", ID, ID.ToString());
                Toast.MakeText(this, GetString(Resource.String.added_to_favorites), ToastLength.Short).Show();
            }
        }

        private void BtnShareVideo_Click(object sender, EventArgs e)
        {
            ShareVideo();
        }

        private void BtnShareOnWhatsapp_Click(object sender, EventArgs e)
        {
            ShareVideo("com.whatsapp");
        }

        private void BtnShareOnFacebook_Click(object sender, EventArgs e)
        {
            ShareVideo("com.facebook.katana");
        }

        private void BtnShareOnInstagram_Click(object sender, EventArgs e)
        {
            ShareVideo("com.instagram.android");
        }

        private void BtnDownloadVideo_Click(object sender, EventArgs e)
        {
            if ((int)Build.VERSION.SdkInt >= 23)
            {
                if (CheckSelfPermission(Manifest.Permission.WriteExternalStorage) != (int)Permission.Granted)
                {
                    RequestPermissions(new string[] { Manifest.Permission.WriteExternalStorage }, 978463125);
                    return;
                }
            }

            InstallVideo();
        }

        public void ShareVideo(string packageName = "defualt")
        {
            try
            {
                if (!packageName.Equals("defualt") && !IsAppInstalled(packageName))
                {
                    string appName;
                    if (packageName.Equals("com.whatsapp"))
                        appName = "تطبيق واتس اب ";
                    else if (packageName.Equals("com.instagram.android"))
                        appName = "تطبيق انستغرام ";
                    else
                        appName = "تطبيق الفيسبوك ";
                    Toast.MakeText(this, appName + "غير مثبت على جهازك", ToastLength.Short).Show();
                    return;
                }

                if ((int)Build.VERSION.SdkInt >= 23)
                {
                    if (CheckSelfPermission(Manifest.Permission.WriteExternalStorage) != (int)Permission.Granted)
                    {
                        RequestPermissions(new string[] { Manifest.Permission.WriteExternalStorage }, 135489213);
                        return;
                    }
                }

                InstallVideo();

                string path = Path.Combine(storagePath, GetString(Resource.String.app_name), URLUtil.GuessFileName(videoPath, null, MimeTypeMap.GetFileExtensionFromUrl(videoPath)));

                Intent intent = new Intent(Intent.ActionSend);
                intent.SetType("video/*");
                intent.PutExtra(Intent.ExtraText, "هذا الفيديو متوفر على تطبيق حالات فيديو بامكانكم تحميل التطبيق من Google Play مجاناً رابط التحميل\n" + "https://play.google.com/store/apps/details?id=" + PackageName);
                intent.PutExtra(Intent.ExtraStream, Uri.Parse(path));

                if (packageName.Equals("com.whatsapp"))
                    intent.SetPackage(packageName);

                StartActivity(Intent.CreateChooser(intent, GetString(Resource.String.share_with_friends)));
            }
            catch
            {
                Toast.MakeText(this, GetString(Resource.String.error), ToastLength.Short).Show();
            }
        }

        private bool IsAppInstalled(string packageName)
        {
            try
            {
                PackageManager.GetPackageInfo(packageName, PackageInfoFlags.Activities);
                return true;
            }
            catch// (PackageManager.NameNotFoundException)
            {
                return false;
            }
        }

        private void InstallVideo()
        {
            try
            {
                if (File.Exists(Path.Combine(storagePath, GetString(Resource.String.app_name), URLUtil.GuessFileName(videoPath, null, MimeTypeMap.GetFileExtensionFromUrl(videoPath)))))
                {
                    Toast.MakeText(this, GetString(Resource.String.video_already_downloaded), ToastLength.Short).Show();
                    return;
                }

                var request = new DownloadManager.Request(Uri.Parse(videoPath))
                    .SetTitle(statusTitle)
                    .SetDescription(GetString(Resource.String.load))
                    .SetNotificationVisibility(DownloadVisibility.VisibleNotifyCompleted)
                    .SetDestinationInExternalPublicDir(Environment.DirectoryDownloads, Path.Combine(GetString(Resource.String.app_name), URLUtil.GuessFileName(videoPath, null, MimeTypeMap.GetFileExtensionFromUrl(videoPath))));

                ((DownloadManager)GetSystemService(DownloadService)).Enqueue(request);

                Toast.MakeText(this, GetString(Resource.String.video_downloaded_successfully), ToastLength.Short).Show();
            }
            catch
            {
                Toast.MakeText(this, GetString(Resource.String.error), ToastLength.Short).Show();
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            if (requestCode == 978463125 && grantResults[0] == Permission.Granted)
                InstallVideo();

            else if (requestCode == 135489213 && grantResults[0] == Permission.Granted)
                ShareVideo();
        }
    }
}