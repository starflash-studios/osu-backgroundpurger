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
    public class OsuBackgroundVideo : OsuBackground, IEquatable<OsuBackgroundVideo> {

        public OsuBackgroundVideo(OsuEventType EventType = OsuEventType.Video, int StartTime = 0, string[] EventParams = default) : base(EventType, StartTime, EventParams) {
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

        #region Equality Members

        public override bool Equals(object Obj) => Equals(Obj as OsuBackgroundVideo);

        public override int GetHashCode() {
            // ReSharper disable NonReadonlyMemberInGetHashCode
            unchecked {
                int HashCode = base.GetHashCode();
                HashCode = (HashCode * 397) ^ (FileName != null ? FileName.GetHashCode() : 0);
                HashCode = (HashCode * 397) ^ XOffset;
                HashCode = (HashCode * 397) ^ YOffset;
                return HashCode;
            }
            // ReSharper restore NonReadonlyMemberInGetHashCode
        }

        public bool Equals(OsuBackgroundVideo Other) =>
            Other != null &&
            base.Equals(Other) &&
            EventType == Other.EventType &&
            StartTime == Other.StartTime &&
            EqualityComparer<string[]>.Default.Equals(EventParams, Other.EventParams) &&
            EqualityComparer<RelativeFileInfo>.Default.Equals(FileName, Other.FileName) &&
            XOffset == Other.XOffset &&
            YOffset == Other.YOffset;

        public override string ToString() => $"(EventType: {EventType}, StartTime: {StartTime}, FileName: {FileName}, XOffset: {XOffset}, YOffset: {YOffset})";

        public static bool operator ==(OsuBackgroundVideo Left, OsuBackgroundVideo Right) => EqualityComparer<OsuBackgroundVideo>.Default.Equals(Left, Right);

        public static bool operator !=(OsuBackgroundVideo Left, OsuBackgroundVideo Right) => !(Left == Right);

        #endregion
    }
}