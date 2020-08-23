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
    public class OsuHitObject : IEquatable<OsuHitObject> {
        /// <summary> Position in osu! pixels of the object. </summary>
        public readonly int X;
        /// <summary> Position in osu! pixels of the object. </summary>
        public readonly int Y;
        /// <summary> Time when the object is to be hit, in milliseconds from the beginning of the beatmap's audio. </summary>
        public readonly int Time;
        /// <summary> Bit flags indicating the type of the object. </summary>
        public readonly OsuHitObjectType Type;
        /// <summary> Bit flags indicating the hitsound applied to the object. </summary>
        public readonly OsuHitSound HitSound;
        /// <summary> Extra parameters specific to the object's type.</summary>
        public readonly string[] ObjectParams;
        /// <summary> Information about which samples are played when the object is hit. It is closely related to hitSound. If it is not written, it defaults to 0:0:0:0:. </summary>
        public readonly OsuHitSample HitSample;

        public OsuHitObject(int X = 0, int Y = 0, int Time = 0, OsuHitObjectType Type = default, OsuHitSound HitSound = default, string[] ObjectParams = default, OsuHitSample HitSample = default) {
            this.X = X;
            this.Y = Y;
            this.Time = Time;
            this.Type = Type;
            this.HitSound = HitSound;
            this.ObjectParams = ObjectParams;
            this.HitSample = HitSample;
        }

        public override string ToString() => $"(X: {X}, Y: {Y}, Time: {Time}, Type: {Type}, HitSound: {HitSound}, ObjectParams: {ObjectParams.GetListedString()}, HitSample: {HitSample})";

        #region Equality Members

        public override bool Equals(object Obj) => Equals(Obj as OsuHitObject);

        public bool Equals(OsuHitObject Other) =>
            Other != null &&
            X == Other.X &&
            Y == Other.Y &&
            Time == Other.Time &&
            Type == Other.Type &&
            HitSound == Other.HitSound &&
            EqualityComparer<string[]>.Default.Equals(ObjectParams, Other.ObjectParams) &&
            HitSample.Equals(Other.HitSample);

        public override int GetHashCode() {
            int HashCode = -215512371;
            HashCode = HashCode * -1521134295 + X.GetHashCode();
            HashCode = HashCode * -1521134295 + Y.GetHashCode();
            HashCode = HashCode * -1521134295 + Time.GetHashCode();
            HashCode = HashCode * -1521134295 + Type.GetHashCode();
            HashCode = HashCode * -1521134295 + HitSound.GetHashCode();
            HashCode = HashCode * -1521134295 + EqualityComparer<string[]>.Default.GetHashCode(ObjectParams);
            HashCode = HashCode * -1521134295 + HitSample.GetHashCode();
            return HashCode;
        }

        public static bool operator ==(OsuHitObject Left, OsuHitObject Right) => EqualityComparer<OsuHitObject>.Default.Equals(Left, Right);

        public static bool operator !=(OsuHitObject Left, OsuHitObject Right) => !(Left == Right);

        #endregion
    }
}