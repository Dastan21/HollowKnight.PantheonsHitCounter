using System;
using System.Collections.Generic;
using System.Linq;
using Modding;
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

        public Pantheon(int number)
        {
            this.number = number;
            switch (number)
            {
                case 1: name = "Pantheon of the Master"; break;
                case 2: name = "Pantheon of the Artist"; break;
                case 3: name = "Pantheon of the Sage"; break;
                case 4: name = "Pantheon of the Knight"; break;
                default: name = "Pantheon of Hallownest"; break;
            }
        }

        public static int FindPantheon(List<Pantheon> pantheons, string previousSceneName, string nextSceneName)
        {
            // not a pantheon
            if (previousSceneName != "GG_Boss_Door_Entrance" && previousSceneName != "GG_Atrium_Roof") return -1;
            // pantheon 5
            if (previousSceneName == "GG_Atrium_Roof" && pantheons[4].bosses.Exists(b => b.sceneName == nextSceneName)) return 5;
            
            // pantheon 1-4
            if (previousSceneName == "GG_Boss_Door_Entrance")
            {
                if (pantheons[0].bosses.Exists(b => b.sceneName.Equals(nextSceneName))) return 1;
                if (pantheons[1].bosses.Exists(b => b.sceneName.Equals(nextSceneName))) return 2;
                if (pantheons[2].bosses.Exists(b => b.sceneName.Equals(nextSceneName))) return 3;
                if (pantheons[3].bosses.Exists(b => b.sceneName.Equals(nextSceneName))) return 4;
                PantheonsHitCounter.instance.Log(pantheons[3].bosses.Count);
                foreach (var boss in pantheons[3].bosses)
                {
                    PantheonsHitCounter.instance.Log($"{boss.sceneName} - {nextSceneName}");
                }
            }

            return -1;
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

        public Boss GetBossBySceneName(string sceneName) => bosses.Find(boss => boss.sceneName.Equals(sceneName));
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