using System.Collections.Generic;

namespace Managers.RaidSceneManagement.EffectsExecution.Routines
{
    public class ExecuteSkillAnimationOutro : Command
    {
        private readonly List<FormationUnit> Riposters;
        private readonly List<SkillResult> RiposteResults;
        private PartyFormationManager Formations;

        public void Execute(FormationUnit performer, SkillTargetInfo targetInfo)
        {
            if (RiposteResults.Count > 0)
            {
                performer.SetPerformerSkillAnimation(targetInfo.SkillArtInfo, false);
                performer.SetDefendAnimation(false);

                for (int i = 0; i < RiposteResults.Count; i++)
                {
                    Riposters[i].SetDefendAnimation(false);
                    Riposters[i].SetPerformerSkillAnimation(RiposteResults[i].ArtInfo, false);
                }
            }

            if (targetInfo.Skill.ValidModes.Count > 1 && targetInfo.Mode != null)
                Formations.UnitSkillOutroOverriden(performer, targetInfo.SkillArtInfo, targetInfo.Mode.Id);
            else
                Formations.UnitSkillOutro(performer, targetInfo.SkillArtInfo);

            if (targetInfo.Type == SkillTargetType.Party)
            {
                foreach (var targetUnit in targetInfo.Targets)
                    if (performer != targetUnit)
                        Formations.UnitBuffedOutro(targetUnit);
            }
            else if (targetInfo.Type == SkillTargetType.Enemy)
            {
                foreach (var targetUnit in targetInfo.Targets)
                    Formations.UnitDefendOutro(targetUnit);
            }

            RiposteResults.Clear();
            Riposters.Clear();
        }
    }
}