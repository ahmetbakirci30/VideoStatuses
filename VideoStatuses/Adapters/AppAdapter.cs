using Android.App;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using MahwousWeb.Shared.Models;
using System;
using System.Collections.Generic;

namespace VideoStatuses
{
    public class AppAdapter : RecyclerView.Adapter
    {
        #region Variables
        public event EventHandler<ClickEventArgs> ItemClick;
        public event EventHandler<ClickEventArgs> ItemLongClick;
        private void OnClick(ClickEventArgs args) => ItemClick?.Invoke(this, args);
        private void OnLongClick(ClickEventArgs args) => ItemLongClick?.Invoke(this, args);
        private readonly List<App> apps;
        #endregion

        public AppAdapter(List<App> applications)
        {
            apps = applications;
        }

        public override int ItemCount => apps.Count;

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            ((AppViewHolder)holder).SetData(apps[position], position);
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            return new AppViewHolder(LayoutInflater.From(parent.Context).Inflate(Resource.Layout.app_item_view, parent, false), OnClick, OnLongClick);
        }

        private class AppViewHolder : RecyclerView.ViewHolder
        {
            #region Variables
            private TextView AppName { get; set; }
            private ImageView AppIcon { get; set; }
            private int lastPosition = -1;
            #endregion

            protected internal AppViewHolder(View itemView, Action<ClickEventArgs> clickListener, Action<ClickEventArgs> longClickListener) : base(itemView)
            {
                AppName = itemView.FindViewById<TextView>(Resource.Id.app_name);
                AppIcon = itemView.FindViewById<ImageView>(Resource.Id.app_icon);

                itemView.Click += (sender, e) => clickListener(new ClickEventArgs { View = itemView, Position = AdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new ClickEventArgs { View = itemView, Position = AdapterPosition });
            }

            protected internal void SetData(App app, int position)
            {
                AppName.Text = app.Name;
                Glide.With(Application.Context).Load(app.ImagePath).Into(AppIcon);

                if (position > lastPosition)
                {
                    ItemView.StartAnimation(AnimationUtils.LoadAnimation(Application.Context, Resource.Animation.item_anim_slide_from_right));
                    lastPosition = position;
                }
            }
        }
    }
}