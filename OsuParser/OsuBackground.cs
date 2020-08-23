#region Copyright (C) 2017-2020  Starflash Studios
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License (Version 3.0)
// as published by the Free Software Foundation.
// 
// More information can be found here: https://www.gnu.org/licenses/gpl-3.0.en.html
#endregion

#region Using Directives

using System;
using System.Collections.Generic;

#endregion

namespace Osu_BackgroundPurge.OsuParser {
    public class OsuBackground : OsuEvent, IEquatable<OsuBackground> {
        /// <summary> Location of the background image relative to the beatmap directory. </summary>
        /// <remarks> Double quotes are usually included surrounding the filename, but they are not required. </remarks>
        public RelativeFileInfo FileName;
        /// <summary> Offset in osu! pixels from the center of the screen. For example, an offset of 50,100 would have the background shown 50 osu! pixels to the right and 100 osu! pixels down from the center of the screen. If the offset is 0,0, writing it is optional. </summary>
        public int XOffset;
        /// <summary> <inheritdoc cref="XOffset"/> </summary>
        public int YOffset;

        public OsuBackground(OsuEventType EventType = OsuEventType.Background, int StartTime = 0, string[] EventParams = default) : base(EventType, StartTime, EventParams) {
            if (EventParams.Length > 0) {
                FileName = EventParams[0].ConvertToRelativeFileInfo();
            }
            if (EventParams.Length > 1) {
                XOffset = EventParams[1].ConvertToInt(0);
            }
            if (EventParams.Length > 2) {
                YOffset = EventParams[2].ConvertToInt(0);
            }
        }

        public override string ToString() => $"(FileName: {FileName}, XOffset: {XOffset}, YOffset: {YOffset})";

        #region Equality Members

        public override bool Equals(object Obj) => Obj is OsuBackground Background && Equals(Background);

        public override int GetHashCode() {
            // ReSharper disable NonReadonlyMemberInGetHashCode
            unchecked {
                int HashCode = FileName != null ? FileName.GetHashCode() : 0;
                HashCode = (HashCode * 397) ^ XOffset;
                HashCode = (HashCode * 397) ^ YOffset;
                return HashCode;
            }
            // ReSharper restore NonReadonlyMemberInGetHashCode
        }

        public bool Equals(OsuBackground Other) =>
            EqualityComparer<RelativeFileInfo>.Default.Equals(FileName, Other.FileName) &&
            XOffset == Other.XOffset &&
            YOffset == Other.YOffset;

        public static bool operator ==(OsuBackground Left, OsuBackground Right) => Left.Equals(Right);

        public static bool operator !=(OsuBackground Left, OsuBackground Right) => !(Left == Right);

        #endregion
    }
}