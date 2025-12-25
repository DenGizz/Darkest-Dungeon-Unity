using System.Collections.Generic;

namespace Managers.RaidSceneManagement.EffectsExecution.Routines
{
    public class ExecuteRiposteInstants : Command
    {
        private readonly List<FormationUnit> Riposters;
        private readonly List<SkillResult> RiposteResults;
        private readonly PrepareDeath _prepareDeathCommand;
        private readonly RaidEvents RaidEvents;
        private readonly List<FormationUnit> DeathDoorEnterQueue;

        public void Execute(FormationUnit performer)
        {
            for (int i = 0; i < Riposters.Count; i++)
            {
                foreach (var skillEntry in RiposteResults[i].SkillEntries)
                {
                    if (skillEntry.Target.CombatInfo.IsDead)
                        break;

                    skillEntry.Target.OverlaySlot.UpdateOverlay();

                    if (skillEntry.IsHarmful && skillEntry.Target.Character.AtDeathsDoor)
                    {
                        if (_prepareDeathCommand.Execute(skillEntry.Target))
                        {
                            RaidEvents.ShowPopupMessage(skillEntry.Target, PopupMessageType.DeathBlow);
                        }
                        else
                        {
                            RaidEvents.ShowPopupMessage(skillEntry.Target, PopupMessageType.DeathsDoor);
                        }
                    }
                    else
                    {
                        switch (skillEntry.Type)
                        {
                            case SkillResultType.Miss:
                                RaidEvents.ShowPopupMessage(skillEntry.Target, PopupMessageType.Miss, "", 40 * i);
                                break;
                            case SkillResultType.Dodge:
                                RaidEvents.ShowPopupMessage(skillEntry.Target, PopupMessageType.Dodge, "", 40 * i);
                                break;
                            case SkillResultType.Hit:
                                if (skillEntry.Amount < 1)
                                    RaidEvents.ShowPopupMessage(skillEntry.Target, PopupMessageType.ZeroDamage, "",
                                        40 * i);
                                else
                                    RaidEvents.ShowPopupMessage(skillEntry.Target,
                                        PopupMessageType.Damage, skillEntry.Amount.ToString(), 40 * i);
                                break;
                            case SkillResultType.Crit:
                                if (skillEntry.Amount < 1)
                                    RaidEvents.ShowPopupMessage(skillEntry.Target, PopupMessageType.ZeroDamage, "",
                                        40 * i);
                                else
                                    RaidEvents.ShowPopupMessage(skillEntry.Target,
                                        PopupMessageType.CritDamage, skillEntry.Amount.ToString(), 40 * i);
                                break;
                            case SkillResultType.Heal:
                                RaidEvents.ShowPopupMessage(skillEntry.Target, PopupMessageType.Heal,
                                    skillEntry.Amount.ToString());
                                FMODUnity.RuntimeManager.PlayOneShot("event:/general/status/heal_ally");
                                break;
                            case SkillResultType.CritHeal:
                                RaidEvents.ShowPopupMessage(skillEntry.Target, PopupMessageType.CritHeal,
                                    skillEntry.Amount.ToString());
                                FMODUnity.RuntimeManager.PlayOneShot("event:/general/status/heal_ally_crit");
                                break;
                        }

                        if (skillEntry.IsZeroed && (skillEntry.Type == SkillResultType.Hit ||
                                                    skillEntry.Type == SkillResultType.Crit))
                        {
                            if (skillEntry.Target.Character.IsMonster)
                                _prepareDeathCommand.Execute(skillEntry.Target);
                            else if (!skillEntry.Target.Character.AtDeathsDoor &&
                                     !DeathDoorEnterQueue.Contains(skillEntry.Target))
                                _prepareDeathCommand.Execute(skillEntry.Target);
                        }
                    }
                }

                performer.OverlaySlot.UpdateOverlay();
            }
        }
    }
}