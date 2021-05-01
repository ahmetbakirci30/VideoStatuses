using Android.App;
using Android.Content;
using Android.Gms.Ads;
using Android.OS;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.SwipeRefreshLayout.Widget;
using System;
using Xamarin.Essentials;
using Uri = Android.Net.Uri;

namespace VideoStatuses
{
    [Activity(Label = "@string/who_are_we", Theme = "@style/AppTheme.NoActionBar")]
    public class AboutWeActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_about_we);

            Init();
        }

        private void Init()
        {
            #region Initialize
            LinearLayout btnWebSite = FindViewById<LinearLayout>(Resource.Id.btn_web_site);
            LinearLayout btnYoutubeChannel = FindViewById<LinearLayout>(Resource.Id.btn_youtube);
            LinearLayout btnTelegram = FindViewById<LinearLayout>(Resource.Id.btn_telegram);
            LinearLayout btnFacebook = FindViewById<LinearLayout>(Resource.Id.btn_facebook);
            LinearLayout btnInstagram = FindViewById<LinearLayout>(Resource.Id.btn_instagram);
            LinearLayout btnWhatsapp = FindViewById<LinearLayout>(Resource.Id.btn_whatsapp);
            LinearLayout btnMail = FindViewById<LinearLayout>(Resource.Id.btn_mail);
            LinearLayout btnGooglePlay = FindViewById<LinearLayout>(Resource.Id.btn_play);
            #endregion

            #region Click Events
            btnWebSite.Click += Btn_Click;
            btnYoutubeChannel.Click += Btn_Click;
            btnTelegram.Click += Btn_Click;
            btnFacebook.Click += Btn_Click;
            btnInstagram.Click += Btn_Click;
            btnMail.Click += Btn_Click;
            btnGooglePlay.Click += Btn_Click;
            btnWhatsapp.Click += Btn_Click;
            #endregion

            LoadAds();
        }

        private void LoadAds()
        {
            try
            {
                FindViewById<AdView>(Resource.Id.adView).LoadAd(new AdRequest.Builder().Build());
            }
            catch
            {
                LoadAds();
            }
        }

        private void Btn_Click(object sender, EventArgs e)
        {
            try
            {
                string tag = ((LinearLayout)sender).Tag.ToString();

                if (tag.Equals(GetString(Resource.String.ep)))
                {
                    try
                    {
                        Intent email = new Intent(Intent.ActionSendto);
                        email.PutExtra(Intent.ExtraSubject, GetString(Resource.String.app_name));
                        email.SetType("message/rfc822");
                        email.SetData(Uri.Parse("mailto:" + tag));
                        StartActivity(email);
                    }
                    catch
                    {
                        Toast.MakeText(this, GetString(Resource.String.email_app_not_found), ToastLength.Short).Show();
                    }
                    return;
                }
                StartActivity(new Intent(Intent.ActionView, Uri.Parse(tag)));
            }
            catch
            {
                Toast.MakeText(this, GetString(Resource.String.error), ToastLength.Short).Show();
            }
        }
    }
}