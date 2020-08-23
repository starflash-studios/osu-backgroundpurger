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
    public class OsuHitObjectSpinner : OsuHitObject, IEquatable<OsuHitObjectSpinner> {
        /// <summary> End time of the spinner, in milliseconds from the beginning of the beatmap's audio. </summary>
        public readonly int EndTime;
        /// <summary> X and Y do not affect spinners. They default to the centre of the playfield, 256,192. </summary>
        public const int CenterX = 256;
        /// <summary> <inheritdoc cref="CenterX"/> </summary>
        public const int CentreX = 256;
        /// <summary> <inheritdoc cref="CenterX"/> </summary>
        public const int CenterY = 192;
        /// <summary> <inheritdoc cref="CenterX"/> </summary>
        public const int CentreY = 192;

        public OsuHitObjectSpinner(int EndTime = 1) => this.EndTime = EndTime;

        public OsuHitObjectSpinner(int X = 0, int Y = 0, int Time = 0, OsuHitObjectType Type = OsuHitObjectType.HitCircle, OsuHitSound HitSound = OsuHitSound.Normal, string[] ObjectParams = null, OsuHitSample HitSample = default) : base(X, Y, Time, Type, HitSound, ObjectParams, HitSample) => EndTime = ObjectParams?.FirstOrDefault().ConvertToInt(1) ?? 1;

        public override string ToString() => $"({base.ToString().TrimEnd('(')}, EndTime: {EndTime})";

        #region Equality Members

        public override bool Equals(object Obj) => Equals(Obj as OsuHitObjectSpinner);

        public override int GetHashCode() {
            unchecked { return(base.GetHashCode() * 397) ^ EndTime; }
        }

        public bool Equals(OsuHitObjectSpinner Other) =>
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

        public static bool operator ==(OsuHitObjectSpinner Left, OsuHitObjectSpinner Right) => EqualityComparer<OsuHitObjectSpinner>.Default.Equals(Left, Right);

        public static bool operator !=(OsuHitObjectSpinner Left, OsuHitObjectSpinner Right) => !(Left == Right);

        #endregion
    }
}