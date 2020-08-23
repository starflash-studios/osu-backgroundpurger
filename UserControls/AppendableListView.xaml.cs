#region Copyright (C) 2017-2020  Starflash Studios
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License (Version 3.0)
// as published by the Free Software Foundation.
// 
// More information can be found here: https://www.gnu.org/licenses/gpl-3.0.en.html
#endregion

#region Using Directives

using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

#endregion

namespace Osu_BackgroundPurge.UserControls {
    public partial class AppendableListView {
        public AppendableListView() {
            InitializeComponent();
        }

        public List<object> ListProperty {
            get => (List<object>)GetValue(ListPropertyProperty);
            set => SetValue(ListPropertyProperty, value);
        }

        public static readonly DependencyProperty ListPropertyProperty = DependencyProperty.Register(nameof(ListProperty), typeof(List<object>), typeof(AppendableListView), new PropertyMetadata(new List<object>(), DependencyChangedCallback));

        public static void DependencyChangedCallback(DependencyObject D, DependencyPropertyChangedEventArgs E) {
            AppendableListView ALV = D as AppendableListView;
            ALV.OnListChanged();
        }

        public void SetList(List<object> NewList) => ListProperty = NewList;

        public void Append(object NewItem) => ListProperty.Add(NewItem);

        public void Remove(object NewItem) => ListProperty.Remove(NewItem);

        public void Clear() => ListProperty = new List<object>();

        protected internal void OnListChanged() {
            Debug.WriteLine("List changed; update view...");
            ViewPanel.Items.Clear();

            foreach(object Item in ListProperty) {
                Debug.WriteLine($"\t\t=> Adding {Item}");
                ViewPanel.Items.Add(Item);
            }

            LabelListCount.Content = ViewPanel.Items.Count.ToString("N0");
            Debug.WriteLine("\t=> Finished view update.");
        }

        void ViewPanel_MouseMove(object Sender, MouseEventArgs E) {
            DependencyObject HitTest = VisualTreeHelper.HitTest(ViewPanel, E.GetPosition(ViewPanel)).VisualHit;

            // find ListViewItem (or null)
            while (HitTest != null && !(HitTest is ListViewItem)) {
                HitTest = VisualTreeHelper.GetParent(HitTest);
            }

            if (HitTest != null) {
                int I = ViewPanel.Items.IndexOf(((ListViewItem)HitTest).DataContext);
                LabelHoverIndex.Content = $"#{I:N0}";
                //label.Content = $"I'm on item {i}";
            }
        }


        public static readonly RoutedEvent SelectionChangedEvent = EventManager.RegisterRoutedEvent(
       nameof(SelectionChanged), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(AppendableListView));


        public delegate void RoutedEventHandler(object Sender, AppendableSelectionChangedEventArgs E);

        public event RoutedEventHandler SelectionChanged {
            add => AddHandler(SelectionChangedEvent, value);
            remove => RemoveHandler(SelectionChangedEvent, value);
        }

        void ViewPanel_SelectionChanged(object Sender, SelectionChangedEventArgs E) {
            ListView LV = Sender as ListView;
            RaiseEvent(new AppendableSelectionChangedEventArgs(SelectionChangedEvent, LV.SelectedIndex, LV.SelectedItem));
        }

    }

    public class AppendableSelectionChangedEventArgs : RoutedEventArgs {
        public int SelectedIndex;
        public object SelectedItem;

        public AppendableSelectionChangedEventArgs(RoutedEvent RoutedEvent = null, int SelectedIndex = -1, object SelectedItem = null) {
            this.SelectedIndex = SelectedIndex;
            this.SelectedItem = SelectedItem;
            this.RoutedEvent = RoutedEvent;
        }
    }
}
