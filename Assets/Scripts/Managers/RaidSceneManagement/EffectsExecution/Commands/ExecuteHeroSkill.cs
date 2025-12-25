using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Managers.RaidSceneManagement.EffectsExecution.Routines
{
    public class ExecuteHeroSkill : Command
    {
        private readonly RaidEvents RaidEvents;
        private readonly List<FormationUnit> Riposters;
        private readonly PartyFormationManager Formations;
        private readonly RaidPartyCamera DungeonCamera;
        private readonly TorchMeter TorchMeter;
        private readonly ExecuteSkillInstants _executeSkillInstantsCommand;
        private readonly ExecuteSlidingSetup _executeSlidingSetupCommand;
        private readonly ExecuteRiposteSkillActivation _executeRiposteSkillActivationCommand;
        private readonly ExecuteRiposteAnimationIntro _executeRiposteAnimationIntroCommand;
        private readonly ExecuteRiposteInstants _executeRiposteInstantsCommand;
        private readonly ExecuteSkillAnimationOutro _executeSkillAnimationOutroCommand;
        private readonly ExecuteBattlegroundDeaths _executeBattlegroundDeathsCommand;
        private readonly ExecuteDeathDamages _executeDeathDamagesCommand;
        private readonly BattleGround BattleGround;
        private readonly ExecuteEffectEvents _executeEffectEventsCommand;
        private readonly ExecuteGuardRedirection _executeGuardRedirectionCommand;
        private readonly ExecuteSkillBase _executeSkillBaseCommand;
        private readonly ExecuteSkillAnimationIntro _executeSkillAnimationIntroCommand;
        
        public IEnumerator ExecuteAsync(FormationUnit actionUnit, SkillTargetInfo targetInfo, CombatSkill skill)
        {
            RaidEvents.MonsterTooltip.IsDisabled = true;
            RaidEvents.MonsterTooltip.Hide();
            
            _executeGuardRedirectionCommand.Execute(actionUnit, targetInfo);
            
            Formations.HideUnitOverlay();
            TorchMeter.Hide();
            yield return new WaitForSeconds(0.2f);
            DungeonCamera.Zoom(50, 0.05f);
           
            var skillResult =  _executeSkillBaseCommand.Execute(actionUnit, targetInfo);
            yield return new WaitForSeconds(0.05f);
            DungeonCamera.SwitchBlur(true);
            _executeSkillAnimationIntroCommand.Execute(actionUnit, targetInfo);
            yield return new WaitForSeconds(0.01f);
            _executeSkillInstantsCommand.Execute(actionUnit, targetInfo, skillResult);
            yield return new WaitForSeconds(0.01f);
            _executeSlidingSetupCommand.Execute(actionUnit, targetInfo);
            yield return new WaitForSeconds(0.70f);
            _executeRiposteSkillActivationCommand.Execute(actionUnit, targetInfo);
            yield return new WaitForSeconds(0.05f);
            _executeRiposteAnimationIntroCommand.Execute(actionUnit, targetInfo);
            yield return new WaitForSeconds(0.05f);
            _executeRiposteInstantsCommand.Execute(actionUnit);
            yield return new WaitForSeconds(Riposters.Count > 0 ? 1.2f : 0.7f);
            DungeonCamera.Zoom(DungeonCamera.StandardFOV, 0.1f);
            DungeonCamera.SwitchBlur(false);
            _executeSkillAnimationOutroCommand.Execute(actionUnit, targetInfo);

            List<DeathDamage> deathDamages = _executeBattlegroundDeathsCommand.Execute(actionUnit);
            if (deathDamages.Count > 0)
                yield return StartCoroutine(_executeDeathDamagesCommand.ExecuteAsync(deathDamages));

            yield return new WaitForSeconds(0.175f);
            Formations.ShowUnitOverlay();
            TorchMeter.Show();
            Formations.ResetSelections();
            yield return new WaitForSeconds(0.075f);

            if (targetInfo.Type == SkillTargetType.Enemy && skillResult.HasCritEffect)
            {
                DarkestDungeonManager.Data.Effects["Heal Stress 1"].ApplyIndependent(actionUnit);

                for (int j = 0; j < BattleGround.HeroParty.Units.Count; j++)
                    if (BattleGround.HeroParty.Units[j] != actionUnit && RandomSolver.CheckSuccess(0.33f))
                        DarkestDungeonManager.Data.Effects["Heal Stress 1"]
                            .ApplyIndependent(BattleGround.HeroParty.Units[j]);
            }
            else if (skillResult.HasDeadEffect)
                DarkestDungeonManager.Data.Effects["Heal Stress Chance 1"].ApplyIndependent(actionUnit);

            yield return StartCoroutine(_executeEffectEventsCommand.ExecuteAsync(true));

            for (int i = 0; i < targetInfo.Targets.Count; i++)
                BattleSolver.RemoveConditions(targetInfo.Targets[i]);
            BattleSolver.RemoveConditions(actionUnit);

            RaidEvents.MonsterTooltip.IsDisabled = false;

            #region Trait Comment Attack Result

            if (BattleGround.HeroParty.Units.Contains(actionUnit) && BattleGround.HeroParty.Units.Count > 1)
            {
                for (int i = 0; i < actionUnit.Party.Units.Count; i++)
                {
                    if (actionUnit == actionUnit.Party.Units[i] || actionUnit.Party.Units[i].Character.Trait == null)
                        continue;

                    if (targetInfo.Type != SkillTargetType.Enemy)
                        continue;

                    ReactionType reactionType = skillResult.HasHit
                        ? ReactionType.CommentAllyAttackHit
                        : ReactionType.CommentAllyAttackMiss;

                    if (RandomSolver.CheckSuccess(actionUnit.Party.Units[i].Character.Trait.Reactions[reactionType]
                            .Chance))
                    {
                        var barkStressEffect = actionUnit.Party.Units[i].Character.Trait.Reactions[reactionType].Effect;
                        yield return new WaitForSeconds(1f);
                        foreach (SubEffect subEffect in barkStressEffect.SubEffects)
                            subEffect.Apply(actionUnit.Party.Units[i], actionUnit, barkStressEffect);
                        yield return new WaitForSeconds(0.1f);
                        yield return StartCoroutine(_executeEffectEventsCommand.ExecuteAsync(false));
                        break;
                    }
                }
            }

            #endregion
        }
    }
}