using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;

namespace LiveTunes
{
	public partial class PivotPageDotIndicator : UserControl
	{
		private Color _inactiveColor = Color.FromArgb(255, 128, 128, 128);
		private Color _activeColor = Color.FromArgb(255, 255, 255, 0);

		private List<Ellipse> _ellipses;

		public PivotPageDotIndicator()
		{
			InitializeComponent();

			RebuildDots();
		}

		private void RebuildDots()
		{
			LayoutRoot.Children.Clear();
			LayoutRoot.ColumnDefinitions.Clear();

			LayoutRoot.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star)});
			if (Pivot != null)
			{
				_ellipses = new List<Ellipse>(Pivot.Items.Count);

				for (int i = 0; i < Pivot.Items.Count; i++)
				{
					var fill = new SolidColorBrush(i == Pivot.SelectedIndex ? _activeColor : _inactiveColor);
					var dot = new Ellipse {Width = 5, Height = 5, Fill = fill, Margin = new Thickness(10)};
					dot.SetValue(Grid.ColumnProperty, i + 1);
					_ellipses.Add(dot);

					LayoutRoot.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
					LayoutRoot.Children.Add(dot);
				}
			}
			LayoutRoot.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
		}

		private void UpdateSelectedPivot(int oldIndex)
		{
			_ellipses[oldIndex].Fill = new SolidColorBrush(_inactiveColor);
			_ellipses[Pivot.SelectedIndex].Fill = new SolidColorBrush(_activeColor);
		}

		private void OnPivotSet()
		{
			Pivot.SelectionChanged += (sender, args) => UpdateSelectedPivot(Pivot.Items.IndexOf(args.RemovedItems[0]));
			RebuildDots();
		}

		public static readonly DependencyProperty PivotProperty = DependencyProperty.Register("Pivot", typeof(Pivot), typeof(PivotPageDotIndicator), new PropertyMetadata((d,e) => { (d as PivotPageDotIndicator).OnPivotSet();  }));
		public Pivot Pivot
		{
			get { return GetValue(PivotProperty) as Pivot; }
			set { SetValue(PivotProperty, value); }
		}

		public Color InactiveColor { get { return _inactiveColor; } set { _inactiveColor = value; } }
		public Color ActiveColor { get { return _activeColor; } set { _activeColor = value; } }
	}
}
