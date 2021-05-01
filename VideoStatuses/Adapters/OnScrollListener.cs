using AndroidX.RecyclerView.Widget;
using System;

namespace VideoStatuses
{
    public class OnScrollListener : RecyclerView.OnScrollListener
    {
        #region Variables
        public delegate void LoadMoreEventHandler(object sender, EventArgs e);
        public event LoadMoreEventHandler LoadMoreEvent;

        private readonly LinearLayoutManager LayoutManager;
        private int previousTotal = 0; // The total number of items in the dataset after the last load
        private bool loading = true; // True if we are still waiting for the last set of data to load.
        private readonly int visibleThreshold = 10; // The minimum amount of items to have below your current scroll position before loading more.
        private int firstVisibleItem, visibleItemCount, totalItemCount;
        #endregion

        public OnScrollListener(LinearLayoutManager layoutManager)
        {
            LayoutManager = layoutManager;
        }

        public override void OnScrolled(RecyclerView recyclerView, int dx, int dy)
        {
            base.OnScrolled(recyclerView, dx, dy);

            visibleItemCount = recyclerView.ChildCount;
            totalItemCount = recyclerView.GetAdapter().ItemCount;
            firstVisibleItem = LayoutManager.FindFirstVisibleItemPosition();

            if (loading)
            {
                if (totalItemCount > previousTotal)
                {
                    loading = false;

                    previousTotal = totalItemCount;
                }
            }

            if (!loading && (totalItemCount - visibleItemCount) <= (firstVisibleItem + visibleThreshold))
            {
                LoadMoreEvent(this, null);

                loading = true;
            }
        }
    }
}