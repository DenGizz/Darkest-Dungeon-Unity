using System.Collections.Generic;

namespace Managers.RaidSceneManagement.EffectsExecution.Routines
{
    public class SummonPurging : Command
    {
        private readonly PrepareDeath _prepareDeathCommand;
        private readonly List<FormationUnit> UnitEventQueue;
        private readonly PartyFormationManager Formations;
        private readonly BattleGround BattleGround;
        
        public void Execute(FormationUnit targetUnit)
        {
            targetUnit.SetSortingOrder(4);
            _prepareDeathCommand.Execute(targetUnit);
            UnitEventQueue.RemoveAll(item => item == targetUnit);
            BattleGround.UnitDestroyed(targetUnit);
            Formations.Monsters.DeleteUnitDelayed(targetUnit, 1.867f);
        }
    }
}