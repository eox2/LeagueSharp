using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace SephKhazix
{
    class JumpManager
    {

        public Khazix K6; 

        public JumpManager(Khazix K6)
        {
            this.K6 = K6;
        }

        internal bool HaveEvolvedJump { get { return K6.EvolvedE; } }

        internal Vector3 PreJumpPos;

        internal bool MidAssasination;

        internal float startAssasinationTick;

        internal Obj_AI_Hero AssasinationTarget;


        public void Assasinate()
        {
            if (HaveEvolvedJump)
            {
                if (MidAssasination)
                {
                    if (AssasinationTarget == null || AssasinationTarget.IsDead || Utils.TickCount - startAssasinationTick > 2500)
                    {
                        MidAssasination = false;

                        if (SpellSlot.E.IsReady())
                        {
                            var posmode = Helper.Config.GetAssinationMode();
                            var point = posmode == KhazixMenu.AssasinationMode.ToOldPos ? PreJumpPos : Game.CursorPos;
                            if (point.Distance(ObjectManager.Player.ServerPosition) > K6.E.Range)
                            {
                                PreJumpPos = ObjectManager.Player.ServerPosition.Extend(point, K6.E.Range);
                            }

                            K6.E.Cast(point);
                        }
                    }

                    else
                    {

                        K6.AssasinationCombo(AssasinationTarget);
                    }
                }

                else if (SpellSlot.E.IsReady())
                {
                    var selT = TargetSelector.GetSelectedTarget();
                    var bestEnemy = selT != null && selT.IsInRange(K6.E.Range) && K6.GetBurstDamage(selT) >= selT.Health ? selT : HeroManager.Enemies.Where(x => x.IsValidTarget(K6.E.Range) && x.Health <= K6.GetBurstDamage(x)).MaxOrDefault(x => TargetSelector.GetPriority(x));
                    if (bestEnemy != null)
                    {
                        PreJumpPos = ObjectManager.Player.ServerPosition;
                        K6.E.Cast(bestEnemy.ServerPosition);
                        AssasinationTarget = bestEnemy;
                        MidAssasination = true;
                        startAssasinationTick = Utils.TickCount;
                        Utility.DelayAction.Add(2500, () => MidAssasination = false);
                    }
                }
            }
        }
    }
}
