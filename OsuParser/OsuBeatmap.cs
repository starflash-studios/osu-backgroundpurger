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
using System.Diagnostics;
using System.Security.Policy;

#endregion

namespace Osu_BackgroundPurge.OsuParser {
    /// <summary>
    /// Represents the Osu! '.osu' beatmap format.
    /// </summary>
    /// <remarks>
    /// See: <see href="https://osu.ppy.sh/help/wiki/osu!_File_Formats/Osu_(file_format)">https://osu.ppy.sh/help/wiki/osu!_File_Formats/Osu_(file_format)</see> for more information.
    /// </remarks>
    public class OsuBeatmap {
        #region Fields

        // ReSharper disable MemberInitializerValueIgnored

        #region General

        /// <summary> Location of the audio file relative to the current folder. </summary>
        public RelativeFileInfo AudioFileName;
        /// <summary> Milliseconds of silence before the audio starts playing. </summary>
        public int AudioLeadIn = 0;
        /// <summary> Deprecated. </summary>
        [Obsolete] public Hash AudioHash;
        /// <summary> Time in milliseconds when the audio preview should start. </summary>
        public int PreviewTime = -1;
        /// <summary> Speed of the countdown before the first hit object. </summary>
        /// <remarks> (0 = no countdown, 1 = normal, 2 = half, 3 = double). </remarks>
        public OsuCountdown Countdown = OsuCountdown.Normal;
        /// <summary> Sample set that will be used if timing points do not override it. </summary>
        /// <remarks> (Normal, Soft, Drum) </remarks>
        public OsuSampleSet SampleSet = OsuSampleSet.Normal;
        /// <summary> Multiplier for the threshold in time where hit objects placed close together stack. </summary>
        /// <remarks> (0-1) </remarks>
        public decimal StackLeniency = 0.7M;
        /// <summary> Game mode. </summary>
        /// <remarks> (0 = osu!, 1 = osu!taiko, 2 = osu!catch, 3 = osu!mania) </remarks>
        public OsuGameMode Mode = OsuGameMode.Osu;
        /// <summary> Whether or not breaks have a letter-boxing effect. </summary>
        public bool LetterboxInBreaks = false;
        /// <summary> Deprecated. </summary>
        [Obsolete] public bool StoryFireInFront = true;
        /// <summary> Whether or not the storyboard can use the user's skin images. </summary>
        public bool UseSkinSprites = true;
        /// <summary> Deprecated. </summary>
        [Obsolete] public bool AlwaysShowPlayfield = false;
        /// <summary> Draw order of hit circle overlays compared to hit numbers. </summary>
        /// <remarks> (NoChange = use skin setting, Below = draw overlays under numbers, Above = draw overlays on top of numbers) </remarks>
        public OsuDrawPosition OverlayPosition = OsuDrawPosition.NoChange;
        /// <summary> Preferred skin to use during gameplay. </summary>
        public string SkinPreference;
        /// <summary> Whether or not a warning about flashing colours should be shown at the beginning of the map. </summary>
        public bool EpilepsyWarning = false;
        /// <summary> Time in beats that the countdown starts before the first hit object. </summary>
        public int CountdownOffset = 0;
        /// <summary> Whether or not the "N+1" style key layout is used for osu!mania. </summary>
        public bool SpecialStyle = false;
        /// <summary> Whether or not the storyboard allows widescreen viewing. </summary>
        public bool WidescreenStoryboard = false;
        /// <summary> Whether or not sound samples will change rate when playing with speed-changing mods. </summary>
        public bool SamplesMatchPlaybackRate = false;

        #endregion

        #region Editor

        // These options are only relevant when opening maps in the beatmap editor.
        // They do not affect gameplay.

        /// <summary> Time in milliseconds of bookmarks. </summary>
        public int[] Bookmarks;
        /// <summary> Distance snap multiplier. </summary>
        public decimal DistanceSpacing;
        /// <summary> Beat snap divisor. </summary>
        public decimal BeatDivisor;
        /// <summary> Grid size. </summary>
        public int GridSize;
        /// <summary> Scale factor for the object timeline. </summary>
        public decimal TimelineZoom;

        #endregion

        #region Metadata

        /// <summary> Romanised song title. </summary>
        public string Title;
        /// <summary> Song title. </summary>
        public string TitleUnicode;
        /// <summary> Romanised song artist. </summary>
        public string Artist;
        /// <summary> Song artist. </summary>
        public string ArtistUnicode;
        /// <summary> Beatmap creator. </summary>
        public string Creator;
        /// <summary> Difficulty name. </summary>
        public string Version;
        /// <summary> Original media the song was produced for. </summary>
        public string Source;
        /// <summary> Search terms. </summary>
        public string[] Tags;
        /// <summary> Beatmap ID. </summary>
        public int BeatmapID;
        /// <summary> Beatmapset ID. </summary>
        public int BeatmapSetID;

        #endregion

        #region Difficulty

