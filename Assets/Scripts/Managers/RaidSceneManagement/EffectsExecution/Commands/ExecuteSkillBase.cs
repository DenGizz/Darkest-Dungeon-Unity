namespace Managers.RaidSceneManagement.EffectsExecution.Routines
{
    public class ExecuteSkillBase : ICommand
    {
        private readonly BattleGround BattleGround;
        private readonly RaidPartyCamera DungeonCamera;

        public SkillResult Execute(FormationUnit performer, SkillTargetInfo targetInfo)
        {
            BattleSolver.SkillResult.Reset();
            BattleGround.LastDamaged.Clear();

            foreach (var targetUnit in targetInfo.Targets)
            {
                BattleSolver.ExecuteSkill(performer, targetUnit, targetInfo.Skill, targetInfo.SkillArtInfo);
                if (BattleSolver.SkillResult.Current.IsTargetHit)
                    BattleGround.LastDamaged.Add(targetUnit.Character.Class);
            }

            var skillResult = BattleSolver.SkillResult.Copy();

            string playSkillEvent;
            string playSkillMissEvent;

            if (performer.Character.IsMonster)
            {
                playSkillEvent = "event:/char/enemy/" + performer.Character.Class + "_" + targetInfo.Skill.Id;
                playSkillMissEvent = "event:/char/enemy/" + performer.Character.Class + "_" + targetInfo.Skill.Id +
                                     "_miss";
            }
            else if (performer.Character.Mode != null)
            {
                playSkillEvent = "event:/char/ally/" + performer.Character.Class + "_" +
                                 targetInfo.Skill.Id + "_" + performer.Character.Mode.Id;
                playSkillMissEvent = "event:/char/ally/" + performer.Character.Class + "_" +
                                     targetInfo.Skill.Id + "_miss" + "_" + performer.Character.Mode.Id;
            }
            else
            {
                playSkillEvent = "event:/char/ally/" + performer.Character.Class + "_" + targetInfo.Skill.Id;
                playSkillMissEvent = playSkillEvent + "_miss";
            }

            if (skillResult.HasHit && FMODUnity.RuntimeManager.GetEventDescription(playSkillEvent) != null)
                FMODUnity.RuntimeManager.PlayOneShot(playSkillEvent, DungeonCamera.Transform.position);
            else if (FMODUnity.RuntimeManager.GetEventDescription(playSkillMissEvent) != null)
                FMODUnity.RuntimeManager.PlayOneShot(playSkillMissEvent, DungeonCamera.Transform.position);

            if (skillResult.HasCritEffect && targetInfo.Type == SkillTargetType.Enemy)
                DarkestSoundManager.ExecuteNarration(performer.Character.IsMonster ? "crit_hero" : "crit_monster",
                    NarrationPlace.Raid);

            return skillResult;
        }
    }
}