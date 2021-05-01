using Android.Views;
using System;

namespace VideoStatuses
{
    public class ClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}