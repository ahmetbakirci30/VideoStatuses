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
    public class VideoAdapter : RecyclerView.Adapter
    {
        #region Variables
        public event EventHandler<ClickEventArgs> ItemClick;
        public event EventHandler<ClickEventArgs> ItemLongClick;
        private void OnClick(ClickEventArgs args) => ItemClick?.Invoke(this, args);
        private void OnLongClick(ClickEventArgs args) => ItemLongClick?.Invoke(this, args);
        private readonly List<VideoStatus> videoList;
        #endregion

        public VideoAdapter(List<VideoStatus> videos)
        {
            videoList = videos;
        }

        public override int ItemCount => videoList.Count;

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            return new VideoViewHolder(LayoutInflater.From(parent.Context).Inflate(Resource.Layout.video_item_view, parent, false), OnClick, OnLongClick);
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            ((VideoViewHolder)holder).SetData(videoList[position], position);
        }

        private class VideoViewHolder : RecyclerView.ViewHolder
        {
            #region Variables
            private TextView VideoTitle { get; set; }
            private ImageView CoverImage { get; set; }
            private int lastPosition = -1;
            #endregion

            protected internal VideoViewHolder(View itemView, Action<ClickEventArgs> clickListener, Action<ClickEventArgs> longClickListener) : base(itemView)
            {
                VideoTitle = itemView.FindViewById<TextView>(Resource.Id.video_title);
                CoverImage = itemView.FindViewById<ImageView>(Resource.Id.videoImage);

                itemView.Click += (sender, e) => clickListener(new ClickEventArgs { View = itemView, Position = AdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new ClickEventArgs { View = itemView, Position = AdapterPosition });
            }

            protected internal void SetData(VideoStatus video, int position)
            {
                VideoTitle.Text = video.Title;
                Glide.With(Application.Context).Load(video.CoverPath).Placeholder(Application.Context.GetDrawable(Resource.Drawable.video)).Into(CoverImage);

                if (position > lastPosition)
                {
                    ItemView.StartAnimation(AnimationUtils.LoadAnimation(Application.Context, Resource.Animation.item_anim_slide_from_right));
                    lastPosition = position;
                }
            }
        }
    }
}