        /// <summary> HP setting (0–10). </summary>
        public decimal HPDrainRate;
        /// <summary> CS setting (0-10). </summary>
        public decimal CircleSize;
        /// <summary> OD setting (0-10). </summary>
        public decimal OverallDifficulty;
        /// <summary> AR setting (0-10). </summary>
        public decimal ApproachRate;
        /// <summary> Base slider velocity in hecto-osu! pixels per beat. </summary>
        /// <remarks> osupixel: The representation of one screen pixel when osu! is running in 640x480 resolution.
        /// osupixels are one of the main coordinate systems used in osu!, and apply to hit circle placement and
        /// storyboard screen coordinates (these pixels are scaled over a 4:3 ratio to fit your screen). </remarks>
        public decimal SliderMultiplier;
        /// <summary> Amount of slider ticks per beat. </summary>
        public decimal SliderTickRate;

        #endregion

        #region Events

        public OsuEvent[] Events;

        #endregion

        #region Storyboards

        // TODO Implement Storyboarding Support
        // For information about storyboard syntax, see Storyboard Scripting.
        // https://osu.ppy.sh/help/wiki/Storyboard_Scripting

        #endregion

        #region Timing Points

        public OsuTimingPoint[] TimingPoints;

        #endregion

        #region Colours

        // TODO Implement Colours Support
        // For information about colours syntax, see .osu (file format)#colours.
        // https://osu.ppy.sh/help/wiki/osu!_File_Formats/Osu_(file_format)#colours

        #endregion

        #region Hit Objects

        public OsuHitObject[] HitObjects;

        #endregion


        // ReSharper restore MemberInitializerValueIgnored

        #endregion

        #region Beatmap Parsing

        #region Normalisation

        public static IEnumerable<string> GetLowerInvariantEnum(IEnumerable<string> Enum) => Enum.Select(Value => Value.ToLowerInvariant());

        public static IEnumerable<string> NormaliseLines(IEnumerable<string> Enum) => from Value in Enum where !string.IsNullOrWhiteSpace(Value) select Value.ToLowerInvariant().Trim(' ');

        #endregion

        #region Section Parsing

        public static bool TryGetSection(string Value, out string Result) {
            if (Value.StartsWith("[") && Value.EndsWith("]")) {
                Result = Value.Substring(1, Value.Length - 2);
                return true;
            }
            Result = null;
            return false;
        }

        public static string GetSection(string Value) => TryGetSection(Value, out string Result) ? Result : null;

        #endregion

        #region Key/Value Separating

        public static IEnumerable<KeyValuePair<string, string>> GetKeyValuePairs(string[] Lines, string Delimeter = ":", StringComparison StringComparison = StringComparison.InvariantCultureIgnoreCase) {
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach(string Line in Lines) {
                int DIndex = Line.IndexOf(Delimeter, StringComparison);
                if (DIndex >= 0) {
                    string Key = Line.Substring(0, DIndex);
                    string Value = Line.Substring(DIndex + Delimeter.Length);

                    yield return new KeyValuePair<string, string>(Key, Value);
                }
            }
        }

        public static Dictionary<string, string> GetBindingDictionary(string[] Lines, string Delimeter = ":", StringComparison StringComparison = StringComparison.InvariantCultureIgnoreCase) => GetKeyValuePairs(Lines, Delimeter, StringComparison).ToDictionary(V => V.Key, V => V.Value);

        public static IEnumerable<(string Key, string Value)> GetValues(IEnumerable<string> Lines, string Delimeter = ":", StringComparison StringComparison = StringComparison.InvariantCultureIgnoreCase) {
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (string Line in Lines) {
                int DIndex = Line.IndexOf(Delimeter, StringComparison);
                if (DIndex >= 0) {
                    string Key = Line.Substring(0, DIndex);
                    string Value = Line.Substring(DIndex + Delimeter.Length);

                    yield return (Key, Value);
                }
            }
        }

        #endregion

