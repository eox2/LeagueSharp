using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace SephKhazix
{
    class SmiteManager
    {

        public Dictionary<string, SmiteType> SmiteDictionary = new Dictionary<string, SmiteType>()
        {
          { "summonersmite", SmiteType.ChallengingSmite},
          { "s5_summonersmiteplayerganker", SmiteType.ChallengingSmite },
          { "s5_summonersmiteduel", SmiteType.ChallengingSmite},
        };

        public SmiteType CurrentSmiteType = SmiteType.NotChosen;

        public Spell Smite = null;

        internal SpellDataInst SmiteInstance = null;

        public SmiteManager()
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private void Game_OnGameLoad(EventArgs args)
        {
            SmiteInstance = ObjectManager.Player.Spellbook.Spells.FirstOrDefault(x => x.Name.ToLower().Contains("smite"));

            //Smite is not chosen
            if (SmiteInstance == null)
            {
                return;
            }

            //Setup a Spell instance using the Smite slot
            Smite = new Spell(SmiteInstance.Slot, 500, TargetSelector.DamageType.True);

            //Set Current Smite Type based on the spell name 
            CurrentSmiteType = SmiteDictionary[SmiteInstance.Name.ToLower()];

            //Register Events to monitor check smite type every time an item is bought/sold
            Obj_AI_Base.OnPlaceItemInSlot += Obj_AI_Base_OnPlaceItemInSlot;
            Obj_AI_Base.OnRemoveItem += Obj_AI_Base_OnRemoveItem;

        }

        private void Obj_AI_Base_OnRemoveItem(Obj_AI_Base sender, Obj_AI_BaseRemoveItemEventArgs args)
        {
            if (sender.IsMe)
            {
                UpdateSmiteType();
            }
        }

        private void Obj_AI_Base_OnPlaceItemInSlot(Obj_AI_Base sender, Obj_AI_BasePlaceItemInSlotEventArgs args)
        {
            if (sender.IsMe)
            {
                UpdateSmiteType();
            }
        }


        void UpdateSmiteType()
        {
            CurrentSmiteType = SmiteDictionary[Smite.Instance.Name.ToLower()];
        }

        public enum SmiteType
        {
            NotChosen,
            RegularSmite,
            ChillingSmite,
            ChallengingSmite,
        }

        public bool CanCast(Obj_AI_Base unit)
        {
            if (CurrentSmiteType == SmiteType.NotChosen || !Smite.IsReady() || !unit.IsInRange(Smite.Range))
            {
                return false;
            }


            if (unit is Obj_AI_Hero)
            {
                return CurrentSmiteType == SmiteType.ChallengingSmite || CurrentSmiteType == SmiteType.ChillingSmite;
            }

            var asMinion = unit as Obj_AI_Minion;

            if (asMinion != null && !MinionManager.IsWard(asMinion))
            {
                return true;
            }

            return false;
        }

        public double GetSmiteDamage(Obj_AI_Base target)
        {
            if (CurrentSmiteType == SmiteType.NotChosen)
            {
                return 0;
            }

            var asHero = target as Obj_AI_Hero;
            var asMinion = target as Obj_AI_Minion;

            if (asMinion != null)
            {
                return new double[] { 390, 410, 430, 450, 480, 510, 540, 570, 600, 640, 680, 720, 760, 800, 850, 900, 950, 1000 }[ObjectManager.Player.Level - 1];
            }

            else if (asHero != null)
            {
                if (CurrentSmiteType == SmiteType.RegularSmite)
                {
                    return 0;
                }

                else if (CurrentSmiteType == SmiteType.ChallengingSmite)
                {
                    return 54 + (6 * ObjectManager.Player.Level);
                }

                else if (CurrentSmiteType == SmiteType.ChillingSmite)
                {
                    return 20 + (8 * ObjectManager.Player.Level);
                }
            }

            return 0;
        }

        public Spell.CastStates Cast(Obj_AI_Base target)
        {
            if (!CanCast(target))
            {
                return Spell.CastStates.NotCasted;
            }

            return Smite.Cast(target);
        }
    }
}