using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Managers.RaidSceneManagement.EffectsExecution.Routines
{
    public class ExecuteSkillAnimationIntro : ICommand
    {
        private readonly PartyFormationManager Formations;

        public void Execute(FormationUnit performer, SkillTargetInfo targetInfo)
        {
            if (targetInfo.Skill.ValidModes.Count > 1 && targetInfo.Mode != null)
                Formations.UnitSkillIntroOverriden(performer, targetInfo.SkillArtInfo, targetInfo.Mode.Id);
            else
                Formations.UnitSkillIntro(performer, targetInfo.SkillArtInfo);

            if (targetInfo.Type == SkillTargetType.Party)
            {
                foreach (var targetUnit in targetInfo.Targets)
                    if (performer != targetUnit)
                        Formations.UnitBuffedIntro(targetUnit);

                if (targetInfo.Targets.Contains(performer))
                {
                    if (performer.Team == Team.Heroes)
                        Formations.PartyBuffPositions.SetUnitTargets(targetInfo.Targets.OrderByDescending(unit =>
                            unit == performer ? 5 : unit.Rank).ToList(), 0.01f, Vector2.zero);
                    else
                        Formations.PartyBuffPositions.SetUnitTargets(targetInfo.Targets.OrderBy(unit =>
                            unit == performer ? 5 : unit.Rank).ToList(), 0.01f, Vector2.zero);
                }
                else
                {
                    var positionTargets = new List<FormationUnit>(targetInfo.Targets);
                    positionTargets.Insert(0, performer);
                    if (performer.Team == Team.Heroes)
                        Formations.PartyBuffPositions.SetUnitTargets(positionTargets.OrderByDescending(unit =>
                            unit == performer ? 5 : unit.Rank).ToList(), 0.01f, Vector2.zero);
                    else
                        Formations.PartyBuffPositions.SetUnitTargets(positionTargets.OrderBy(unit =>
                            unit == performer ? 5 : unit.Rank).ToList(), 0.01f, Vector2.zero);
                }
            }
            else if (targetInfo.Type == SkillTargetType.Enemy)
            {
                foreach (var targetUnit in targetInfo.Targets)
                    Formations.UnitDefendIntro(targetUnit);

                if (performer.Team == Team.Monsters)
                {
                    if (targetInfo.Skill.Type == "melee")
                        Formations.MonstersAttackMeleePosition.SetUnitTargets(performer, 0.01f,
                            targetInfo.SkillArtInfo.AreaOffset);
                    else
                        Formations.MonstersAttackRangePosition.SetUnitTargets(performer, 0.01f,
                            targetInfo.SkillArtInfo.AreaOffset);

                    Formations.HeroesDefencePositions.SetUnitTargets(targetInfo.Targets.OrderByDescending(unit =>
                        unit.Rank).ToList(), 0.01f, targetInfo.SkillArtInfo.TargetAreaOffset);
                }
                else
                {
                    if (targetInfo.Skill.Type == "melee")
                        Formations.HeroesAttackMeleePosition.SetUnitTargets(performer, 0.01f,
                            targetInfo.SkillArtInfo.AreaOffset);
                    else
                        Formations.HeroesAttackRangePosition.SetUnitTargets(performer, 0.01f,
                            targetInfo.SkillArtInfo.AreaOffset);

                    Formations.MonstersDefencePositions.SetUnitTargets(targetInfo.Targets.OrderBy(unit =>
                        unit.Rank).ToList(), 0.01f, targetInfo.SkillArtInfo.TargetAreaOffset);
                }
            }
            else if (targetInfo.Type == SkillTargetType.Self)
            {
                Formations.PartyBuffPositions.SetUnitTargets(performer, 0.01f, Vector2.zero);
            }
        }
    }
}