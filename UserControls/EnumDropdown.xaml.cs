#region Copyright (C) 2017-2020  Starflash Studios
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License (Version 3.0)
// as published by the Free Software Foundation.
// 
// More information can be found here: https://www.gnu.org/licenses/gpl-3.0.en.html
#endregion

#region Using Directives

using System;
using System.Collections;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

#endregion

namespace Osu_BackgroundPurge.UserControls {

    public partial class EnumDropdown {

        public static readonly RoutedEvent SelectionChangedEvent = EventManager.RegisterRoutedEvent(
       nameof(SelectionChanged), RoutingStrategy.Bubble, typeof(EnumSelectionChangedEventHandler), typeof(EnumDropdown));

        public delegate void EnumSelectionChangedEventHandler(object Sender, EnumSelectionChangedArgs E);

        public event EnumSelectionChangedEventHandler SelectionChanged {
            add => AddHandler(SelectionChangedEvent, value);
            remove => RemoveHandler(SelectionChangedEvent, value);
        }

        public Type EnumType {
            get => (Type)GetValue(EnumTypeProperty);
            set {
                SetValue(EnumTypeProperty, value);

                ComboControl.Items.Clear();
                foreach(object PossibleValue in Enum.GetValues(value)) {
                    ComboControl.Items.Add(PossibleValue);
                }

                _IgnoreNext = true;
                ComboControl.SelectedIndex = 0;
            }
        }

        public static readonly DependencyProperty EnumTypeProperty =
            DependencyProperty.Register(nameof(EnumType), typeof(Type), typeof(EnumDropdown), new PropertyMetadata(typeof(DefaultOptions)));

        public void SetValue<T>(T Value, bool RaiseEvents = false) where T : struct, Enum {
            if (!typeof(T).IsEnum) {
                throw new ArgumentException($"Value '{Value}' must be an enumerated type.");
            }

            EnumType = typeof(T);
            if (!RaiseEvents) { _IgnoreNext = true; }
            ComboControl.SelectedIndex = Convert.ToInt32(Value);
        }

        public object GetValue() => ComboControl.SelectedItem;

        public EnumDropdown() {
            InitializeComponent();
        }

        bool _IgnoreNext = false;
        void ComboControl_SelectionChanged(object Sender, SelectionChangedEventArgs E) {
            if (_IgnoreNext) { _IgnoreNext = false; return; }

            if (Sender is ComboBox CB) {
                try {
                    EnumSelectionChangedArgs Args = new EnumSelectionChangedArgs(SelectionChangedEvent, EnumType, GetInitialSelection(E.AddedItems));

                    CB.RaiseEvent(Args);
                } catch (TargetInvocationException) { }
            }
        }

        public static object GetInitialSelection(IList AddedItems) {
            foreach(object AddedItem in AddedItems) {
                return AddedItem;
            }

            return DefaultOptions.ParseError;
        }

        public enum DefaultOptions { ParseError }
    
    }

    public class EnumSelectionChangedArgs : RoutedEventArgs {

        public Type EnumType { get; protected set; }

        public object SelectedValue { get; protected set; }

        public EnumSelectionChangedArgs(RoutedEvent RoutedEvent, Type EnumType, object SelectedValue) : base(RoutedEvent) {
            this.EnumType = EnumType;
            this.SelectedValue = SelectedValue;
        }
    }
}
