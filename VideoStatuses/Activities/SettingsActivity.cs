using Android.App;
using Android.Gms.Ads;
using Android.OS;
using AndroidX.AppCompat.App;
using AndroidX.SwipeRefreshLayout.Widget;
using Xamarin.Essentials;

namespace VideoStatuses
{
    [Activity(Label = "@string/action_settings", Theme = "@style/AppTheme")]
    public class SettingsActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_settings);

            Init();
        }

        private void Init()
        {
            SwipeRefreshLayout refreshLayout = FindViewById<SwipeRefreshLayout>(Resource.Id.swipe_refresh_layout_settings);

            refreshLayout.Refresh += delegate
            {
                ShowAds();
                refreshLayout.Refreshing = false;
            };

            ShowAds();
        }

        private void ShowAds()
        {
            try
            {
                FindViewById<AdView>(Resource.Id.adView).LoadAd(new AdRequest.Builder().Build());
            }
            catch
            {
                ShowAds();
            }
        }
    }
}