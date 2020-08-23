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
    public class OsuBreak : OsuEvent, IEquatable<OsuBreak> {
        /// <summary> End time of the break, in milliseconds from the beginning of the beatmap's audio. </summary>
        public int EndTime;

        public OsuBreak(OsuEventType EventType = OsuEventType.Video, int StartTime = 0, string[] EventParams = default) : base(EventType, StartTime, EventParams) {
            if (EventParams.Length > 0) {
                EndTime = int.TryParse(EventParams[0], out int NewEndTime) ? NewEndTime : StartTime + 1;
            }
        }

        public override string ToString() => $"(EventType: {EventType}, StartTime: {StartTime}, EndTime: {EndTime})";

        #region Equality Members

        public override bool Equals(object Obj) => Equals(Obj as OsuBreak);

        public override int GetHashCode() {
            // ReSharper disable NonReadonlyMemberInGetHashCode
            unchecked { return(base.GetHashCode() * 397) ^ EndTime; }
            // ReSharper restore NonReadonlyMemberInGetHashCode
        }

        public bool Equals(OsuBreak Other) =>
            Other != null &&
            base.Equals(Other) &&
            EventType == Other.EventType &&
            StartTime == Other.StartTime &&
            EqualityComparer<string[]>.Default.Equals(EventParams, Other.EventParams) &&
            EndTime == Other.EndTime;

        public static bool operator ==(OsuBreak Left, OsuBreak Right) => EqualityComparer<OsuBreak>.Default.Equals(Left, Right);

        public static bool operator !=(OsuBreak Left, OsuBreak Right) => !(Left == Right);

        #endregion
    }
}