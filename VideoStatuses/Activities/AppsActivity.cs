using Android.App;
using Android.Content;
using Android.Gms.Ads;
using Android.Net;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using MahwousWeb.Shared.Filters;
using MahwousWeb.Shared.Models;
using MahwousWeb.Shared.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace VideoStatuses
{
    [Activity(Label = "@string/other_apps", Theme = "@style/AppTheme")]
    public class AppsActivity : AppCompatActivity
    {
        #region Variables
        private List<App> applications;
        #endregion

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_apps);

            Init();
        }

        private async void Init()
        {
            SwipeRefreshLayout swipeRefreshLayout = FindViewById<SwipeRefreshLayout>(Resource.Id.swipe_refresh_layout_apps);

            swipeRefreshLayout.Refresh += async delegate
            {
                await Refresh();
                swipeRefreshLayout.Refreshing = false;
            };

            await Refresh();
        }

        private async Task Refresh()
        {
            RecyclerView recycler = FindViewById<RecyclerView>(Resource.Id.recycler_apps);
            ProgressBar progress = FindViewById<ProgressBar>(Resource.Id.progress_bar_apps);
            recycler.RemoveAllViewsInLayout();
            progress.Visibility = ViewStates.Visible;

            if (applications != null)
                applications.Clear();

            ShowAds();
            await GetApps();

            recycler.SetLayoutManager(new GridLayoutManager(this, 2));
            AppAdapter adapter = new AppAdapter(applications);
            adapter.ItemClick += Adapter_ItemClick;
            adapter.ItemLongClick += Adapter_ItemClick;
            recycler.SetAdapter(adapter);
            progress.Visibility = ViewStates.Gone;
        }

        private void Adapter_ItemClick(object sender, ClickEventArgs e)
        {
            StartActivity(new Intent(Intent.ActionView, Uri.Parse(applications[e.Position].PlayStoreLink)));
        }

        private async Task GetApps()
        {
            try
            {
                var response = await new MahwousRepositories().AppsRepository.GetFiltered(new AppFilter() { SortType = SortType.Random });

                if (response.Response != null && response.Response.Count > 0)
                    applications = response.Response.ToList();
                else
                    await GetApps();
            }
            catch
            {
                await GetApps();
            }
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