        public static OsuBeatmap GetBeatmap(string[] Lines, bool ParseGeneralValues = true, bool ParseEditorValues = true, bool ParseMetadataValues = true, bool ParseDifficultyValues = true, bool ParseEvents = true, bool ParseTimingPoints = true, bool ParseHitObjects = true) {
            Lines = NormaliseLines(Lines).ToArray();

            List<string> GeneralValues = new List<string>();
            List<string> EditorValues = new List<string>();
            List<string> MetadataValues = new List<string>();
            List<string> DifficultyValues = new List<string>();
            List<string> EventValues = new List<string>();
            List<string> TimingPointValues = new List<string>();
            //List<string> ColourValues = new List<string>();
            List<string> HitObjectValues = new List<string>();

            string Section = null;
            // ReSharper disable once LoopCanBePartlyConvertedToQuery
            foreach(string Line in Lines) {
                if (Line.TrimStart(' ').StartsWith("//")) { continue; } //Ignore comments
                if (TryGetSection(Line, out string NewSection)) {
                    //Debug.WriteLine($"\tEntered new section {NewSection}...");
                    Section = NewSection;
                }

                switch (Section) {
                    case "general" when ParseGeneralValues:
                        if (Line.Contains(": ")) {
                            //Debug.WriteLine($"\t\tLine '{Line}' is of type 'General'");
                            GeneralValues.Add(Line);
                        }
                        break;
                    case "editor" when ParseEditorValues:
                        if (Line.Contains(": ")) {
                            //Debug.WriteLine($"\t\tLine '{Line}' is of type 'Editor'");
                            EditorValues.Add(Line);
                        }
                        break;
                    case "metadata" when ParseMetadataValues:
                        if (Line.Contains(":")) {
                            //Debug.WriteLine($"\t\tLine '{Line}' is of type 'Metadata'");
                            MetadataValues.Add(Line);
                        }
                        break;
                    case "difficulty" when ParseDifficultyValues:
                        if (Line.Contains(':')) {
                            //Debug.WriteLine($"\t\tLine '{Line}' is of type 'Difficulty'");
                            DifficultyValues.Add(Line);
                        }
                        break;
                    case "events" when ParseEvents:
                        if (Line.Contains(',')) {
                            //Debug.WriteLine($"\t\tLine '{Line}' is of type 'Events'");
                            EventValues.Add(Line);
                        }
                        break;
                    case "timingpoints" when ParseTimingPoints:
                        if (Line.Contains(',')) {
                            //Debug.WriteLine($"\t\tLine '{Line}' is of type 'TimingPoints'");
                            TimingPointValues.Add(Line);
                        }
                        break;
                    //case "colours": //TODO Implement Colours Support
                    //    if (Line.Contains(" : ")) {
                    //        ColourValues.Add(Line);
                    //    }
                    //    break;
                    case "hitobjects" when ParseHitObjects:
                        if (Line.Contains(',')) {
                            //Debug.WriteLine($"\t\tLine '{Line}' is of type 'HitObjects'");
                            HitObjectValues.Add(Line);
                        }
                        break;
                }
            }

            //Debug.WriteLine("\tParsing General Fields...");
            RelativeFileInfo AudioFileName = default;
            int AudioLeadIn = 0, CountdownOffset = 0, PreviewTime = -1;
            Hash AudioHash = default;
            OsuCountdown Countdown = OsuCountdown.Normal;
            OsuSampleSet SampleSet = OsuSampleSet.Normal;
            decimal StackLeniency = 0.7M;
            OsuGameMode Mode = OsuGameMode.Osu;
            bool LetterboxInBreaks = false, AlwaysShowPlayfield = false, EpilepsyWarning = false, SpecialStyle = false, WidescreenStoryboard = false, SamplesMatchPlaybackRate = false;
            bool StoryFireInFront = true, UseSkinSprites = true;
            OsuDrawPosition OverlayPosition = OsuDrawPosition.NoChange;
            string SkinPreference = default;

            if (ParseGeneralValues) {
                GetGeneralFields(GeneralValues, out AudioFileName, out AudioLeadIn, out AudioHash, out PreviewTime, out Countdown, out SampleSet, out StackLeniency, out Mode, out LetterboxInBreaks, out StoryFireInFront, out UseSkinSprites, out AlwaysShowPlayfield, out OverlayPosition, out SkinPreference, out EpilepsyWarning, out CountdownOffset, out SpecialStyle, out WidescreenStoryboard, out SamplesMatchPlaybackRate);
            }


            //Debug.WriteLine("\tParsing Editor Fields...");
            int[] Bookmarks = Array.Empty<int>();
            decimal DistanceSpacing = 0M, BeatDivisor = 0M, TimelineZoom = 0M;
            int GridSize = 0;

            if (ParseEditorValues) {
                GetEditorFields(EditorValues, out Bookmarks, out DistanceSpacing, out BeatDivisor, out GridSize, out TimelineZoom);
            }


            //Debug.WriteLine("\tParsing Metadata Fields...");
            string Title = string.Empty, TitleUnicode = string.Empty, Artist = string.Empty, ArtistUnicode = string.Empty, Creator = string.Empty, Version = string.Empty, Source = string.Empty;
            string[] Tags = Array.Empty<string>();
            int BeatmapID = 0, BeatmapSetID = 0;

            if (ParseMetadataValues) {
                GetMetadataFields(MetadataValues, out Title, out TitleUnicode, out Artist, out ArtistUnicode, out Creator, out Version, out Source, out Tags, out BeatmapID, out BeatmapSetID);
            }

            //Debug.WriteLine("\tParsing Difficulty Fields...");
            decimal HPDrainRate = 5M, CircleSize = 5M, OverallDifficulty = 5M, ApproachRate = 5M, SliderMultiplier = 5M, SliderTickRate = 5M;

            if (ParseDifficultyValues) {
                GetDifficultyFields(DifficultyValues, out HPDrainRate, out CircleSize, out OverallDifficulty, out ApproachRate, out SliderMultiplier, out SliderTickRate);
            }

            //Debug.WriteLine("\tParsing Event Fields...");
            OsuEvent[] Events = Array.Empty<OsuEvent>();

            if (ParseEvents) {
                GetEventsField(EventValues, out Events);
            }

            //Debug.WriteLine("\tParsing TimingPoint Fields...");
            OsuTimingPoint[] TimingPoints = Array.Empty<OsuTimingPoint>();

            if (ParseTimingPoints) {
                GetTimingPointField(TimingPointValues, out TimingPoints);
            }

            //Debug.WriteLine("\tParsing HitObject Fields...");
            OsuHitObject[] HitObjects = Array.Empty<OsuHitObject>();

            if (ParseHitObjects) {
                GetHitObjectsField(HitObjectValues, out HitObjects);
            }

            return new OsuBeatmap(AudioFileName, AudioLeadIn, AudioHash, PreviewTime, Countdown, SampleSet, StackLeniency, Mode, LetterboxInBreaks, StoryFireInFront, UseSkinSprites, AlwaysShowPlayfield, OverlayPosition, SkinPreference, EpilepsyWarning, CountdownOffset, SpecialStyle, WidescreenStoryboard, SamplesMatchPlaybackRate, Bookmarks, DistanceSpacing, BeatDivisor, GridSize, TimelineZoom, Title,
                 TitleUnicode, Artist, ArtistUnicode, Creator, Version, Source, Tags, BeatmapID, BeatmapSetID, HPDrainRate, CircleSize, OverallDifficulty, ApproachRate, SliderMultiplier, SliderTickRate, Events, TimingPoints, HitObjects);
        }

