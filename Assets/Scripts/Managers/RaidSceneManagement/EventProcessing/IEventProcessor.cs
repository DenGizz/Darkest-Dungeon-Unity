using System.Collections;

namespace Managers.RaidSceneManagement.EventProcessing
{
    public interface IEventProcessor
    {
        void ProcessStress(FormationUnit actionUnit, int modeStressPerTurn);
        IEnumerator ProcessTransformationsAfterBattle();
        bool ProcessDamage(FormationUnit unit, int roundToInt);
        bool ProcessDeathDamage(DeathDamage deathDamage);
    }
}