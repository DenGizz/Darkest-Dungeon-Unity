using System.Collections.Generic;

namespace Managers.RaidSceneManagement.EffectsExecution.Routines
{
    public class ExecuteRiposteSkillActivation : Command
    {
        private readonly List<FormationUnit> Riposters;
        private readonly List<SkillResult> RiposteResults;
        
        public void Execute(FormationUnit performer, SkillTargetInfo targetInfo)
        {
            Riposters.Clear();
            RiposteResults.Clear();

            if (targetInfo.Type != SkillTargetType.Enemy)
                return;

            foreach (var target in targetInfo.Targets)
            {
                if (!target.Character.GetStatusEffect(StatusType.Riposte).IsApplied)
                    continue;
                if (target.CombatInfo.IsDead)
                    continue;

                var riposteSkill = target.Character.RiposteSkill;

                if (riposteSkill == null)
                    continue;

                var riposteArt = target.Character.SkillArtInfo.Find(art => art.SkillId == riposteSkill.Id);
                if (riposteArt == null)
                    continue;

                BattleSolver.SkillResult.Reset();
                BattleSolver.ExecuteSkill(target, performer, riposteSkill, riposteArt);

                Riposters.Add(target);
                RiposteResults.Add(BattleSolver.SkillResult.Copy());

                if (target.Character is Hero)
                {
                    if (target.Character.Mode != null)
                        FMODUnity.RuntimeManager.PlayOneShot("event:/char/ally/" + 
                                                             target.Character.Class + "_" + riposteSkill.Id + "_" + target.Character.Mode.Id);
                    else
                        FMODUnity.RuntimeManager.PlayOneShot("event:/char/ally/" +
                                                             target.Character.Class + "_" + riposteSkill.Id);
                }
                else
                {
                    FMODUnity.RuntimeManager.PlayOneShot("event:/char/enemy/" + 
                                                         target.Character.Class + "_" + riposteSkill.Id);
                }
            }
        }
    }
}