        #region Field Parsers

        public static void GetGeneralFields(List<string> Lines, out RelativeFileInfo AudioFileName, out int AudioLeadIn, out Hash AudioHash, out int PreviewTime, out OsuCountdown Countdown, out OsuSampleSet SampleSet, out decimal StackLeniency, out OsuGameMode Mode, out bool LetterboxInBreaks, out bool StoryFireInFront, out bool UseSkinSprites, out bool AlwaysShowPlayfield, out OsuDrawPosition OverlayPosition, out string SkinPreference, out bool EpilepsyWarning, out int CountdownOffset, out bool SpecialStyle, out bool WidescreenStoryboard, out bool SamplesMatchPlaybackRate) {
            AudioFileName = default;
            AudioLeadIn = CountdownOffset = 0;
            AudioHash = default;
            PreviewTime = -1;
            Countdown = OsuCountdown.Normal;
            SampleSet = OsuSampleSet.Normal;
            StackLeniency = 0.7M;
            Mode = OsuGameMode.Osu;
            LetterboxInBreaks = AlwaysShowPlayfield = EpilepsyWarning = SpecialStyle = WidescreenStoryboard = SamplesMatchPlaybackRate = false;
            StoryFireInFront = UseSkinSprites = true;
            OverlayPosition = OsuDrawPosition.NoChange;
            SkinPreference = default;

            //Debug.WriteLine("Iterating through General values");
            foreach ((string Key, string Value) in GetValues(Lines, ": ")) {
                switch(Key) {
                    case @"audiofilename":
                        //Debug.WriteLine($"\tFound: {Key} == {Value}");
                        AudioFileName = Value.ConvertToRelativeFileInfo();
                        break;
                    case @"audioleadin":
                        //Debug.WriteLine($"\tFound: {Key} == {Value}");
                        AudioLeadIn = Value.ConvertToInt(0);
                        break;
                    case @"audiohash":
                        //Debug.WriteLine($"\tFound: {Key} == {Value}");
                        //TODO Implement obtaining pre-generated hash stored in string
                        AudioHash = null;
                        break;
                    case @"previewtime":
                        //Debug.WriteLine($"\tFound: {Key} == {Value}");
                        PreviewTime = Value.ConvertToInt(-1);
                        break;
                    case @"countdown":
                        //Debug.WriteLine($"\tFound: {Key} == {Value}");
                        Countdown = Value.ConvertToEnum<OsuCountdown>();
                        break;
                    case @"sampleset":
                        //Debug.WriteLine($"\tFound: {Key} == {Value}");
                        SampleSet = Value.ConvertToEnum<OsuSampleSet>();
                        break;
                    case @"stackleniency":
                        //Debug.WriteLine($"\tFound: {Key} == {Value}");
                        StackLeniency = Value.ConvertToDecimal(0.7M);
                        break;
                    case @"mode":
                        //Debug.WriteLine($"\tFound: {Key} == {Value}");
                        Mode = Value.ConvertToEnum<OsuGameMode>();
                        break;
                    case @"letterboxinbreaks":
                        //Debug.WriteLine($"\tFound: {Key} == {Value}");
                        LetterboxInBreaks = Value.ConvertToBool();
                        break;
                    case @"storyfireinfront":
                        //Debug.WriteLine($"\tFound: {Key} == {Value}");
                        StoryFireInFront = Value.ConvertToBool();
                        break;
                    case @"useskinsprites":
                        //Debug.WriteLine($"\tFound: {Key} == {Value}");
                        UseSkinSprites = Value.ConvertToBool();
                        break;
                    case @"alwaysshowplayfield":
                        //Debug.WriteLine($"\tFound: {Key} == {Value}");
                        AlwaysShowPlayfield = Value.ConvertToBool();
                        break;
                    case @"overlayposition":
                        //Debug.WriteLine($"\tFound: {Key} == {Value}");
                        OverlayPosition = Value.ConvertToEnum<OsuDrawPosition>();
                        break;
                    case @"skinpreference":
                        //Debug.WriteLine($"\tFound: {Key} == {Value}");
                        SkinPreference = Value;
                        break;
                    case @"epilepsywarning":
                        //Debug.WriteLine($"\tFound: {Key} == {Value}");
                        EpilepsyWarning = Value.ConvertToBool();
                        break;
                    case @"countdownoffset":
                        //Debug.WriteLine($"\tFound: {Key} == {Value}");
                        CountdownOffset = Value.ConvertToInt(0);
                        break;
                    case @"specialstyle":
                        //Debug.WriteLine($"\tFound: {Key} == {Value}");
                        SpecialStyle = Value.ConvertToBool();
                        break;
                    case @"widescreenstoryboard":
                        //Debug.WriteLine($"\tFound: {Key} == {Value}");
                        WidescreenStoryboard = Value.ConvertToBool();
                        break;
                    case @"samplesmatchplaybackrate":
                        //Debug.WriteLine($"\tFound: {Key} == {Value}");
                        SamplesMatchPlaybackRate = Value.ConvertToBool();
                        break;
                }
            }
        }

