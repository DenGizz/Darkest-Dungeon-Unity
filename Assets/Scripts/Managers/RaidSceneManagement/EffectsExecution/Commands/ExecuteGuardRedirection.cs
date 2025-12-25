namespace Managers.RaidSceneManagement.EffectsExecution.Routines
{
    public class ExecuteGuardRedirection : Command
    {
        public void Execute(FormationUnit performer, SkillTargetInfo targetInfo)
        {
            if (targetInfo.Type != SkillTargetType.Enemy) 
                return;
            
            for (int i = targetInfo.Targets.Count - 1; i >= 0; i--)
                if (targetInfo.Targets[i].Character.GetStatusEffect(StatusType.Guarded).IsApplied)
                {
                    var guardedStatus =
                        targetInfo.Targets[i].Character.GetStatusEffect(StatusType.Guarded) as GuardedStatusEffect;
                    if (!targetInfo.Targets.Contains(guardedStatus.Guard))
                        targetInfo.Targets[i] = guardedStatus.Guard;
                }
        }
    }
}