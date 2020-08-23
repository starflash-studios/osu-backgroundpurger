#region Copyright (C) 2017-2020  Starflash Studios
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License (Version 3.0)
// as published by the Free Software Foundation.
// 
// More information can be found here: https://www.gnu.org/licenses/gpl-3.0.en.html
#endregion

#region Using Directives

using System;
using System.Windows;
using System.Windows.Controls;

#endregion

namespace Osu_BackgroundPurge.UserControls {
    public class BottomDockingPanel : DockPanel {
        protected override Size ArrangeOverride(Size ArrangeSize) {
            UIElementCollection ChildrenColl = InternalChildren;
            int TotalChildrenCount = ChildrenColl.Count;

            double AccumulatedBottom = 0;

            for (int I = TotalChildrenCount - 1; I >= 0; --I) {
                UIElement Child = ChildrenColl[I];
                if (Child == null) { continue; }

                Size ChildDesiredSize = Child.DesiredSize;
                Rect RcChild = new Rect(
                    0,
                    0,
                    Math.Max(0.0, ArrangeSize.Width - (0 + (double)0)),
                    Math.Max(0.0, ArrangeSize.Height - (0 + AccumulatedBottom)));

                if (I > 0) {
                    AccumulatedBottom += ChildDesiredSize.Height;
                    RcChild.Y = Math.Max(0.0, ArrangeSize.Height - AccumulatedBottom);
                    RcChild.Height = ChildDesiredSize.Height;
                }

                Child.Arrange(RcChild);
            }

            return ArrangeSize;
        }
    }
}