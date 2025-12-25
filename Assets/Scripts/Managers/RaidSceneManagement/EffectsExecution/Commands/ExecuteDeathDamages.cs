using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Managers.RaidSceneManagement.EffectsExecution.Routines
{
    public class ExecuteDeathDamages : ICommand
    {
        private readonly BattleGround _battleGround;
        private readonly RaidEvents _raidEvents;

        public ExecuteDeathDamages(BattleGround battleGround, RaidEvents raidEvents)
        {
            _battleGround = battleGround;
            _raidEvents = raidEvents;
        }

        public IEnumerator ExecuteAsync(IEnumerable<DeathDamage> deathDamages)
        {
            foreach (var t in deathDamages)
            {
                var deathDamageTarget =
                    _battleGround.MonsterParty.Units.Find(unit => unit.Character.Class == t.TargetBaseClass) 
                    ?? _battleGround.HeroParty.Units.Find(unit => unit.Character.Class == t.TargetBaseClass);

                if (deathDamageTarget == null)
                    continue;

                int damage = deathDamageTarget.Character.TakeDamage(t.TargetDamage);
                deathDamageTarget.OverlaySlot.UpdateOverlay();
                _raidEvents.ShowPopupMessage(deathDamageTarget, PopupMessageType.Damage, damage.ToString());
                deathDamageTarget.SetDefendAnimation(true);
                yield return new WaitForSeconds(0.8f);
                deathDamageTarget.SetDefendAnimation(false);
            }
        }
    }
}