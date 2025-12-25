using UnityEngine;

namespace Managers.RaidSceneManagement.EventProcessing.Processors
{
    public class ProcessStress
    {
        private readonly RaidSceneManager Instanse;
        private readonly RaidEvents RaidEvents;
        
        public void Process(FormationUnit unit, int stress)
        {
            int damage = Mathf.RoundToInt(stress * (1 + unit.Character[AttributeType.StressDmgReceivedPercent].ModifiedValue));
            if (damage < 1)
                damage = 1;

            unit.Character.Stress.IncreaseValue(damage);

            if (unit.Character.IsOverstressed)
            {
                if (unit.Character.IsVirtued)
                    unit.Character.Stress.CurrentValue = Mathf.Clamp(unit.Character.Stress.CurrentValue, 0, 100);
                else if (!unit.Character.IsAfflicted && unit.Character.IsOverstressed)
                    Instanse.AddResolveCheck(unit);

                if (Mathf.RoundToInt(unit.Character.Stress.CurrentValue) == 200)
                    Instanse.AddHeartAttackCheck(unit);
            }

            unit.OverlaySlot.UpdateOverlay();
            RaidEvents.ShowPopupMessage(unit, PopupMessageType.Stress, damage.ToString());
            unit.SetHalo("afflicted");
        }
    }
}