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
    public readonly struct OsuHitSample : IEquatable<OsuHitSample> {
        /// <summary> Sample set of the normal sound. </summary>
        /// <remarks> normalSet and additionSet can be any of the following:
        /// 0: No custom sample set
        ///     For normal sounds, the set is determined by the timing point's sample set.
        ///     For additions, the set is determined by the normal sound's sample set.
        /// 1: Normal set
        /// 2: Soft set
        /// 3: Drum set</remarks>
        public readonly int NormalSet;
        /// <summary> Sample set of the whistle, finish, and clap sounds. </summary>
        public readonly int AdditionSet;
        /// <summary> Index of the sample. If this is 0, the timing point's sample index will be used instead. </summary>
        public readonly int Index;
        /// <summary> Volume of the sample from 1 to 100. If this is 0, the timing point's volume will be used instead. </summary>
        public readonly int Volume;
        /// <summary> Custom filename of the addition sound. </summary>
        public readonly RelativeFileInfo FileName;

        public OsuHitSample(int NormalSet = 0, int AdditionSet = 0, int Index = 0, int Volume = 100, RelativeFileInfo FileName = null) {
            this.NormalSet = NormalSet;
            this.AdditionSet = AdditionSet;
            this.Index = Index;
            this.Volume = Volume;
            this.FileName = FileName;
        }

        public override string ToString() => $"(NormalSet: {NormalSet}, AdditionSet: {AdditionSet}, Index: {Index}, Volume: {Volume}, FileName: {FileName})";

        #region Equality Members

        public override bool Equals(object Obj) => Obj is OsuHitSample Sample && Equals(Sample);

        public override int GetHashCode() {
            unchecked {
                int HashCode = NormalSet;
                HashCode = (HashCode * 397) ^ AdditionSet;
                HashCode = (HashCode * 397) ^ Index;
                HashCode = (HashCode * 397) ^ Volume;
                HashCode = (HashCode * 397) ^ (FileName != null ? FileName.GetHashCode() : 0);
                return HashCode;
            }
        }

        public bool Equals(OsuHitSample Other) =>
            NormalSet == Other.NormalSet &&
            AdditionSet == Other.AdditionSet &&
            Index == Other.Index &&
            Volume == Other.Volume &&
            EqualityComparer<RelativeFileInfo>.Default.Equals(FileName, Other.FileName);

        public static bool operator ==(OsuHitSample Left, OsuHitSample Right) => Left.Equals(Right);

        public static bool operator !=(OsuHitSample Left, OsuHitSample Right) => !(Left == Right);

        #endregion
    }
}