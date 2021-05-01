using Android.App;
using Android.Content;
using Android.Gms.Ads;
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
    [Activity(Label = "@string/liked_videos", Theme = "@style/AppTheme")]
    public class LikedVideosActivity : AppCompatActivity
    {
        #region Variables
        private List<VideoStatus> videos;
        #endregion

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_liked_videos);

            Init();
        }

        private async void Init()
        {
            SwipeRefreshLayout swipeRefreshLayout = FindViewById<SwipeRefreshLayout>(Resource.Id.swipe_refresh_layout);

            swipeRefreshLayout.Refresh += async delegate
            {
                await Refresh();
                swipeRefreshLayout.Refreshing = false;
            };

            await Refresh();
        }

        private async Task Refresh()
        {
            RecyclerView recycler = FindViewById<RecyclerView>(Resource.Id.recyclerViewFavorites);
            ProgressBar progress = FindViewById<ProgressBar>(Resource.Id.progressBarFavorites);
            recycler.RemoveAllViewsInLayout();
            progress.Visibility = ViewStates.Visible;
            if (videos != null)
                videos.Clear();

            LoadAd();
            await CheckFavorites();

            if (videos != null && videos.Count > 0)
            {
                recycler.SetLayoutManager(new LinearLayoutManager(this));
                var adapter = new VideoAdapter(videos);
                adapter.ItemClick += Adapter_ItemClick;
                adapter.ItemLongClick += Adapter_ItemLongClick;
                recycler.SetAdapter(adapter);
            }
            else
                Toast.MakeText(this, GetString(Resource.String.favorites_notfound), ToastLength.Short).Show();

            progress.Visibility = ViewStates.Gone;
        }

        private void LoadAd()
        {
            try
            {
                FindViewById<AdView>(Resource.Id.adView).LoadAd(new AdRequest.Builder().Build());
            }
            catch
            {
                LoadAd();
            }
        }

        private void Adapter_ItemClick(object sender, ClickEventArgs e)
        {
            Intent intent = new Intent(this, typeof(PlayVideoActivity));
            intent.PutExtra("videoUrl", videos[e.Position].VideoPath);
            intent.PutExtra("coverUrl", videos[e.Position].CoverPath);
            intent.PutExtra("title", videos[e.Position].Title);
            intent.PutExtra("videoId", videos[e.Position].Id);
            intent.PutExtra("date", videos[e.Position].Date.ToString());
            intent.PutExtra("likes", videos[e.Position].LikesCount);
            intent.PutExtra("downloads", videos[e.Position].DownloadsCount);
            intent.PutExtra("views", videos[e.Position].ViewsCount);
            StartActivity(intent);
        }

        private void Adapter_ItemLongClick(object sender, ClickEventArgs e)
        {
            if (Preferences.ContainsKey("favorites", videos[e.Position].Id.ToString()))
            {
                Preferences.Remove("favorites", videos[e.Position].Id.ToString());
                Toast.MakeText(this, GetString(Resource.String.remove_from_favorites), ToastLength.Short).Show();
            }
            else
            {
                Preferences.Set("favorites", videos[e.Position].Id, videos[e.Position].Id.ToString());
                Toast.MakeText(this, GetString(Resource.String.added_to_favorites), ToastLength.Short).Show();
            }
        }

        private async Task CheckFavorites()
        {
            try
            {
                var filter = new VideoFilter() { SortType = StatusSortType.Random, Visible = true };
                filter.Pagination.RecordsPerPage = 1000000000;

                var response = await new MahwousRepositories().VideosRepository.GetFiltered(filter);

                if (response.Response != null && response.Response.Count > 0)
                    videos = response.Response.Where(v => Preferences.Get("favorites", -1, v.Id.ToString()) != -1).ToList();
                else
                    await CheckFavorites();
            }
            catch
            {
                await CheckFavorites();
            }
        }
    }
}