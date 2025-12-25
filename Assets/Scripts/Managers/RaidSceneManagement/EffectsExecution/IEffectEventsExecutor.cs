using System.Collections;
using System.Collections.Generic;

namespace Managers.RaidSceneManagement.EffectsExecution
{
    public interface IEffectEventsExecutor
    {
        bool IsExecutingEffectEvent { get; }
        IEnumerator ExecuteEffectEventsAsync(bool includeMonsters, float waitAfter = 0.0f);
        IEnumerator ExecuteHeroSkillAsync(FormationUnit actionUnit, SkillTargetInfo targetInfo, CombatSkill skill);
        IEnumerator ExecuteDeathAsync(FormationUnit targetUnit);

        bool PrepareDeath(FormationUnit targetUnit, DeathFactor deathFactor = DeathFactor.AttackMonster,
            FormationUnit killer = null);

        void ExecuteGuardRedirection(FormationUnit performer, SkillTargetInfo targetInfo);

        SkillResult ExecuteSkillBase(FormationUnit performer, SkillTargetInfo targetInfo);
        void ExecuteSkillAnimationIntro(FormationUnit performer, SkillTargetInfo targetInfo);
        void ExecuteSkillInstants(FormationUnit actionUnit, SkillTargetInfo brainDecisionTargetInfo, SkillResult skillResult);
        void ExecuteSlidingSetup(FormationUnit actionUnit, SkillTargetInfo brainDecisionTargetInfo);
        void ExecuteRiposteSkillActivation(FormationUnit actionUnit, SkillTargetInfo brainDecisionTargetInfo);
        void ExecuteRiposteAnimationIntro(FormationUnit actionUnit, SkillTargetInfo brainDecisionTargetInfo);
        void ExecuteRiposteInstants(FormationUnit actionUnit);
        void ExecuteSkillAnimationOutro(FormationUnit actionUnit, SkillTargetInfo brainDecisionTargetInfo);
        List<DeathDamage> ExecuteBattlegroundDeaths(FormationUnit actionUnit);
        IEnumerator ExecuteDeathDamages(List<DeathDamage> deathDamages);
    };
}