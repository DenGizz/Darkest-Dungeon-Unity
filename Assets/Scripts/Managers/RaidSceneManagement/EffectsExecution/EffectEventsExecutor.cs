using System;
using System.Collections;

namespace Managers.RaidSceneManagement.EffectsExecution
{
    public class EffectEventsExecutor : IEffectEventsExecutor
    {
        public bool IsExecutingEffectEvent { get; private set; }

        public IEnumerator ExecuteEffectEventsAsync(bool includeMonsters, float waitAfter = 0.0f)
        {
            throw new NotImplementedException();
        }

        public IEnumerator ExecuteHeroSkillAsync(FormationUnit actionUnit, SkillTargetInfo targetInfo, CombatSkill skill)
        {
            throw new NotImplementedException();
        }
    }
}