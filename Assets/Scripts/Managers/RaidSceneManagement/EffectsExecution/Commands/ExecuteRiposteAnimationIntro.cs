using System.Collections.Generic;

namespace Managers.RaidSceneManagement.EffectsExecution.Routines
{
    public class ExecuteRiposteAnimationIntro : Command
    {
        private readonly List<SkillResult> RiposteResults;
        private readonly List<FormationUnit> Riposters;
        
        public void Execute(FormationUnit performer, SkillTargetInfo targetInfo)
        {
            if (RiposteResults.Count <= 0) 
                return;
            
            performer.SetPerformerSkillAnimation(targetInfo.SkillArtInfo, false);
            performer.SetDefendAnimation(true);

            for (int i = 0; i < RiposteResults.Count; i++)
            {
                Riposters[i].SetDefendAnimation(false);
                Riposters[i].SetPerformerSkillAnimation(RiposteResults[i].ArtInfo, true);
            }
        }
    }
}