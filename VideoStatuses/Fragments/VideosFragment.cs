using Android.Content;
using Android.Gms.Ads;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.Fragment.App;
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
    public class VideosFragment : Fragment
    {
        #region Variables
        private readonly int categoryId;
        private View view;
        private List<VideoStatus> videos;
        private int pageNumber = 1;
        private InterstitialAd interstitialAd;
        #endregion

        public VideosFragment(int categoryId = 3)
        {
            this.categoryId = categoryId;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            view = inflater.Inflate(Resource.Layout.fragment_videos, container, false);
            Init();
            return view;
        }

        private async void Init()
        {
            try
            {
                #region Initialize
                SwipeRefreshLayout refreshLayout = view.FindViewById<SwipeRefreshLayout>(Resource.Id.refresh_layout);

                refreshLayout.Refresh += async delegate
                {
                    await Refresh();
                    refreshLayout.Refreshing = false;
                };

                await Refresh();
                LoadAd();
                #endregion
            }
            catch
            {
                Init();
            }
        }

        private async Task Refresh()
        {
            try
            {
                RecyclerView recycler = view.FindViewById<RecyclerView>(Resource.Id.recycler_view_videos);
                ProgressBar progress = view.FindViewById<ProgressBar>(Resource.Id.progress_load);
                TextView text = view.FindViewById<TextView>(Resource.Id.text_loading);
                ProgressBar loadMoreBar = view.FindViewById<ProgressBar>(Resource.Id.load_more);
                recycler.RemoveAllViewsInLayout();
                progress.Visibility = ViewStates.Visible;
                pageNumber = 1;

                if (videos != null)
                    videos.Clear();

                await LoadVideos();

                if (videos != null && videos.Count > 0)
                {
                    LinearLayoutManager layoutManager = new LinearLayoutManager(view.Context);
                    recycler.SetLayoutManager(layoutManager);
                    OnScrollListener onScrollListener = new OnScrollListener(layoutManager);
                    recycler.AddOnScrollListener(onScrollListener);
                    VideoAdapter adapter = new VideoAdapter(videos);
                    adapter.ItemClick += Adapter_ItemClick;
                    adapter.ItemLongClick += Adapter_ItemLongClick;
                    recycler.SetAdapter(adapter);

                    onScrollListener.LoadMoreEvent += async delegate
                    {
                        loadMoreBar.Visibility = ViewStates.Visible;
                        await LoadVideos();
                        adapter.NotifyDataSetChanged();
                        loadMoreBar.Visibility = ViewStates.Gone;
                    };
                }

                recycler.Visibility = ViewStates.Visible;
                progress.Visibility = ViewStates.Gone;
                text.Visibility = ViewStates.Gone;
            }
            catch
            {
                await Refresh();
            }
        }

        private async Task LoadVideos()
        {
            try
            {
                Category category = await new MahwousRepositories().CategoriesRepository.Get(categoryId);
                VideoFilter filter = new VideoFilter() { SortType = StatusSortType.Random, Visible = true };
                filter.Categories.Add(category);
                filter.Pagination.Page = pageNumber;

                var response = await new MahwousRepositories().VideosRepository.GetFiltered(filter);

                if (response.Response != null && response.Response.Count > 0)
                {
                    if (videos == null)
                        videos = response.Response.ToList();
                    else
                        videos.AddRange(response.Response);

                    pageNumber++;
                }
            }
            catch
            {
                await LoadVideos();
            }
        }

        private void Adapter_ItemClick(object sender, ClickEventArgs e)
        {
            StartVideoStatus(e.Position);

            if (interstitialAd.IsLoaded)
                interstitialAd.Show();

            LoadAd();
        }

        private void StartVideoStatus(int position)
        {
            try
            {
                Intent intent = new Intent(view.Context, typeof(PlayVideoActivity));
                intent.PutExtra("videoUrl", videos[position].VideoPath);
                intent.PutExtra("coverUrl", videos[position].CoverPath);
                intent.PutExtra("title", videos[position].Title);
                intent.PutExtra("videoId", videos[position].Id);
                intent.PutExtra("catId", categoryId);
                intent.PutExtra("date", videos[position].Date.ToString());
                intent.PutExtra("likes", videos[position].LikesCount);
                intent.PutExtra("downloads", videos[position].DownloadsCount);
                intent.PutExtra("views", videos[position].ViewsCount);
                StartActivity(intent);
            }
            catch
            {
                Toast.MakeText(view.Context, GetString(Resource.String.display_video_error), ToastLength.Short).Show();
            }
        }

        private void Adapter_ItemLongClick(object sender, ClickEventArgs e)
        {
            try
            {
                if (Preferences.ContainsKey("favorites", videos[e.Position].Id.ToString()))
                {
                    Preferences.Remove("favorites", videos[e.Position].Id.ToString());
                    Toast.MakeText(view.Context, GetString(Resource.String.remove_from_favorites), ToastLength.Short).Show();
                }
                else
                {
                    Preferences.Set("favorites", videos[e.Position].Id, videos[e.Position].Id.ToString());
                    Toast.MakeText(view.Context, GetString(Resource.String.added_to_favorites), ToastLength.Short).Show();
                }
            }
            catch { }
        }

        private void LoadAd()
        {
            try
            {
                interstitialAd = new InterstitialAd(view.Context)
                {
                    AdUnitId = GetString(Resource.String.interstitial_ad_between_videos)
                };

                interstitialAd.LoadAd(new AdRequest.Builder().Build());
            }
            catch { }
        }
    }
}