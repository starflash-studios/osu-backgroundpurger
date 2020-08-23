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
    public abstract class OsuEvent : IEquatable<OsuEvent> {
        /// <summary> Type of the event. </summary>
        /// <remarks> Some events may be referred to by either a name or a number. </remarks>
        public readonly OsuEventType EventType;
        /// <summary> Start time of the event, in milliseconds from the beginning of the beatmap's audio.
        /// For events that do not use a start time, the default is 0. </summary>
        public readonly int StartTime;
        /// <summary> Extra parameters specific to the event's type. </summary>
        public readonly string[] EventParams;

        public enum OsuEventType {
            Background = 0,
            Video = 1,
            Break = 2,
            Colour = 3,
            Unknown = -1
        }

        protected OsuEvent(OsuEventType EventType = OsuEventType.Background, int StartTime = 0, string[] EventParams = default) {
            this.EventType = EventType;
            this.StartTime = StartTime;
            this.EventParams = EventParams;
        }

        public override string ToString() => $"(EventType: {EventType}, StartTime: {StartTime}, EventParams: {((EventParams?.Length ?? -1) > 0 ? EventParams.GetListedString() : "")})";

        #region Equality Members

        public override bool Equals(object Obj) => Equals(Obj as OsuEvent);

        public override int GetHashCode() {
            unchecked {
                int HashCode = (int)EventType;
                HashCode = (HashCode * 397) ^ StartTime;
                HashCode = (HashCode * 397) ^ (EventParams != null ? EventParams.GetHashCode() : 0);
                return HashCode;
            }
        }

        public bool Equals(OsuEvent Other) =>
            Other != null &&
            EqualityComparer<OsuEventType>.Default.Equals(EventType, Other.EventType) &&
            EqualityComparer<int>.Default.Equals(StartTime, Other.StartTime) &&
            EqualityComparer<string[]>.Default.Equals(EventParams, Other.EventParams);

        public static bool operator ==(OsuEvent Left, OsuEvent Right) => EqualityComparer<OsuEvent>.Default.Equals(Left, Right);

        public static bool operator !=(OsuEvent Left, OsuEvent Right) => !(Left == Right);

        #endregion
    }
}