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

    /// <summary>A <see cref="System.Windows.Controls.UserControl"/> allowing the user to select a <see cref="Enum"/> option.</summary>
    /// <seealso cref="UserControl" />
    /// <seealso cref="System.Windows.Markup.IComponentConnector" />
    public partial class EnumDropdown {

        /// <summary>The selection changed event</summary>
        public static readonly RoutedEvent SelectionChangedEvent = EventManager.RegisterRoutedEvent(
       nameof(SelectionChanged), RoutingStrategy.Bubble, typeof(EnumSelectionChangedEventHandler), typeof(EnumDropdown));

        /// <summary>Occurs when the selected enum option changes.</summary>
        /// <param name="Sender">The sender.</param>
        /// <param name="E">The <see cref="EnumSelectionChangedArgs"/>.</param>
        public delegate void EnumSelectionChangedEventHandler(object Sender, EnumSelectionChangedArgs E);

        /// <summary>Occurs when the selected enum option changes.</summary>
        public event EnumSelectionChangedEventHandler SelectionChanged {
            add => AddHandler(SelectionChangedEvent, value);
            remove => RemoveHandler(SelectionChangedEvent, value);
        }

        /// <summary>Gets or sets the type of the enum.</summary>
        /// <value>The type of the enum.</value>
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

        /// <summary>A <see cref="DependencyProperty"/> responsible for handling event/bindings for <see cref="EnumType"/>.</summary>
        public static readonly DependencyProperty EnumTypeProperty =
            DependencyProperty.Register(nameof(EnumType), typeof(Type), typeof(EnumDropdown), new PropertyMetadata(typeof(DefaultOptions)));

        /// <summary>Sets the selected enum value.</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Value">The value.</param>
        /// <param name="RaiseEvents">if set to <c>true</c> [raise events].</param>
        /// <exception cref="ArgumentException">Value '{Value}' must be an enumerated type.</exception>
        public void SetValue<T>(T Value, bool RaiseEvents = false) where T : struct, Enum {
            if (!typeof(T).IsEnum) {
                throw new ArgumentException($"Value '{Value}' must be an enumerated type.");
            }

            EnumType = typeof(T);
            if (!RaiseEvents) { _IgnoreNext = true; }
            ComboControl.SelectedIndex = Convert.ToInt32(Value);
        }

        /// <summary>Gets the selected enum value.</summary>
        /// <returns><see cref="object"/></returns>
        public object GetValue() => ComboControl.SelectedItem;

        /// <summary>Initialises a new instance of the <see cref="EnumDropdown"/> class.</summary>
        public EnumDropdown() {
            InitializeComponent();
        }

        /// <summary>Whether or not the next <c>SelectionChanged</c> event should be ignored.</summary>
        bool _IgnoreNext = false;

        /// <summary>Handles the <c>SelectionChanged</c> event of the <see cref="ComboControl"/> control.</summary>
        /// <param name="Sender">The source of the event.</param>
        /// <param name="E">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
        void ComboControl_SelectionChanged(object Sender, SelectionChangedEventArgs E) {
            if (_IgnoreNext) { _IgnoreNext = false; return; }

            if (Sender is ComboBox CB) {
                try {
                    EnumSelectionChangedArgs Args = new EnumSelectionChangedArgs(SelectionChangedEvent, EnumType, GetInitialSelection(E.AddedItems));

                    CB.RaiseEvent(Args);
                } catch (TargetInvocationException) { }
            }
        }

        /// <summary>Gets the initial selection.</summary>
        /// <param name="AddedItems">The added items.</param>
        /// <returns><see cref="object"/></returns>
        public static object GetInitialSelection(IList AddedItems) {
            foreach(object AddedItem in AddedItems) {
                return AddedItem;
            }

            return DefaultOptions.ParseError;
        }

        /// <summary>The default display enum.</summary>
        public enum DefaultOptions {
            /// <summary>Represents an error caused during enum parsing.</summary>
            ParseError
        }
    
    }

    /// <summary>Provides data for the <see cref="EnumDropdown.SelectionChanged"/> event. </summary>
    /// <seealso cref="System.Windows.RoutedEventArgs" />
    public class EnumSelectionChangedArgs : RoutedEventArgs {

        /// <summary>Gets or sets the type of the enum.</summary>
        /// <value>The type of the enum.</value>
        public Type EnumType { get; protected set; }

        /// <summary>Gets or sets the selected value.</summary>
        /// <value>The selected value <see cref="Object"/>.</value>
        public object SelectedValue { get; protected set; }

        /// <summary>Initialises a new instance of the <see cref="EnumSelectionChangedArgs"/> class.</summary>
        /// <param name="RoutedEvent">The routed event.</param>
        /// <param name="EnumType">Type of the enum.</param>
        /// <param name="SelectedValue">The selected value.</param>
        public EnumSelectionChangedArgs(RoutedEvent RoutedEvent, Type EnumType, object SelectedValue) : base(RoutedEvent) {
            this.EnumType = EnumType;
            this.SelectedValue = SelectedValue;
        }
    }
}
