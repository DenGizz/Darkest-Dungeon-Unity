using System.Collections.Generic;
using UnityEngine;

namespace Managers.RaidSceneManagement.EffectsExecution.Routines
{
    public class PrepareDeath : Command
    {
        private readonly BattleGround BattleGround;
        
        private readonly List<FormationUnit> DeathDoorEnterQueue;
        
        public bool Execute(FormationUnit targetUnit, DeathFactor deathFactor = DeathFactor.AttackMonster,
            FormationUnit killer = null)
        {
            if (targetUnit.Character.IsMonster)
            {
                if (targetUnit.CombatInfo.IsDead)
                    return true;
                if (targetUnit.Character.DeathClass != null &&
                    targetUnit.Character.DeathClass.CanDieFromDamage == false)
                    return false;

                targetUnit.CombatInfo.IsDead = true;

                Monster monster = targetUnit.Character as Monster;

                if (BattleGround.SharedHealth.IsActive)
                    if (BattleGround.SharedHealth.SharedUnits.Contains(targetUnit))
                        for (int i = 0; i < BattleGround.SharedHealth.SharedUnits.Count; i++)
                            if (BattleGround.SharedHealth.SharedUnits[i].CombatInfo.IsDead == false)
                                /*PrepareDeath*/Execute(BattleGround.SharedHealth.SharedUnits[i]);

                if (monster.Data.FullCaptor != null &&
                    BattleGround.Captures.Find(capture => capture.CaptorUnit == targetUnit) != null)
                {
                    targetUnit.SetReleaseAnimation(true);
                    targetUnit.SetDefendAnimation(false);
                }
                else if (monster.Types.Contains(MonsterType.Corpse))
                    targetUnit.SetCorpseKillAnimation(true);
                else
                    targetUnit.SetDeathAnimation(true);

                if (monster.Data.FullCaptor != null)
                    FMODUnity.RuntimeManager.PlayOneShot("event:/general/char/death_enemy");

                FMODUnity.RuntimeManager.PlayOneShot("event:/char/enemy/" + monster.Data.TypeId + "_vo_death");

                if (!monster.MonsterTypes.Contains(MonsterType.Corpse))
                    DarkestSoundManager.ExecuteNarration("kill_monster", NarrationPlace.Raid,
                        monster.Class, monster.Size > 1 ? "strong" : "weak", monster.Size > 1 ? "big" : "small",
                        (deathFactor == DeathFactor.BleedMonster || deathFactor == DeathFactor.PoisonMonster)
                            ? "dot"
                            : "one_shot");

                if (monster.Data.FullCaptor == null)
                {
                    GameObject deathFx = Instantiate(Resources.Load("Prefabs/Effects/" +
                                                                    monster.CommonEffects.DeathEffect) as GameObject);
                    AnimatedEffect effect = deathFx.GetComponent<AnimatedEffect>();
                    effect.gameObject.layer = targetUnit.CurrentState.gameObject.layer;
                    effect.BindToTarget(targetUnit, targetUnit.SkeletonAnimations[1], "fxdeath");
                }

                targetUnit.ResetHalo();
                targetUnit.OverlaySlot.Hide();
                return true;
            }
            else
            {
                if (targetUnit.CombatInfo.IsDead)
                    return true;

                Hero hero = targetUnit.Character as Hero;
                if (hero.AtDeathsDoor || targetUnit.CombatInfo.MarkedForDeath)
                {
                    if (RandomSolver.CheckSuccess(hero.DeathResist) && !targetUnit.CombatInfo.MarkedForDeath)
                        return false;
                    targetUnit.SetDeathAnimation(true);
                    FMODUnity.RuntimeManager.PlayOneShot("event:/general/char/death_ally");

                    var captureRecord = BattleGround.Captures.Find(capture => capture.PrisonerUnit == targetUnit);
                    if (captureRecord != null)
                    {
                        captureRecord.CaptorUnit.SetReleaseAnimation(true);
                        captureRecord.CaptorUnit.SetDefendAnimation(false);
                    }

                    GameObject deathFx = Instantiate(Resources.Load("Prefabs/Effects/death_medium") as GameObject);
                    AnimatedEffect effect = deathFx.GetComponent<AnimatedEffect>();
                    effect.gameObject.layer = targetUnit.CurrentState.gameObject.layer;
                    effect.BindToTarget(targetUnit, targetUnit.SkeletonAnimations[1], "fxdeath");
                    targetUnit.ResetHalo();
                    targetUnit.CombatInfo.IsDead = true;
                    targetUnit.OverlaySlot.Hide();

                    DarkestSoundManager.ExecuteNarration("kill_hero", NarrationPlace.Raid);
                    return true;
                }
                else
                {
                    DeathDoorEnterQueue.Add(targetUnit);
                    return false;
                }
            }
        }
    }
}