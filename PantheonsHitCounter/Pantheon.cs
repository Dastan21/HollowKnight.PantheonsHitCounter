using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace PantheonsHitCounter
{
    [Serializable]
    public class Pantheon
    {
        [JsonProperty("name")] public string name;
        [JsonProperty("number")] public int number;
        [JsonProperty("bosses")] public List<Boss> bosses;
        public int bossNumber;

        public int TotalHits
        {
            get { return bosses.Sum(boss => boss.hits); }
        }
        public int TotalHitsPb
        {
            get { return bosses.Any(boss => boss.hitsPb > -1) ? bosses.Sum(boss => Math.Max(0, boss.hitsPb)) : -1; }
        }

        public static int FindPantheon(string previousSceneName, string nextSceneName)
        {
            if (previousSceneName == "GG_Atrium_Roof" && nextSceneName == "GG_Vengefly_V") return 5;
            
            switch (nextSceneName)
            {
                case "GG_Vengefly": return 1;
                case "GG_Ghost_Xero": return 2;
                case "GG_Hive_Knight": return 3;
                case "GG_Crystal_Guardian_2": return 4;
            }
            
            return 0;
        }

        public void ResetCounter()
        {
            bossNumber = 0;
            foreach (var boss in bosses)
                boss.hits = 0;
        }

        public void ResetPbCounter()
        {
            foreach (var boss in bosses)
                boss.hitsPb = -1;
        }

        public Boss GetBossBySceneName(string sceneName) => bosses.Find(boss => boss.sceneName == sceneName);
        public void NextBoss()
        {
            if (bossNumber < bosses.Count - 1) bossNumber++;
        }

        public void PreviousBoss()
        {
            if (bossNumber > 0) bossNumber--;
        }
        public bool IsPbRun() => TotalHitsPb < 0 || TotalHits < TotalHitsPb;
    }
    
    [Serializable]
    public class Boss
    {
        [JsonProperty("name")] public string name;
        [JsonProperty("scene")] public string sceneName;
        public int hits;
        public int hitsPb = -1;
        public bool disabled; // If Grey Prince Zote is not beaten
        
        public void AddHit() => hits++;
    }
}