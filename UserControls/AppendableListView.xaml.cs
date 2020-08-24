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

    /// <summary>A <see cref="UserControl"/> allowing the user to view and rearrange a list.</summary>
    /// <seealso cref="UserControl" />
    /// <seealso cref="System.Windows.Markup.IComponentConnector" />
    public partial class AppendableListView {

        /// <summary>Initialises a new instance of the <see cref="AppendableListView"/> class.</summary>
        public AppendableListView() {
            InitializeComponent();
        }

        /// <summary>Gets or sets the list property.</summary>
        /// <value>The list property.</value>
        public List<object> ListProperty {
            get => (List<object>)GetValue(ListPropertyProperty);
            set => SetValue(ListPropertyProperty, value);
        }

        /// <summary>A <see cref="DependencyProperty"/> responsible for handling event/bindings for <see cref="ListProperty"/>.</summary>
        public static readonly DependencyProperty ListPropertyProperty = DependencyProperty.Register(nameof(ListProperty), typeof(List<object>), typeof(AppendableListView), new PropertyMetadata(new List<object>(), DependencyChangedCallback));

        /// <summary>Called when <see cref="ListProperty"/> changes.</summary>
        /// <param name="D">The d.</param>
        /// <param name="E">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        public static void DependencyChangedCallback(DependencyObject D, DependencyPropertyChangedEventArgs E) {
            AppendableListView ALV = D as AppendableListView;
            ALV.OnListChanged();
        }

        /// <summary>Sets the list to the specified <paramref name="NewList"/>.</summary>
        /// <param name="NewList">The new list.</param>
        public void Set(List<object> NewList) => ListProperty = NewList;

        /// <summary>Adds the specified <paramref name="NewItem"/> to the list.</summary>
        /// <param name="NewItem">The new item.</param>
        public void Add(object NewItem) => ListProperty.Add(NewItem);

        /// <summary>Removes the specified <paramref name="NewItem"/> to the list.</summary>
        /// <param name="NewItem">The new item.</param>
        public void Remove(object NewItem) => ListProperty.Remove(NewItem);

        /// <summary>Clears the list.</summary>
        public void Clear() => ListProperty = new List<object>();

        /// <summary>Called when the list changes.</summary>
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

        /// <summary>Handles the <c>MouseMove</c> event of the <see cref="ViewPanel"/> control.</summary>
        /// <param name="Sender">The source of the event.</param>
        /// <param name="E">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
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

        /// <summary>The selection changed event</summary>
        public static readonly RoutedEvent SelectionChangedEvent = EventManager.RegisterRoutedEvent(
       nameof(SelectionChanged), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(AppendableListView));


        /// <summary>Occurs when the list selection changes.</summary>
        /// <param name="Sender">The sender.</param>
        /// <param name="E">The <see cref="AppendableSelectionChangedEventArgs"/> instance containing the event data.</param>
        public delegate void RoutedEventHandler(object Sender, AppendableSelectionChangedEventArgs E);

        /// <summary>Occurs when the list selection changes.</summary>
        public event RoutedEventHandler SelectionChanged {
            add => AddHandler(SelectionChangedEvent, value);
            remove => RemoveHandler(SelectionChangedEvent, value);
        }

        /// <summary>Handles the <c>SelectionChanged</c> event of the <see cref="ViewPanel"/> control.</summary>
        /// <param name="Sender">The source of the event.</param>
        /// <param name="E">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
        void ViewPanel_SelectionChanged(object Sender, SelectionChangedEventArgs E) {
            ListView LV = Sender as ListView;
            RaiseEvent(new AppendableSelectionChangedEventArgs(SelectionChangedEvent, LV.SelectedIndex, LV.SelectedItem));
        }

    }

    /// <summary>Provides data for the <see cref="AppendableListView.SelectionChanged"/> event. </summary>
    /// <seealso cref="System.Windows.RoutedEventArgs" />
    public class AppendableSelectionChangedEventArgs : RoutedEventArgs {
        /// <summary>The index of the selected item.</summary>
        public int SelectedIndex;
        /// <summary>The selected item.</summary>
        public object SelectedItem;

        /// <summary>Initialises a new instance of the <see cref="AppendableSelectionChangedEventArgs"/> class.</summary>
        /// <param name="RoutedEvent">The routed event.</param>
        /// <param name="SelectedIndex">Index of the selected.</param>
        /// <param name="SelectedItem">The selected item.</param>
        public AppendableSelectionChangedEventArgs(RoutedEvent RoutedEvent = null, int SelectedIndex = -1, object SelectedItem = null) {
            this.SelectedIndex = SelectedIndex;
            this.SelectedItem = SelectedItem;
            this.RoutedEvent = RoutedEvent;
        }
    }
}