        public static void GetEditorFields(List<string> Values, out int[] Bookmarks, out decimal DistanceSpacing, out decimal BeatDivisor, out int GridSize, out decimal TimelineZoom) {
            Bookmarks = Array.Empty<int>();
            DistanceSpacing = BeatDivisor = TimelineZoom = 0M;
            GridSize = 0;

            foreach((string Key, string Value) in GetValues(Values, ": ")) {
                switch (Key) {
                    case @"bookmarks":
                        List<int> NewBookmarks = new List<int>();
                        foreach(string Bookmark in Value.SeparateCommaList()) {
                            if (Bookmark.TryConvertToInt(out int BookmarkTime)) {
                                NewBookmarks.Add(BookmarkTime);
                            }
                        }

                        Bookmarks = NewBookmarks.ToArray();
                        break;
                    case @"distancespacing":
                        DistanceSpacing = Value.ConvertToDecimal(0M);
                        break;
                    case @"beatdivisor":
                        BeatDivisor = Value.ConvertToDecimal(0M);
                        break;
                    case @"gridsize":
                        GridSize = Value.ConvertToInt(0);
                        break;
                    case @"timelinezoom":
                        TimelineZoom = Value.ConvertToDecimal(0M);
                        break;
                }
            }
        }

        public static void GetMetadataFields(List<string> Values, out string Title, out string TitleUnicode, out string Artist, out string ArtistUnicode, out string Creator, out string Version, out string Source, out string[] Tags, out int BeatmapID, out int BeatmapSetID) {
            Title = TitleUnicode = Artist = ArtistUnicode = Creator = Version = Source = string.Empty;
            Tags = Array.Empty<string>();
            BeatmapID = BeatmapSetID = 0;

            foreach((string Key, string Value) in GetValues(Values)) {
                switch (Key) {
                    case @"title":
                        Title = Value;
                        break;
                    case @"titleunicode":
                        TitleUnicode = Value;
                        break;
                    case @"artist":
                        Artist = Value;
                        break;
                    case @"artistunicode":
                        ArtistUnicode = Value;
                        break;
                    case @"creator":
                        Creator = Value;
                        break;
                    case @"version":
                        Version = Value;
                        break;
                    case @"source":
                        Source = Value;
                        break;
                    case @"tags":
                        Tags = Value.SeparateSpaceList(StringSplitOptions.RemoveEmptyEntries).ToArray();
                        break;
                    case @"beatmapid":
                        BeatmapID = Value.ConvertToInt(0);
                        break;
                    case @"beatmapsetid":
                        BeatmapSetID = Value.ConvertToInt(0);
                        break;
                }
            }
        }

        public static void GetDifficultyFields(List<string> Values, out decimal HPDrainRate, out decimal CircleSize, out decimal OverallDifficulty, out decimal ApproachRate, out decimal SliderMultiplier, out decimal SliderTickRate) {
            HPDrainRate = CircleSize = OverallDifficulty = ApproachRate = SliderMultiplier = SliderTickRate = 5M;

            foreach ((string Key, string Value) in GetValues(Values)) {
                switch(Key) {
                    case @"hpdrainrate":
                        HPDrainRate = Value.ConvertToDecimal(5M);
                        break;
                    case @"circlesize":
                        CircleSize = Value.ConvertToDecimal(5M);
                        break;
                    case @"overalldifficulty":
                        OverallDifficulty = Value.ConvertToDecimal(5M);
                        break;
                    case @"approachrate":
                        ApproachRate = Value.ConvertToDecimal(5M);
                        break;
                    case @"slidermultiplier":
                        SliderMultiplier = Value.ConvertToDecimal(5M);
                        break;
                    case @"slidertickrate":
                        SliderTickRate = Value.ConvertToDecimal(5M);
                        break;
                }
            }
        }

        public static void GetEventsField(List<string> Value, out OsuEvent[] Events) => Events = Value.Select(GetEvent).Where(E => E != null).ToArray();

