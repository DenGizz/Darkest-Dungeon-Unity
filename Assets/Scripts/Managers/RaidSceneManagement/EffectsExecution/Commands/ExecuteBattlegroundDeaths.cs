using System.Collections.Generic;

namespace Managers.RaidSceneManagement.EffectsExecution.Routines
{
    public class ExecuteBattlegroundDeaths : Command
    {
        private readonly BattleGround BattleGround;
        private readonly SummonPurging _summonPurgingCommand;
        private readonly ExecuteDeath _executeDeathCommand;
        
        public List<DeathDamage> Execute(FormationUnit peformer)
        {
            List<DeathDamage> deathDamages = new List<DeathDamage>();

            if (peformer.CombatInfo.MarkedForDeath)
            {
                peformer.CombatInfo.IsDead = true;
                List<FormationUnit> lifeLinkedUnits = new List<FormationUnit>();
                for (int i = 0; i < peformer.Party.Units.Count; i++)
                {
                    if (peformer.Party.Units[i].Character.LifeLink != null)
                    {
                        if (peformer.Party.Units[i].Character.LifeLink.LinkBaseClass == peformer.Character.Class)
                            lifeLinkedUnits.Add(peformer.Party.Units[i]);
                    }
                }
                lifeLinkedUnits.ForEach(_summonPurgingCommand.Execute);
                lifeLinkedUnits.Clear();

                if (peformer.CombatInfo.IsDead)
                    if (peformer.Character.DeathDamage != null)
                        deathDamages.Add(peformer.Character.DeathDamage);

                _executeDeathCommand.Execute(peformer);
            }

            for (int i = BattleGround.HeroParty.Units.Count - 1; i >= 0; i--)
            {
                if (BattleGround.HeroParty.Units[i].CombatInfo.IsDead)
                    if (BattleGround.HeroParty.Units[i].Character.DeathDamage != null)
                        deathDamages.Add(BattleGround.HeroParty.Units[i].Character.DeathDamage);

                _executeDeathCommand.Execute(BattleGround.HeroParty.Units[i]);
            }

            for (int i = BattleGround.MonsterParty.Units.Count - 1; i >= 0; i--)
            {
                if (BattleGround.MonsterParty.Units[i].CombatInfo.IsDead)
                    if (BattleGround.MonsterParty.Units[i].Character.DeathDamage != null)
                        deathDamages.Add(BattleGround.MonsterParty.Units[i].Character.DeathDamage);

                _executeDeathCommand.Execute(BattleGround.MonsterParty.Units[i]);
            }

            return deathDamages;
        }
    }
}