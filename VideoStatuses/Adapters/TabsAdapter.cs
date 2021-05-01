using AndroidX.Fragment.App;
using Java.Lang;
using MahwousWeb.Shared.Models;
using System.Collections.Generic;

namespace VideoStatuses
{
    public class TabsAdapter : FragmentPagerAdapter
    {
        #region Variables
        private readonly List<Category> categoryList;
        #endregion

        public TabsAdapter(FragmentManager fm, List<Category> categories) : base(fm, BehaviorResumeOnlyCurrentFragment)
        {
            categoryList = categories;
        }

        public override int Count => categoryList.Count;

        public override Fragment GetItem(int position) => new VideosFragment(categoryList[position].Id);

        public override ICharSequence GetPageTitleFormatted(int position) => new String(categoryList[position].Name);
    }
}