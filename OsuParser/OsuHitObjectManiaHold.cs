#region Copyright (C) 2017-2020  Starflash Studios
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License (Version 3.0)
// as published by the Free Software Foundation.
// 
// More information can be found here: https://www.gnu.org/licenses/gpl-3.0.en.html
#endregion

#region Using Directives

using System;
using System.Linq;
using System.Collections.Generic;

#endregion

namespace Osu_BackgroundPurge.OsuParser {
    public class OsuHitObjectManiaHold : OsuHitObject, IEquatable<OsuHitObjectManiaHold> {
        /// <summary> End time of the hold, in milliseconds from the beginning of the beatmap's audio. </summary>
        public readonly int EndTime;

        public OsuHitObjectManiaHold(int EndTime = 1) => this.EndTime = EndTime;

        public OsuHitObjectManiaHold(int X = 0, int Y = 0, int Time = 0, OsuHitObjectType Type = OsuHitObjectType.HitCircle, OsuHitSound HitSound = OsuHitSound.Normal, string[] ObjectParams = null, OsuHitSample HitSample = default) : base(X, Y, Time, Type, HitSound, ObjectParams, HitSample) => EndTime = ObjectParams?.FirstOrDefault().ConvertToInt(1) ?? 1;

        /// <summary> X determines the index of the column that the hold will be in. It is computed by floor(x * <paramref name="ColumnCount"/> / 512) and clamped between 0 and <paramref name="ColumnCount"/> - 1. </summary>
        /// <remarks> Y does not affect holds. It defaults to the centre of the playfield, 192. </remarks>
        public int GetColumnIndex(int ColumnCount = 4) => (int)Clamp(Math.Floor(X * ColumnCount / 512M), 0M, ColumnCount - 1);

        /// <summary> Clamps the specified <paramref name="Val"/> between <paramref name="Min"/> and <paramref name="Max"/>. </summary>
        /// <param name="Val"> The value. </param>
        /// <param name="Min"> The minimum value. </param>
        /// <param name="Max"> The maximum value. </param>
        /// <returns></returns>
        public static decimal Clamp(decimal Val, decimal Min = 0M, decimal Max = 1M) => Val < Min ? Min : Val > Max ? Max : Val;

        public override string ToString() => $"({base.ToString().TrimEnd('(')}, EndTime: {EndTime})";

        #region Equality Members

        public override bool Equals(object Obj) => Equals(Obj as OsuHitObjectManiaHold);

        public override int GetHashCode() {
            unchecked { return(base.GetHashCode() * 397) ^ EndTime; }
        }

        public bool Equals(OsuHitObjectManiaHold Other) =>
            Other != null &&
            base.Equals(Other) &&
            X == Other.X &&
            Y == Other.Y &&
            Time == Other.Time &&
            Type == Other.Type &&
            HitSound == Other.HitSound &&
            EqualityComparer<string[]>.Default.Equals(ObjectParams, Other.ObjectParams) &&
            HitSample.Equals(Other.HitSample) &&
            EndTime == Other.EndTime;

        public static bool operator ==(OsuHitObjectManiaHold Left, OsuHitObjectManiaHold Right) => EqualityComparer<OsuHitObjectManiaHold>.Default.Equals(Left, Right);

        public static bool operator !=(OsuHitObjectManiaHold Left, OsuHitObjectManiaHold Right) => !(Left == Right);

        #endregion
    }
}