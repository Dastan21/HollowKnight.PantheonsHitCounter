using System;
using System.Collections.Generic;
using InControl;
using Modding.Converters;
using Newtonsoft.Json;

namespace PantheonsHitCounter
{
    [Serializable]
    public class LocalData
    {
        public List<PantheonData> pantheons = new List<PantheonData>{
            new PantheonData(1),
            new PantheonData(2),
            new PantheonData(3),
            new PantheonData(4),
            new PantheonData(5)
        };
    }
    
    [Serializable]
    public class PantheonData
    {
        public int number;
        public List<BossData> bosses;

        public PantheonData(int pantheonNumber)
        {
            number = pantheonNumber;
            bosses = new List<BossData>();
        }

        public void FillPantheonBosses()
        {
            var length = (number == 5 ? 42 : 10);
            for (var i = 0; i < length; i++)
                bosses.Add(new BossData());
        }
    }

    [Serializable]
    public class BossData
    {
        public string name;
        public int hitsPb = -1;
    }

    [Serializable]
    public class GlobalData
    {
        public static int defaultSplitsNumber = 5;
        
        [JsonConverter(typeof(PlayerActionSetConverter))]
        public KeyBinds keybinds = new KeyBinds();
        [JsonConverter(typeof(PlayerActionSetConverter))]
        public ButtonBinds buttonbinds = new ButtonBinds();
        
        public bool anonymize;
        public bool compactMode;
        public int totalSplits = defaultSplitsNumber;
    }

    public class KeyBinds : PlayerActionSet
    {
        public PlayerAction toggleCounter;
        public PlayerAction nextBossSplit;
        public PlayerAction previousBossSplit;

        public KeyBinds()
        {
            toggleCounter = CreatePlayerAction("hideCounterKey");
            nextBossSplit = CreatePlayerAction("nextBossSplitKey");
            previousBossSplit = CreatePlayerAction("previousBossSplitKey");
            DefaultBinds();
        }

        private void DefaultBinds()
        {
            toggleCounter.AddDefaultBinding(Key.N);
            nextBossSplit.AddDefaultBinding(Key.PageDown);
            previousBossSplit.AddDefaultBinding(Key.PageUp);
        }
    }
    
    public class ButtonBinds : PlayerActionSet
    {
        public PlayerAction toggleCounter;
        public PlayerAction nextBossSplit;
        public PlayerAction previousBossSplit;
        
        public ButtonBinds()
        {
            toggleCounter = CreatePlayerAction("hideCounterButton");
            nextBossSplit = CreatePlayerAction("nextBossSplitButton");
            previousBossSplit = CreatePlayerAction("previousBossSplitButton");
            DefaultBinds();
        }

        private void DefaultBinds()
        {
            toggleCounter.AddDefaultBinding(InputControlType.RightBumper);
            nextBossSplit.AddDefaultBinding(InputControlType.DPadDown);
            previousBossSplit.AddDefaultBinding(InputControlType.DPadUp);
        }
    }
}