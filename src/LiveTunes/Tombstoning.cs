using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Phone.Controls;

namespace LiveTunes
{
    public class TombstoningService
    {
        #region ListBox
        public static void PushListBox(ListBox listBox, Action<double> actionOnTombstone)
        {
            ListBoxesToSerialize[listBox] = actionOnTombstone;
        }

        public static void PopListBox(ListBox listBox)
        {
            ListBoxesToSerialize.Remove(listBox);
        }
        #endregion

        #region Panorama
        public static void PushPanorama(Panorama panorama, Action<int> actionOnTombstone)
        {
            PanoramasToSerialize[panorama] = actionOnTombstone;
        }

        public static void PopPanorama(Panorama panorama)
        {
            PanoramasToSerialize.Remove(panorama);
        }
        #endregion

        public static void Serialize()
        {
            foreach (var listBox in ListBoxesToSerialize.Keys)
            {
                double scrollPos = ((VisualTreeHelper.GetChild(listBox, 0) as FrameworkElement).FindName("ScrollViewer") as ScrollViewer).VerticalOffset;
                ListBoxesToSerialize[listBox](scrollPos);
            }
            foreach (var panorama in PanoramasToSerialize.Keys)
            {
                PanoramasToSerialize[panorama](panorama.SelectedIndex);
            }
        }

        private static Dictionary<ListBox, Action<double>> ListBoxesToSerialize = new Dictionary<ListBox,Action<double>>();
        private static Dictionary<Panorama, Action<int>> PanoramasToSerialize = new Dictionary<Panorama, Action<int>>();
    }
}
