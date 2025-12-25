using System.Collections;
using Managers.RaidSceneManagement.EffectsExecution;

namespace Managers.RaidSceneManagement.EventProcessing.Processors
{
    public class ProcessTransformationsAfterBattle : Processor
    {
        private readonly PartyFormationManager Formations;
        private readonly IEffectEventsExecutor _effectEventsExecutor;
        
        protected IEnumerator ProcessAsync()
        {
            foreach (FormationUnit unit in Formations.Heroes.Party.Units)
            {
                var hero = (Hero)unit.Character;
                if (hero.Mode == null || hero.Mode.AfflictionSkillId == null)
                    continue;

                var skill = hero.SelectedCombatSkills.Find(s => s.Id == hero.Mode.BattleCompleteSkillId);
                if (skill == null)
                    continue;

                SkillTargetInfo targetInfo = BattleSolver.SelectSkillTargets(unit, unit, skill).UpdateSkillInfo(unit, skill);
                yield return StartCoroutine(_effectEventsExecutor.ExecuteHeroSkillAsync(unit, targetInfo, skill));
            }
        }
    }
}