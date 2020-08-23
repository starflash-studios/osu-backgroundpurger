#region Copyright (C) 2017-2020  Starflash Studios
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License (Version 3.0)
// as published by the Free Software Foundation.
// 
// More information can be found here: https://www.gnu.org/licenses/gpl-3.0.en.html
#endregion

#region Using Directives

using System;

#endregion

namespace Osu_BackgroundPurge.OsuParser {
    public readonly struct OsuTimingPoint : IEquatable<OsuTimingPoint> {
        /// <summary> Start time of the timing section, in milliseconds from the beginning of the beatmap's audio. </summary>
        /// <remarks> The end of the timing section is the next timing point's time (or never, if this is the last timing point). </remarks>
        public readonly int Time;
        /// <summary> This property has two meanings:
        /// For uninherited timing points, the duration of a beat, in milliseconds.
        /// For inherited timing points, a negative inverse slider velocity multiplier, as a percentage. For example, -50 would make all sliders in this timing section twice as fast as SliderMultiplier. </summary>
        public readonly decimal BeatLength;
        /// <summary> Amount of beats in a measure. </summary>
        /// <remarks> Inherited timing points ignore this property. </remarks>
        public readonly int Meter;
        /// <summary> <inheritdoc cref="OsuSampleSet"/> </summary>
        public readonly OsuSampleSet SampleSet;
        /// <summary> Custom sample index for hit objects. 0 indicates osu!'s default hitsounds. </summary>
        public readonly int SampleIndex;
        /// <summary> Volume percentage for hit objects. </summary>
        public readonly int Volume;
        /// <summary> Whether or not the timing point is uninherited. </summary>
        public readonly bool Uninherited;
        /// <summary> Bit flags that give the timing point extra effects. </summary>
        /// <remarks> See the effects section. <see href="https://osu.ppy.sh/help/wiki/osu!_File_Formats/Osu_(file_format)#effects"/> </remarks>
        public readonly OsuEffects Effects;

        public OsuTimingPoint(int Time, decimal BeatLength, int Meter, OsuSampleSet SampleSet, int SampleIndex, int Volume, bool Uninherited, OsuEffects Effects) {
            this.Time = Time;
            this.BeatLength = BeatLength;
            this.Meter = Meter;
            this.SampleSet = SampleSet;
            this.SampleIndex = SampleIndex;
            this.Volume = Volume;
            this.Uninherited = Uninherited;
            this.Effects = Effects;
        }

        //public static string SummariseFlags(OsuEffects Flags) => (from OsuEffects Effect in Enum.GetValues(typeof(OsuEffects)) where Flags.HasFlag(Effect) select Effect.ToString()).ToList().GetListString();

        public override string ToString() => $"(Time: {Time}, BeatLength: {BeatLength}, Meter: {Meter}, SampleSet: {SampleSet}, SampleIndex: {SampleIndex}, Volume: {Volume}, Uninherited: {Uninherited}, Effects: {Effects.SummariseFlags()})";

        #region Equality Members

        public override bool Equals(object Obj) => Obj is OsuTimingPoint Point && Equals(Point);

        public override int GetHashCode() {
            unchecked {
                int HashCode = Time;
                HashCode = (HashCode * 397) ^ BeatLength.GetHashCode();
                HashCode = (HashCode * 397) ^ Meter;
                HashCode = (HashCode * 397) ^ (int)SampleSet;
                HashCode = (HashCode * 397) ^ SampleIndex;
                HashCode = (HashCode * 397) ^ Volume;
                HashCode = (HashCode * 397) ^ Uninherited.GetHashCode();
                HashCode = (HashCode * 397) ^ (int)Effects;
                return HashCode;
            }
        }

        public bool Equals(OsuTimingPoint Other) =>
            Time == Other.Time &&
            BeatLength == Other.BeatLength &&
            Meter == Other.Meter &&
            SampleSet == Other.SampleSet &&
            SampleIndex == Other.SampleIndex &&
            Volume == Other.Volume &&
            Uninherited == Other.Uninherited &&
            Effects == Other.Effects;

        public static bool operator ==(OsuTimingPoint Left, OsuTimingPoint Right) => Left.Equals(Right);

        public static bool operator !=(OsuTimingPoint Left, OsuTimingPoint Right) => !(Left == Right);

        #endregion
    }
}