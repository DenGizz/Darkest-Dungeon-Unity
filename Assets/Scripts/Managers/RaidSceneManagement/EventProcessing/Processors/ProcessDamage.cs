using Managers.RaidSceneManagement.EffectsExecution;

namespace Managers.RaidSceneManagement.EventProcessing.Processors
{
    public class ProcessDamage
    {
        private readonly RaidEvents RaidEvents;
        private readonly IEffectEventsExecutor _effectEventsExecutor;
        
        public bool Process(FormationUnit unit, int damage)
        {
            unit.Character.TakeDamage(damage);
            unit.OverlaySlot.UpdateOverlay();

            if (!unit.Character.HasZeroHealth)
                RaidEvents.ShowPopupMessage(unit, PopupMessageType.Damage, damage.ToString());
            else
            {
                bool atDeathDoor = unit.Character.AtDeathsDoor;
                bool isDead = _effectEventsExecutor.PrepareDeath(unit);

                RaidEvents.ShowPopupMessage(unit, atDeathDoor ? (isDead ? PopupMessageType.DeathBlow :
                    PopupMessageType.DeathsDoor) : PopupMessageType.Damage, damage.ToString());

                return isDead;
            }
            return false;
        }
    }
}