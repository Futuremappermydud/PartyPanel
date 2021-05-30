using System;

namespace PartyPanelShared.Models
{
    [Serializable]
    public class PlaySong
    {
        public string levelId;
        public string difficulty;
        public Characteristic characteristic;
        //public PlayerSpecificSettings playerSettings;
        //public GameplayModifiers gameplayModifiers;
        //public PracticeSettings practiceSettings;
    }
}