        public static OsuEvent GetEvent(string Value) {
            Debug.WriteLine($"\t\t\tParsing event from value: '{Value}'");
            IEnumerable<string> Values = Value.SeparateCommaList(StringSplitOptions.RemoveEmptyEntries);

            OsuEvent.OsuEventType EventType = default;
            int StartTime = 0;
            List<string> EventParams = new List<string>();

            int I = 0;
            //Debug.WriteLine($"\t\t\tPrepared to iterate through forms of value '{Value}'");
            foreach(string V in Values) {
                switch (I) {
                    case 0: //EventType
                        //Debug.WriteLine($"\t\t\t\tForm[0] ('{V}') is {nameof(OsuEvent.OsuEventType)}");
                        //TODO: Event types greater than 2 are storyboard events.
                        //TODO: Currently they are parsed as 'Unknown events'.
                        //TODO: Add Storyboard support.
                        EventType = V.ConvertToEnum(OsuEvent.OsuEventType.Unknown);
                        //Debug.WriteLine($"\t\t\t\t\t=>{EventType}");
                        break;
                    case 1: //StartTime
                        //Debug.WriteLine($"\t\t\t\tForm[1] ('{V}')  is {nameof(Int32)}");
                        StartTime = V.ConvertToInt(0);
                        //Debug.WriteLine($"\t\t\t\t\t=>{StartTime}");
                        break;
                    default: //EventParams
                        //Debug.WriteLine($"\t\t\t\tForm[2] ('{V}')  is {nameof(List<string>)}");
                        EventParams.Add(V);
                        //Debug.WriteLine($"\t\t\t\t\t=>{EventParams.GetListString()}");
                        break;
                }
                I++;
            }

            //Debug.WriteLine($"\t\t\tFound Event; (Type: {EventType}, StartTime: {StartTime}, EventParams: {EventParams.GetListString()})");
            switch (EventType) {
                case OsuEvent.OsuEventType.Background:
                    return new OsuBackground(EventType, StartTime, EventParams.ToArray());
                case OsuEvent.OsuEventType.Video:
                    return new OsuBackgroundVideo(EventType, StartTime, EventParams.ToArray());
                case OsuEvent.OsuEventType.Break:
                    return new OsuBreak(EventType, StartTime, EventParams.ToArray());
                case OsuEvent.OsuEventType.Colour:
                    return null;
                default:
                    Debug.WriteLine($"Event {EventType} is unknown.", "WARNING");
                    return null;
                    //throw new ArgumentOutOfRangeException();
            }
        }

        public static void GetTimingPointField(List<string> Value, out OsuTimingPoint[] TimingPoints) => TimingPoints = Value.Select(GetTimingPoint).ToArray();

        public static OsuTimingPoint GetTimingPoint(string Value) {
            IEnumerable<string> Values = Value.SeparateCommaList(StringSplitOptions.RemoveEmptyEntries);

            int Time = 0, Meter = 0, SampleIndex = 0, Volume = 100;
            decimal BeatLength = 0;
            OsuSampleSet SampleSet = default;
            bool Uninherited = default;
            OsuEffects Effects = default;

            int I = 0;
            foreach(string V in Values) {
                switch (I) {
                    case 0: //Time
                        Time = V.ConvertToInt(0);
                        break;
                    case 1: //BeatLength
                        BeatLength = V.ConvertToDecimal(0);
                        break;
                    case 2: //Meter
                        Meter = V.ConvertToInt(0);
                        break;
                    case 3: //SampleSet
                        SampleSet = V.ConvertToEnum<OsuSampleSet>();
                        break;
                    case 4: //SampleIndex
                        SampleIndex = V.ConvertToInt(0);
                        break;
                    case 5: //Volume
                        Volume = V.ConvertToInt(0);
                        break;
                    case 6: //Uninherited
                        Uninherited = V.ConvertToBool(StringComparison.InvariantCultureIgnoreCase);
                        break;
                    default: //Effects
                        Effects = V.ConvertToEnum<OsuEffects>();
                        break;
                }

                I++;
            }

            return new OsuTimingPoint(Time, BeatLength, Meter, SampleSet, SampleIndex, Volume, Uninherited, Effects);
        }

        public static void GetHitObjectsField(List<string> Value, out OsuHitObject[] HitObjects) => HitObjects = Value.Select(GetHitObject).Where(E => E != null).ToArray();

        public static OsuHitObject GetHitObject(string Value) {
            List<string> Values = Value.SeparateCommaList(StringSplitOptions.RemoveEmptyEntries).ToList();

            int X = 0, Y = 0, Time = 0;
            OsuHitObjectType Type = default;
            OsuHitSound HitSound = default;
            OsuHitSample HitSample = default;
            List<string> ObjectParams = new List<string>();

            int I = 0;
            int L = Values.Count;
            //Debug.WriteLine("\t\t\tIteration Prepared...");
            foreach (string V in Values) {
                switch(I) {
                    case 0: //X
                        //Debug.WriteLine($"\t\t\t\tGot X: {X}");
                        X = V.ConvertToInt(0);
                        break;
                    case 1: //Y
                        //Debug.WriteLine($"\t\t\t\tGot Y: {Y}");
                        Y = V.ConvertToInt(0);
                        break;
                    case 2: //Time
                        //Debug.WriteLine($"\t\t\t\tGot Time: {Time}");
                        Time = V.ConvertToInt(0);
                        break;
                    case 3: //Type
                        //Debug.WriteLine($"\t\t\t\tGot Type: {Type}");
                        Type = V.ConvertToEnum<OsuHitObjectType>();
                        break;
                    case 4: //HitSound
                        //Debug.WriteLine($"\t\t\t\tGot HitSound: {HitSound}");
                        HitSound = V.ConvertToEnum<OsuHitSound>();
                        break;
                    default:
                        //Debug.WriteLine($"\t\t\t\tGot Param: {V}");
                        if (I <= L - 1) { //Last Value: HitSample
                            //Debug.WriteLine("\t\t\t\t\t=>Actually a HitSample");
                            HitSample = GetHitSample(V);
                        } else { //All values before last: ObjectParams
                            ObjectParams.Add(V);
                        }
                        break;
                }

                I++;
            }

            //Debug.WriteLine($"\t\t\tType: {Type}");
            switch (Type) {
                case OsuHitObjectType.HitCircle:
                case OsuHitObjectType.NewCombo:
                case OsuHitObjectType.SkipOneCombo:
                case OsuHitObjectType.SkipTwoCombos:
                case OsuHitObjectType.SkipThreeCombos:
                    //Debug.WriteLine("\t\t\t=>HitObject");
                    return new OsuHitObject(X, Y, Time, Type, HitSound, ObjectParams.ToArray(), HitSample);
                case OsuHitObjectType.Slider:
                    //Debug.WriteLine("\t\t\t=>HitObjectSlider");
                    return new OsuHitObjectSlider(X, Y, Time, Type, HitSound, ObjectParams.ToArray(), HitSample);
                case OsuHitObjectType.Spinner:
                    //Debug.WriteLine("\t\t\t=>HitObjectSpinner");
                    return new OsuHitObjectSpinner(X, Y, Time, Type, HitSound, ObjectParams.ToArray(), HitSample);
                case OsuHitObjectType.OsuManiaHold:
                    //Debug.WriteLine("\t\t\t=>HitObjectManiaHold");
                    return new OsuHitObjectManiaHold(X, Y, Time, Type, HitSound, ObjectParams.ToArray(), HitSample);
                default:
                    //throw new ArgumentOutOfRangeException();
                    return null;
            }

        }

