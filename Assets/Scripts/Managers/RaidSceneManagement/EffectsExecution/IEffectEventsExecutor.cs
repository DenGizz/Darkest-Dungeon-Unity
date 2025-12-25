using System.Collections;

namespace Managers.RaidSceneManagement.EffectsExecution
{
    public interface IEffectEventsExecutor
    {
        bool IsExecutingEffectEvent { get; }
        IEnumerator ExecuteEffectEventsAsync(bool includeMonsters, float waitAfter = 0.0f);
        IEnumerator ExecuteHeroSkillAsync(FormationUnit actionUnit, SkillTargetInfo targetInfo, CombatSkill skill);
        IEnumerator ExecuteDeath(FormationUnit targetUnit);
    }
}