using Android.App;
using Android.Content;
using Android.Gms.Ads;
using Android.Net;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.Core.View;
using AndroidX.DrawerLayout.Widget;
using AndroidX.ViewPager.Widget;
using AndroidX.Work;
using Google.Android.Material.Navigation;
using Google.Android.Material.Tabs;
using MahwousWeb.Shared.Filters;
using MahwousWeb.Shared.Repositories;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Xamarin.Essentials;
using static AndroidX.AppCompat.Widget.SearchView;
using SearchView = AndroidX.AppCompat.Widget.SearchView;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace VideoStatuses
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity, NavigationView.IOnNavigationItemSelectedListener, IOnQueryTextListener
    {
        #region Variables
        private readonly Timer backPressTimer = new Timer();
        private int id;
        #endregion

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            Init();
        }

        private async void Init()
        {
            try
            {
                await SetCategories();

                RelativeLayout LoaddingLayout = FindViewById<RelativeLayout>(Resource.Id.loadding_layout);
                LoaddingLayout.Visibility = ViewStates.Gone;

                Toolbar toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
                SetSupportActionBar(toolbar);

                DrawerLayout drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
                drawer.Visibility = ViewStates.Visible;

                ActionBarDrawerToggle toggle = new ActionBarDrawerToggle(this, drawer, toolbar, Resource.String.navigation_drawer_open, Resource.String.navigation_drawer_close);
                drawer.AddDrawerListener(toggle);
                toggle.SyncState();

                NavigationView navigationView = FindViewById<NavigationView>(Resource.Id.nav_view);
                navigationView.SetNavigationItemSelectedListener(this);

                await ShowAdsAndCheckNotification();
            }
            catch
            {
                Init();
            }
        }

        private async Task ShowAdsAndCheckNotification()
        {
            try
            {
                if ((id = Intent.GetIntExtra("notification_id", -1)) != -1)
                    await new MahwousRepositories().NotificationsRepository.IncrementOpened(id);

                FindViewById<AdView>(Resource.Id.adView).LoadAd(new AdRequest.Builder().Build());

                WorkManager.GetInstance(Application.Context).Enqueue(PeriodicWorkRequest.Builder.From<NotificationWorker>(System.TimeSpan.FromMinutes(15)).Build());
            }
            catch
            {
                return;
            }
        }

        private async Task SetCategories()
        {
            try
            {
                CategoryFilter filter = new CategoryFilter() { ForVideos = true, SortType = SortType.Random, Visible = true };
                filter.Pagination.RecordsPerPage = 100000000;

                var response = await new MahwousRepositories().CategoriesRepository.GetFiltered(filter);

                if (response.Response != null && response.Response.Count > 0)
                {
                    TabsAdapter adapter = new TabsAdapter(SupportFragmentManager, response.Response.ToList());

                    ViewPager pager = FindViewById<ViewPager>(Resource.Id.viewPager);
                    pager.Adapter = adapter;

                    TabLayout tabLayout = FindViewById<TabLayout>(Resource.Id.tabLayout);
                    tabLayout.SetupWithViewPager(pager);
                }
                else
                    await SetCategories();
            }
            catch
            {
                await SetCategories();
            }
        }

        public override void OnBackPressed()
        {
            DrawerLayout drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);

            if (drawer.IsDrawerOpen(GravityCompat.Start))
                drawer.CloseDrawer(GravityCompat.Start);

            else if (!backPressTimer.Enabled)
            {
                Toast.MakeText(this, GetString(Resource.String.press_again_to_exit), ToastLength.Short).Show();

                backPressTimer.Interval = 2000;
                backPressTimer.Elapsed += BackPressTimer_Elapsed;
                backPressTimer.Start();
            }
            else
                base.OnBackPressed();
        }

        private void BackPressTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            backPressTimer.Stop();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            SearchManager searchManager = GetSystemService(SearchService) as SearchManager;
            SearchView searchView = menu.FindItem(Resource.Id.nav_search).ActionView as SearchView;
            searchView.SetSearchableInfo(searchManager.GetSearchableInfo(ComponentName));
            searchView.SetOnQueryTextListener(this);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            CheckSelectedItem(item.ItemId);

            return base.OnOptionsItemSelected(item);
        }

        public bool OnNavigationItemSelected(IMenuItem item)
        {
            CheckSelectedItem(item.ItemId);
            FindViewById<DrawerLayout>(Resource.Id.drawer_layout).CloseDrawer(GravityCompat.Start);
            return true;
        }

        private void CheckSelectedItem(int id)
        {
            if (id == Resource.Id.nav_home)
            { }
            else if (id == Resource.Id.nav_apps)
            {
                StartActivity(new Intent(this, typeof(AppsActivity)));
            }
            else if (id == Resource.Id.nav_share)
            {
                Intent intent = new Intent(Intent.ActionSend);
                intent.SetType("text/plain");
                intent.PutExtra(Intent.ExtraText, GetString(Resource.String.app_name) + "\n" + GetString(Resource.String.description) + "\nhttps://play.google.com/store/apps/details?id=" + PackageName);
                StartActivity(Intent.CreateChooser(intent, "مشاركة تطبيق حالات فيديو"));
            }
            else if (id == Resource.Id.nav_app_evaluation)
            {
                StartActivity(new Intent(Intent.ActionView, Uri.Parse("https://play.google.com/store/apps/details?id=" + PackageName)));
            }
            else if (id == Resource.Id.nav_who_are_we)
            {
                StartActivity(new Intent(this, typeof(AboutWeActivity)));
            }
            else if (id == Resource.Id.nav_about_developer)
            {
                StartActivity(new Intent(Intent.ActionView, Uri.Parse("https://play.google.com/store/apps/dev?id=8328270605427376004")));
            }
            else if (id == Resource.Id.nav_like)
            {
                StartActivity(new Intent(this, typeof(LikedVideosActivity)));
            }
            else if (id == Resource.Id.action_settings)
            {
                StartActivity(new Intent(this, typeof(SettingsActivity)));
            }
        }

        public bool OnQueryTextChange(string newText)
        {
            return true;
        }

        public bool OnQueryTextSubmit(string newText)
        {
            Intent intent = new Intent(this, typeof(SearchResultsActivity));
            intent.PutExtra("text", newText);
            StartActivity(intent);
            return true;
        }
    }
}