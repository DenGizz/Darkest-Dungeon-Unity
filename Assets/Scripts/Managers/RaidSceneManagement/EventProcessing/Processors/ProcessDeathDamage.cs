namespace Managers.RaidSceneManagement.EventProcessing.Processors
{
    public class ProcessDeathDamage
    {
        private readonly BattleGround BattleGround;
        private readonly RaidEvents RaidEvents;
        
        protected bool Process(DeathDamage deathDamage)
        {
            if (deathDamage == null)
                return false;

            var damageTarget = BattleGround.MonsterParty.Units.Find(target =>
                target.Character.Class == deathDamage.TargetBaseClass);

            if (damageTarget == null)
                return false;

            int damage = damageTarget.Character.TakeDamage(deathDamage.TargetDamage);
            RaidEvents.ShowPopupMessage(damageTarget, PopupMessageType.Damage, damage.ToString());

            return true;
        }
    }
}