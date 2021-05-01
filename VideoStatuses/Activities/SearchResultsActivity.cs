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
    [Activity(Label = "@string/search_results", Theme = "@style/AppTheme")]
    public class SearchResultsActivity : AppCompatActivity
    {
        #region Variables
        private List<VideoStatus> videos;
        private string text;
        #endregion

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_search_results);

            Init();
        }

        private async void Init()
        {
            text = Intent.GetStringExtra("text");

            var swipeRefreshLayout = FindViewById<SwipeRefreshLayout>(Resource.Id.swipe_refresh_layout_search);

            swipeRefreshLayout.Refresh += async delegate
            {
                await Refresh();
                swipeRefreshLayout.Refreshing = false;
            };

            await Refresh();
        }

        private async Task Refresh()
        {
            RecyclerView recycler = FindViewById<RecyclerView>(Resource.Id.recycler_search_results);
            ProgressBar progress = FindViewById<ProgressBar>(Resource.Id.progressBarSearch);
            recycler.RemoveAllViewsInLayout();
            progress.Visibility = ViewStates.Visible;
            if (videos != null)
                videos.Clear();

            LoadAds();
            await SearchVideos();

            if (videos != null && videos.Count > 0)
            {
                recycler.SetLayoutManager(new LinearLayoutManager(this));
                VideoAdapter adapter = new VideoAdapter(videos);
                adapter.ItemClick += Adapter_ItemClick;
                adapter.ItemLongClick += Adapter_ItemLongClick;
                recycler.SetAdapter(adapter);
            }
            else
                Toast.MakeText(this, GetString(Resource.String.videos_notfound), ToastLength.Short).Show();

            progress.Visibility = ViewStates.Gone;
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

        private async Task SearchVideos()
        {
            try
            {
                string word = string.IsNullOrWhiteSpace(text) ? string.Empty : text;

                var response = await new MahwousRepositories().VideosRepository.GetFiltered(new VideoFilter { Name = word });

                if (response.Response != null)
                    videos = response.Response.ToList();
                else
                    await SearchVideos();
            }
            catch
            {
                await SearchVideos();
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
    }
}