        public static OsuHitSample GetHitSample(string Value) {
            IEnumerable<string> Values = Value.SeparateColonList(StringSplitOptions.RemoveEmptyEntries);

            int NormalSet = 0, AdditionSet = 0, Index = 0, Volume = 100;
            RelativeFileInfo FileName = default;

            int I = 0;
            foreach(string V in Values) {
                switch(I) {
                    case 0: //NormalSet
                        NormalSet = V.ConvertToInt(0);
                        break;
                    case 1: //AdditionSet
                        AdditionSet = V.ConvertToInt(0);
                        break;
                    case 2: //Index
                        Index = V.ConvertToInt(0);
                        break;
                    case 3: //Volume
                        Volume = V.ConvertToInt(100);
                        break;
                    case 4: //FileName
                        FileName = Value.ConvertToRelativeFileInfo();
                        break;
                }

                I++;
            }

            return new OsuHitSample(NormalSet, AdditionSet, Index, Volume, FileName);
        }

        #endregion

        #endregion

        #region Constructor

        public OsuBeatmap(RelativeFileInfo AudioFileName = default, int AudioLeadIn = 0, Hash AudioHash = default, int PreviewTime = -1, OsuCountdown Countdown = OsuCountdown.Normal, OsuSampleSet SampleSet = OsuSampleSet.Normal, decimal StackLeniency = 0.7M, OsuGameMode Mode = OsuGameMode.Osu, bool LetterboxInBreaks = false, bool StoryFireInFront = true, bool UseSkinSprites = true, bool AlwaysShowPlayfield = false, OsuDrawPosition OverlayPosition = OsuDrawPosition.NoChange, string SkinPreference = "", bool EpilepsyWarning = false, int CountdownOffset = 0, bool SpecialStyle = false, bool WidescreenStoryboard = false, bool SamplesMatchPlaybackRate = false, int[] Bookmarks = default, decimal DistanceSpacing = 0M, decimal BeatDivisor = 0M, int GridSize = 0, decimal TimelineZoom = 0M, string Title = "", string TitleUnicode = "", string Artist = "", string ArtistUnicode = "", string Creator = "", string Version = "", string Source = "", string[] Tags = default, int BeatmapID = 0, int BeatmapSetID = 0, decimal HPDrainRate = 5M, decimal CircleSize = 5M, decimal OverallDifficulty = 5M, decimal ApproachRate = 5M, decimal SliderMultiplier = 1M, decimal SliderTickRate = 1M, OsuEvent[] Events = default, OsuTimingPoint[] TimingPoints = default, OsuHitObject[] HitObjects = default) {
            this.AudioFileName = AudioFileName/* ?? throw new ArgumentNullException(nameof(AudioFileName))*/;
            this.AudioLeadIn = AudioLeadIn;
#pragma warning disable CS0612 // Type or member is obsolete
            this.AudioHash = AudioHash/* ?? throw new ArgumentNullException(nameof(AudioHash))*/;
#pragma warning restore CS0612 // Type or member is obsolete
            this.PreviewTime = PreviewTime;
            this.Countdown = Countdown;
            this.SampleSet = SampleSet;
            this.StackLeniency = StackLeniency;
            this.Mode = Mode;
            this.LetterboxInBreaks = LetterboxInBreaks;
#pragma warning disable CS0612 // Type or member is obsolete
            this.StoryFireInFront = StoryFireInFront;
#pragma warning restore CS0612 // Type or member is obsolete
            this.UseSkinSprites = UseSkinSprites;
#pragma warning disable CS0612 // Type or member is obsolete
            this.AlwaysShowPlayfield = AlwaysShowPlayfield;
#pragma warning restore CS0612 // Type or member is obsolete
            this.OverlayPosition = OverlayPosition;
            this.SkinPreference = SkinPreference/* ?? throw new ArgumentNullException(nameof(SkinPreference))*/;
            this.EpilepsyWarning = EpilepsyWarning;
            this.CountdownOffset = CountdownOffset;
            this.SpecialStyle = SpecialStyle;
            this.WidescreenStoryboard = WidescreenStoryboard;
            this.SamplesMatchPlaybackRate = SamplesMatchPlaybackRate;
            this.Bookmarks = Bookmarks/* ?? throw new ArgumentNullException(nameof(Bookmarks))*/;
            this.DistanceSpacing = DistanceSpacing;
            this.BeatDivisor = BeatDivisor;
            this.GridSize = GridSize;
            this.TimelineZoom = TimelineZoom;
            this.Title = Title/* ?? throw new ArgumentNullException(nameof(Title))*/;
            this.TitleUnicode = TitleUnicode/* ?? throw new ArgumentNullException(nameof(TitleUnicode))*/;
            this.Artist = Artist/* ?? throw new ArgumentNullException(nameof(Artist))*/;
            this.ArtistUnicode = ArtistUnicode/* ?? throw new ArgumentNullException(nameof(ArtistUnicode))*/;
            this.Creator = Creator/* ?? throw new ArgumentNullException(nameof(Creator))*/;
            this.Version = Version/* ?? throw new ArgumentNullException(nameof(Version))*/;
            this.Source = Source/* ?? throw new ArgumentNullException(nameof(Source))*/;
            this.Tags = Tags/* ?? throw new ArgumentNullException(nameof(Tags))*/;
            this.BeatmapID = BeatmapID;
            this.BeatmapSetID = BeatmapSetID;
            this.HPDrainRate = HPDrainRate;
            this.CircleSize = CircleSize;
            this.OverallDifficulty = OverallDifficulty;
            this.ApproachRate = ApproachRate;
            this.SliderMultiplier = SliderMultiplier;
            this.SliderTickRate = SliderTickRate;
            this.Events = Events/* ?? throw new ArgumentNullException(nameof(Events))*/;
            this.TimingPoints = TimingPoints/* ?? throw new ArgumentNullException(nameof(TimingPoints))*/;
            this.HitObjects = HitObjects/* ?? throw new ArgumentNullException(nameof(HitObjects))*/;
        }

