using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Managers.RaidSceneManagement.EffectsExecution.Routines
{
    public class ExecuteResolveChecks : Command
    {
        private readonly List<FormationUnit> ResolveCheckQueue;
        private readonly PartyFormationManager Formations;
        private readonly RaidEvents RaidEvents;
        private readonly RaidPartyCamera DungeonCamera;
        private readonly RaidRuleInfo Rules; //TODO: Rules instance can be overriden from outer code, need to implement rules locator

        private readonly ExecuteHeroSkill _executeHeroSkillCommand;
        
       public IEnumerator ExecuteAsync()
        {
            while (ResolveCheckQueue.Count > 0)
            {
                var resolveUnit = ResolveCheckQueue[0];
                var resolveHero = resolveUnit.Character as Hero;
                ResolveCheckQueue.RemoveAt(0);
                float virtueChance = 0.25f + resolveUnit.Character[AttributeType.ResolveCheckPercent].ModifiedValue;
                virtueChance = Mathf.Clamp(virtueChance, 0.01f, 0.6f);
                bool isVirtue = RandomSolver.CheckSuccess(virtueChance);
                var availableTraits = isVirtue
                    ? DarkestDungeonManager.Data.Traits.FindAll(trait => trait.Type == OverstressType.Virtue)
                    : DarkestDungeonManager.Data.Traits.FindAll(trait => trait.Type == OverstressType.Affliction);
                Trait resolveTrait = availableTraits[RandomSolver.Next(availableTraits.Count)];

                if (!isVirtue)
                    for (int i = 0; i < resolveUnit.Party.Units.Count; i++)
                        if (resolveUnit.Party.Units[i] != resolveUnit)
                            DarkestDungeonManager.Data.Effects["AfflictedAllyStress"]
                                .ApplyIndependent(resolveUnit.Party.Units[i]);

                if (!isVirtue && resolveUnit.Character.Mode != null &&
                    resolveUnit.Character.Mode.AfflictionSkillId != null)
                {
                    var resolveSkill = resolveHero.SelectedCombatSkills.Find(skill =>
                        skill.Id == resolveUnit.Character.Mode.AfflictionSkillId);
                    if (resolveSkill != null)
                    {
                        SkillTargetInfo targetInfo = BattleSolver.SelectSkillTargets(resolveUnit,
                            resolveUnit, resolveSkill).UpdateSkillInfo(resolveUnit, resolveSkill);
                        yield return StartCoroutine(_executeHeroSkillCommand.ExecuteAsync(resolveUnit, targetInfo, resolveSkill));
                    }
                }

                RaidEvents.ShowAnnouncment(string.Format(LocalizationManager.GetString("resolve_test"),
                    resolveUnit.Character.Name));

                FMODUnity.RuntimeManager.PlayOneShot("event:/general/char/resolve_test");
                yield return new WaitForSeconds(1.6f);
                RaidEvents.HideAnnouncment();
                Formations.HideUnitOverlay();
                yield return new WaitForSeconds(0.1f);
                DungeonCamera.SwitchBlur(true);

                Rules.GetIdleUnitRules(resolveUnit);
                resolveHero.ApplyTrait(resolveTrait);
                resolveHero.ApplySingleBuffRule(Rules, BuffRule.Afflicted);
                resolveHero.ApplySingleBuffRule(Rules, BuffRule.Virtued);

                if (isVirtue)
                {
                    DarkestSoundManager.ExecuteNarration("virtue", NarrationPlace.Raid, resolveTrait.Id);
                    FMODUnity.RuntimeManager.PlayOneShot("event:/general/char/resolve_virtue");
                }
                else
                {
                    DarkestSoundManager.ExecuteNarration("afflicted", NarrationPlace.Raid, resolveTrait.Id);
                    FMODUnity.RuntimeManager.PlayOneShot("event:/general/char/resolve_afflict");
                }

                Formations.HeroResolveCheckIntro(resolveUnit, isVirtue);
                Formations.PartyBuffPositions.SetUnitTarget(resolveUnit, 0.05f, Vector2.zero);
                RaidEvents.ShowAnnouncment(
                    isVirtue
                        ? LocalizationManager.GetString("str_virtue_name_" + resolveTrait.Id)
                        : LocalizationManager.GetString("str_affliction_name_" + resolveTrait.Id),
                    AnnouncmentPosition.Bottom);
                if (!Rules.IsDoingCamping)
                    DungeonCamera.Zoom(45, 0.1f);

                yield return new WaitForSeconds(2.45f);
                if (!Rules.IsDoingCamping)
                    DungeonCamera.Zoom(DungeonCamera.StandardFOV, 0.1f);

                Formations.HeroResolveCheckOutro(resolveUnit, isVirtue);
                DungeonCamera.SwitchBlur(false);
                yield return new WaitForSeconds(0.15f);
                if (isVirtue)
                    resolveUnit.Character.Stress.CurrentValue = RandomSolver.Next(20, 40);
                resolveUnit.OverlaySlot.UpdateOverlay();

                RaidEvents.HideAnnouncment();
                Formations.ShowUnitOverlay();
                yield return new WaitForSeconds(0.15f);
            }
        }
    }
}