using System.Collections.Generic;
using System.Linq;

namespace PartyPanel
{
    class OstHelper
    {
        public enum LevelDifficulty
        {
            Auto = -1,
            Easy = 0,
            Normal = 1,
            Hard = 2,
            Expert = 3,
            ExpertPlus = 4
        }
        public class Pack
        {
            public string PackID { get; set; }
            public string PackName { get; set; }
            public Dictionary<string, string> SongDictionary { get; set; }
        }

        public static readonly Pack[] packs;
        public static readonly Dictionary<string, string> allLevels = new Dictionary<string, string>();

        //C# doesn't seem to want me to use an array of a non-primitive here.
        private static readonly int[] mainDifficulties = { (int)LevelDifficulty.Easy, (int)LevelDifficulty.Normal, (int)LevelDifficulty.Hard, (int)LevelDifficulty.Expert, (int)LevelDifficulty.ExpertPlus };
        private static readonly int[] angelDifficulties = { (int)LevelDifficulty.Hard, (int)LevelDifficulty.Expert, (int)LevelDifficulty.ExpertPlus };
        private static readonly int[] oneSaberDifficulties = { (int)LevelDifficulty.Expert };
        private static readonly int[] noArrowsDifficulties = { (int)LevelDifficulty.Expert };

        public static string GetOstSongNameFromLevelId(string hash)
        {
            hash = hash.EndsWith("OneSaber") ? hash.Substring(0, hash.IndexOf("OneSaber")) : hash;
            hash = hash.EndsWith("NoArrows") ? hash.Substring(0, hash.IndexOf("NoArrows")) : hash;
            return allLevels[hash];
        }

        public static LevelDifficulty[] GetDifficultiesFromLevelId(string levelId)
        {
            if (IsOst(levelId))
            {
                if (levelId.Contains("OneSaber")) return oneSaberDifficulties.Select(x => (LevelDifficulty)x).ToArray();
                else if (levelId.Contains("NoArrows")) return noArrowsDifficulties.Select(x => (LevelDifficulty)x).ToArray();
                else if (levelId != "AngelVoices") return mainDifficulties.Select(x => (LevelDifficulty)x).ToArray();
                else return angelDifficulties.Select(x => (LevelDifficulty)x).ToArray();
            }
            return null;
        }

        public static bool IsOst(string levelId)
        {
            levelId = levelId.EndsWith("OneSaber") ? levelId.Substring(0, levelId.IndexOf("OneSaber")) : levelId;
            levelId = levelId.EndsWith("NoArrows") ? levelId.Substring(0, levelId.IndexOf("NoArrows")) : levelId;
            return packs.Any(x => x.SongDictionary.ContainsKey(levelId));
        }
    }
}