        #endregion

        #region Debug Methods

#pragma warning disable CS0612 // Type or member is obsolete
        public string GeneralFieldsString() => $"AudioFileName: {AudioFileName}, AudioLeadIn: {AudioLeadIn}, AudioHash: {AudioHash}, PreviewTime: {PreviewTime}, Countdown: {Countdown}, SampleSet: {SampleSet}, StackLeniency: {StackLeniency}, Mode: {Mode}, LetterboxInBreaks: {LetterboxInBreaks}, StoryFireInFront: {StoryFireInFront}, UseSkinSprites: {UseSkinSprites}, AlwaysShowPlayfield: {AlwaysShowPlayfield}, OverlayPosition: {OverlayPosition}, SkinPreference: {SkinPreference}, EpilepsyWarning: {EpilepsyWarning}, CountdownOffset: {CountdownOffset}, SpecialStyle: {SpecialStyle}, WidescreenStoryboard: {WidescreenStoryboard}, SamplesMatchPlaybackRate: {SamplesMatchPlaybackRate}";
#pragma warning restore CS0612 // Type or member is obsolete

        public string EditorFieldsString() => $"Bookmarks: ({Bookmarks.GetListedString()}), DistanceSpacing: {DistanceSpacing}, BeatDivisor: {BeatDivisor}, GridSize: {GridSize}, TimelineZoom: {TimelineZoom}";

        public string MetadataFieldsString() => $"Title: {Title}, TitleUnicode: {TitleUnicode}, Artist: {Artist}, ArtistUnicode: {ArtistUnicode}, Creator: {Creator}, Version: {Version}, Source: {Source}, Tags: ({Tags.GetListedString()}), BeatmapID: {BeatmapID}, BeatmapSetID: {BeatmapSetID}";

        public string DifficultyFieldsString() => $"HPDrainRate: {HPDrainRate}, CircleSize: {CircleSize}, OverallDifficulty: {OverallDifficulty}, ApproachRate: {ApproachRate}, SliderMultiplier: {SliderMultiplier}, SliderTickRate: {SliderTickRate}";

        public string EventsFieldsString() => $"Events: ({Events.GetListedString()})";

        public string TimingPointsFieldsString() => $"TimingPoints: ({TimingPoints.GetListedString()})";

        public string HitObjectsFieldsString() => $"HitObjects: ({HitObjects.GetListedString()})";

        public override string ToString() => $"[{nameof(OsuBeatmap)}・{TitleUnicode}]: {{\r\t[General] {GeneralFieldsString()}\r\t[Editor] {EditorFieldsString()}\r\t[Metadata] {MetadataFieldsString()}\r\t[Difficulty] {DifficultyFieldsString()}\r\t[Events] {EventsFieldsString()}\r\t[Timing Points] {TimingPointsFieldsString()}[HitObjects] {HitObjectsFieldsString()}\r}}";

        #endregion
    }
}
