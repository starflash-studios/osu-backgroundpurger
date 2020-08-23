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
    [Flags] public enum OsuHitSound { Normal = 0, Whistle = 1, Finish = 2, Clap = 3 }
}