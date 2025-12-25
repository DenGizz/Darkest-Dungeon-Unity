namespace Managers.RaidSceneManagement.EffectsExecution.Routines
{
    public class ExecuteSlidingSetup : Command
    {
        private readonly PartyFormationManager Formations;

        public void Execute(FormationUnit performer, SkillTargetInfo targetInfo)
        {
            if (performer.Team == Team.Monsters)
            {
                if (targetInfo.Type == SkillTargetType.Party)
                    Formations.PartyBuffPositions.SetSpacing(120, 1f);
                else if (targetInfo.Type == SkillTargetType.Enemy)
                {
                    if (targetInfo.Skill.Type == "melee")
                        Formations.MonstersAttackMeleePosition.SetSliding(-150, 1f);
                    else
                        Formations.MonstersAttackRangePosition.SetSliding(120, 1f);

                    Formations.HeroesDefencePositions.SetSliding(-120, 1f);
                }
                else if (targetInfo.Type == SkillTargetType.Self)
                    Formations.PartyBuffPositions.SetSpacing(120, 1f);
            }
            else
            {
                if (targetInfo.Type == SkillTargetType.Party)
                    Formations.PartyBuffPositions.SetSpacing(120, 1f);
                else if (targetInfo.Type == SkillTargetType.Enemy)
                {
                    if (targetInfo.Skill.Type == "melee")
                        Formations.HeroesAttackMeleePosition.SetSliding(150, 1f);
                    else
                        Formations.HeroesAttackRangePosition.SetSliding(-120, 1f);

                    Formations.MonstersDefencePositions.SetSliding(120, 1f);
                }
                else if (targetInfo.Type == SkillTargetType.Self)
                    Formations.PartyBuffPositions.SetSpacing(120, 1f);
            }
        }
    }
}