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

        #region Pivot
        public static void PushPivot(Pivot pivot, Action<int> actionOnTombstone)
        {
            PivotsToSerialize[pivot] = actionOnTombstone;
        }

        public static void PopPivot(Pivot pivot)
        {
            PivotsToSerialize.Remove(pivot);
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
            foreach (var pivot in PivotsToSerialize.Keys)
            {
                PivotsToSerialize[pivot](pivot.SelectedIndex);
            }
        }

        private static Dictionary<ListBox, Action<double>> ListBoxesToSerialize = new Dictionary<ListBox,Action<double>>();
        private static Dictionary<Panorama, Action<int>> PanoramasToSerialize = new Dictionary<Panorama, Action<int>>();
        private static Dictionary<Pivot, Action<int>> PivotsToSerialize = new Dictionary<Pivot, Action<int>>();
    }
}
