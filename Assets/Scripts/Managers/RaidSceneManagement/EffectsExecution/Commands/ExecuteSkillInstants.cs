namespace Managers.RaidSceneManagement.EffectsExecution.Routines
{
    public class ExecuteSkillInstants : ICommand
    {
        private readonly RaidEvents RaidEvents;
        private readonly PrepareDeath _prepareDeathCommand;
        private readonly BattleGround BattleGround;

        public void Execute(FormationUnit performer, SkillTargetInfo targetInfo, SkillResult skillResult)
        {
            foreach (var skillEntry in skillResult.SkillEntries)
            {
                if (skillEntry.IsTargetHit)
                    skillEntry.Target.SetTargetSkillEffect(targetInfo.SkillArtInfo, performer);

                skillEntry.Target.OverlaySlot.UpdateOverlay();

                if (targetInfo.Type == SkillTargetType.Enemy && skillEntry.Target.Character.AtDeathsDoor)
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
                            RaidEvents.ShowPopupMessage(skillEntry.Target, PopupMessageType.Miss);
                            break;
                        case SkillResultType.Dodge:
                            RaidEvents.ShowPopupMessage(skillEntry.Target, PopupMessageType.Dodge);
                            break;
                        case SkillResultType.Hit:
                            if (performer.Character.IsMonster && targetInfo.Skill.DamageMax == 0)
                                break;
                            if (!performer.Character.IsMonster && targetInfo.Skill.DamageMod == -1)
                                break;

                            RaidEvents.ShowPopupMessage(skillEntry.Target, PopupMessageType.Damage,
                                skillEntry.Amount.ToString());
                            break;
                        case SkillResultType.Crit:
                            if (performer.Character.IsMonster && targetInfo.Skill.DamageMax == 0)
                                break;
                            if (!performer.Character.IsMonster && targetInfo.Skill.DamageMod == -1)
                                break;

                            RaidEvents.ShowPopupMessage(skillEntry.Target, PopupMessageType.CritDamage,
                                skillEntry.Amount.ToString());
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

                    if (skillEntry.IsTargetHit && skillEntry.Target.Character.SkillReaction != null &&
                        skillEntry.Target.Character.SkillReaction.WasHitPerformerEffects.Count > 0)
                    {
                        for (int i = 0; i < skillEntry.Target.Character.SkillReaction.WasHitPerformerEffects.Count; i++)
                        for (int j = 0;
                             j < skillEntry.Target.Character.SkillReaction.WasHitPerformerEffects[i].SubEffects.Count;
                             j++)
                            skillEntry.Target.Character.SkillReaction.WasHitPerformerEffects[i].SubEffects[j].Apply(
                                skillEntry.Target,
                                performer, skillEntry.Target.Character.SkillReaction.WasHitPerformerEffects[i]);
                    }

                    if (skillEntry.Target.Character.IsMonster && skillEntry.Target.Character.HasZeroHealth)
                        _prepareDeathCommand.Execute(skillEntry.Target);
                    else if (skillEntry.Target.Character.IsMonster == false &&
                             skillEntry.Target.Character.AtDeathsDoor == false)
                        if (skillEntry.Target.Character.HasZeroHealth)
                            _prepareDeathCommand.Execute(skillEntry.Target);
                }
            }

            for (int i = 0; i < BattleGround.MonsterParty.Units.Count; i++)
                if (BattleGround.MonsterParty.Units[i].CombatInfo.MarkedForDeath)
                    _prepareDeathCommand.Execute(BattleGround.MonsterParty.Units[i]);

            performer.OverlaySlot.UpdateOverlay();
        }
    }
}