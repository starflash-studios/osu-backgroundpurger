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
    public class OsuHitObjectSlider : OsuHitObject, IEquatable<OsuHitObjectSlider> {

        /// <summary> Type of curve used to construct this slider. </summary>
        /// <remarks> (B = bézier, C = centripetal catmull-rom, L = linear, P = perfect circle). </remarks>
        public readonly SliderCurveType CurveType;
        /// <summary> Points used to construct the slider. Each point is in the format x:y. </summary>
        public readonly (int X, int Y)[] CurvePoints;
        /// <summary> Amount of times the player has to follow the slider's curve back-and-forth before the slider is complete. It can also be interpreted as the repeat count plus one. </summary>
        public readonly int Slides;
        /// <summary> Visual length in osu! pixels of the slider. </summary>
        public readonly decimal Length;
        /// <summary> Hitsounds that play when hitting edges of the slider's curve. The first sound is the one that plays when the slider is first clicked, and the last sound is the one that plays when the slider's end is hit. </summary>
        public readonly OsuHitSound[] EdgeSounds;
        /// <summary> Sample sets used for the edgeSounds. Each set is in the format normalSet:additionSet, with the same meaning as in the hitsounds section. </summary>
        public readonly OsuSampleSet[] EdgeSets;

        public OsuHitObjectSlider(SliderCurveType CurveType, (int X, int Y)[] CurvePoints, int Slides, decimal Length, OsuHitSound[] EdgeSounds, OsuSampleSet[] EdgeSets) {
            this.CurveType = CurveType;
            this.CurvePoints = CurvePoints;
            this.Slides = Slides;
            this.Length = Length;
            this.EdgeSounds = EdgeSounds;
            this.EdgeSets = EdgeSets;
        }

        public OsuHitObjectSlider(int X = 0, int Y = 0, int Time = 0, OsuHitObjectType Type = OsuHitObjectType.HitCircle, OsuHitSound HitSound = OsuHitSound.Normal, string[] ObjectParams = null, OsuHitSample HitSample = default) : base(X, Y, Time, Type, HitSound, ObjectParams, HitSample) {
            for (int I = 0; I < ObjectParams.Length; I++) {
                switch (I) {
                    case 0: //CurveType|CurvePoints
                        List<string> CurveValues = ObjectParams[I].SeparatePipeList(StringSplitOptions.RemoveEmptyEntries).ToList();
                        List<(int X, int Y)> NewCurvePoints = new List<(int X, int Y)>();

                        int C = 0;
                        foreach(string CurveValue in CurveValues) {
                            switch (C) {
                                case 0: //CurveType
                                    CurveType = CurveValue.ConvertToEnum<SliderCurveType>();
                                    break;
                                default: //New CurvePoint
                                    NewCurvePoints.Add(CurveValue.ConvertToPoint((0, 0), StringSplitOptions.RemoveEmptyEntries));
                                    break;
                            }
                            C++;
                        }

                        CurvePoints = NewCurvePoints.ToArray();

                        break;
                    case 1: //Slides
                        Slides = ObjectParams[I].ConvertToInt(1);
                        break;
                    case 2: //Length
                        Length = ObjectParams[I].ConvertToDecimal(1);
                        break;
                    case 3: //EdgeSounds
                        EdgeSounds = ObjectParams[I].SeparatePipeList(StringSplitOptions.RemoveEmptyEntries).Select(E => E.ConvertToEnum<OsuHitSound>()).ToArray();
                        break;
                    case 4: //EdgeSets
                        EdgeSets = ObjectParams[I].SeparatePipeList(StringSplitOptions.RemoveEmptyEntries).Select(Es => Es.ConvertToEnum<OsuSampleSet>()).ToArray();
                        break;
                }
            }
        }

        public OsuHitSound GetFirstEdgeSound() => EdgeSounds?.FirstOrDefault() ?? default;
        public OsuSampleSet GetFirstEdgeSet() => EdgeSets?.FirstOrDefault() ?? default;
        public OsuHitSound GetLastEdgeSound() => EdgeSounds?.LastOrDefault() ?? default;
        public OsuSampleSet GetLastEdgeSet() => EdgeSets?.LastOrDefault() ?? default;

        public static IEnumerable<string> GetPointsString((int X, int Y)[] Points) {
            if (Points == null) { yield break; }
            foreach((int X, int Y) in Points) {
                yield return $"(X: {X}, Y: {Y})";
            }
        }

        public override string ToString() => $"({base.ToString().TrimEnd('(')}, CurveType: {CurveType}, CurvePoints: {GetPointsString(CurvePoints).GetListedString()}, Slides: {Slides}, Length: {Length}, EdgeSounds: {EdgeSounds.GetListedString()}, EdgeSets: {EdgeSets.GetListedString()})";

        #region Equality Members

        public override bool Equals(object Obj) => Equals(Obj as OsuHitObjectSlider);

        public override int GetHashCode() {
            unchecked {
                int HashCode = base.GetHashCode();
                HashCode = (HashCode * 397) ^ (int)CurveType;
                HashCode = (HashCode * 397) ^ (CurvePoints != null ? CurvePoints.GetHashCode() : 0);
                HashCode = (HashCode * 397) ^ Slides;
                HashCode = (HashCode * 397) ^ Length.GetHashCode();
                HashCode = (HashCode * 397) ^ (EdgeSounds != null ? EdgeSounds.GetHashCode() : 0);
                HashCode = (HashCode * 397) ^ (EdgeSets != null ? EdgeSets.GetHashCode() : 0);
                return HashCode;
            }
        }

        public bool Equals(OsuHitObjectSlider Other) =>
            Other != null &&
            base.Equals(Other) &&
            X == Other.X &&
            Y == Other.Y &&
            Time == Other.Time &&
            Type == Other.Type &&
            HitSound == Other.HitSound &&
            EqualityComparer<string[]>.Default.Equals(ObjectParams, Other.ObjectParams) &&
            HitSample.Equals(Other.HitSample) &&
            CurveType == Other.CurveType &&
            EqualityComparer<(int X, int Y)[]>.Default.Equals(CurvePoints, Other.CurvePoints) &&
            Slides == Other.Slides &&
            Length == Other.Length &&
            EqualityComparer<OsuHitSound[]>.Default.Equals(EdgeSounds, Other.EdgeSounds) &&
            EqualityComparer<OsuSampleSet[]>.Default.Equals(EdgeSets, Other.EdgeSets);

        public enum SliderCurveType {
            Bezier,
            CentripetalCatmullRom,
            Linear,
            PerfectCircle
        }

        public static bool operator ==(OsuHitObjectSlider Left, OsuHitObjectSlider Right) => EqualityComparer<OsuHitObjectSlider>.Default.Equals(Left, Right);

        public static bool operator !=(OsuHitObjectSlider Left, OsuHitObjectSlider Right) => !(Left == Right);

        #endregion
    }
}