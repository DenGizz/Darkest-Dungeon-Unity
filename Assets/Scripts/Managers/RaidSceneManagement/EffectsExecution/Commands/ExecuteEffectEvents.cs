using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Managers.RaidSceneManagement.EffectsExecution.Routines
{
    public class ExecuteEffectEvents : Command
    {
        private readonly List<FormationUnit> DeathDoorEnterQueue;
        private readonly BattleGround BattleGround;
        private readonly RaidEvents RaidEvents;
        private readonly List<FormationUnit> ResolveCheckQueue;
        private readonly List<FormationUnit> HeartAttackCheckQueue;
        private readonly ExecuteResolveChecks _executeResolveChecksCommand;
        private readonly PrepareDeath _prepareDeathCommand;

        private readonly RaidRuleInfo
            Rules; //TODO: Rules instance can be overriden from outer code, need to implement rules locator

        private readonly List<FormationUnit> UnitEventQueue;
        private readonly ExecuteDeath _executeDeathCommand;

        public IEnumerator ExecuteAsync(bool includeMonsters, float waitAfter = 0.0f)
        {
            //TODO: In source code there is state control over this variable
            /*executingEffectEvent = true;*/
            bool executedEvent;

            for (int i = 0; i < BattleGround.HeroParty.Units.Count; i++)
                BattleGround.HeroParty.Units[i].StackEvents();
            if (includeMonsters)
                for (int i = 0; i < BattleGround.MonsterParty.Units.Count; i++)
                    BattleGround.MonsterParty.Units[i].StackEvents();

            do
            {
                #region Death Doors

                if (DeathDoorEnterQueue.Count > 0)
                {
                    if (RaidEvents.CampEvent.ActionType == CampUsageResultType.Skill)
                        RaidEvents.CampEvent.Hide();

                    FMODUnity.RuntimeManager.PlayOneShot("event:/general/combat/deaths_door");
                    DarkestSoundManager.ExecuteNarration("deaths_door", NarrationPlace.Raid);

                    foreach (var deathDoorUnit in DeathDoorEnterQueue)
                    {
                        if (deathDoorUnit.Character.AtDeathsDoor)
                            Debug.LogError("Already at deaths door!");
                        (deathDoorUnit.Character as Hero).ApplyDeathDoor();
                        deathDoorUnit.Character.ApplySingleBuffRule(Rules.GetIdleUnitRules(deathDoorUnit),
                            BuffRule.DeathsDoor);
                        deathDoorUnit.SetHalo("deaths_door");
                        deathDoorUnit.OverlaySlot.UpdateOverlay();
                        DarkestDungeonManager.Data.Effects["BarkStress"].ApplyIndependent(deathDoorUnit);
                    }

                    string deathDoorHeroes = null;
                    switch (DeathDoorEnterQueue.Count)
                    {
                        case 1:
                            deathDoorHeroes = string.Format(LocalizationManager.GetString("str_party_members_1_death"),
                                DeathDoorEnterQueue[0].Character.Name);
                            break;
                        case 2:
                            deathDoorHeroes = string.Format(LocalizationManager.GetString("str_party_members_2_death"),
                                DeathDoorEnterQueue[0].Character.Name, DeathDoorEnterQueue[1].Character.Name);
                            break;
                        case 3:
                            deathDoorHeroes = string.Format(LocalizationManager.GetString("str_party_members_3_death"),
                                DeathDoorEnterQueue[0].Character.Name, DeathDoorEnterQueue[1].Character.Name,
                                DeathDoorEnterQueue[2].Character.Name);
                            break;
                        case 4:
                            deathDoorHeroes = string.Format(LocalizationManager.GetString("str_party_members_4_death"),
                                DeathDoorEnterQueue[0].Character.Name, DeathDoorEnterQueue[1].Character.Name,
                                DeathDoorEnterQueue[2].Character.Name, DeathDoorEnterQueue[3].Character.Name);
                            break;
                        default:
                            Debug.LogError("Too much deathdoors!");
                            break;
                    }

                    if (DeathDoorEnterQueue.Count > 1)
                        RaidEvents.ShowAnnouncment(
                            string.Format(LocalizationManager.GetString("str_ui_deathdoor_multy"), deathDoorHeroes));
                    else
                        RaidEvents.ShowAnnouncment(string.Format(LocalizationManager.GetString("str_ui_deathdoor"),
                            deathDoorHeroes));

                    DeathDoorEnterQueue.Clear();
                    yield return new WaitForSeconds(1.6f);
                    RaidEvents.HideAnnouncment();
                }

                #endregion

                #region Effects

                do
                {
                    executedEvent = false;
                    UnitEventQueue.Clear();
                    UnitEventQueue.AddRange(BattleGround.HeroParty.Units);
                    if (includeMonsters)
                        UnitEventQueue.AddRange(BattleGround.MonsterParty.Units);

                    while (UnitEventQueue.Count > 0)
                    {
                        var eventUnit = UnitEventQueue[0];
                        UnitEventQueue.Remove(eventUnit);

                        if (eventUnit != null && eventUnit.EventQueue.Count > 0)
                        {
                            var eventEffect = eventUnit.EventQueue[0];
                            eventUnit.EventQueue.RemoveAt(0);

                            eventEffect.Execute();
                            executedEvent = true;
                            if (eventEffect.SubEffect is StressEffect || eventEffect.SubEffect is StressHealEffect)
                                yield return new WaitForSeconds(0.25f);

                            if (eventUnit.CombatInfo.MarkedForDeath)
                            {
                                eventUnit.CombatInfo.IsDead = true;
                                _executeDeathCommand.Execute(eventUnit);
                            }
                        }
                    }

                    if (executedEvent)
                        yield return new WaitForSeconds(1f);
                } while (executedEvent);

                #endregion

                #region Resolve Checks

                if (ResolveCheckQueue.Count != 0)
                {
                    executedEvent = true;
                    if (RaidEvents.CampEvent.ActionType == CampUsageResultType.Skill)
                        RaidEvents.CampEvent.Hide();
                    yield return StartCoroutine(_executeResolveChecksCommand.ExecuteAsync());
                }

                #endregion

                #region Heart Attacks

                while (HeartAttackCheckQueue.Count > 0)
                {
                    executedEvent = true;
                    var heartAttackedUnit = HeartAttackCheckQueue[0];
                    HeartAttackCheckQueue.RemoveAt(0);

                    if (heartAttackedUnit.Character.AtDeathsDoor)
                    {
                        heartAttackedUnit.CombatInfo.MarkedForDeath = true;
                        _prepareDeathCommand.Execute(heartAttackedUnit);
                        RaidEvents.ShowPopupMessage(heartAttackedUnit, PopupMessageType.HeartAttack, "", 100);
                        yield return new WaitForSeconds(0.2f);
                        RaidEvents.ShowPopupMessage(heartAttackedUnit, PopupMessageType.DeathBlow);
                        yield return new WaitForSeconds(1.2f);
                        _executeDeathCommand.Execute(heartAttackedUnit);
                        yield return new WaitForSeconds(0.2f);
                    }
                    else
                    {
                        RaidEvents.ShowPopupMessage(heartAttackedUnit, PopupMessageType.HeartAttack, "", 100);
                        heartAttackedUnit.Character.TakeDamagePercent(1.0f);
                        heartAttackedUnit.Character.Stress.ValueRatio = 0.75f;
                        heartAttackedUnit.OverlaySlot.UpdateOverlay();
                        yield return new WaitForSeconds(0.2f);
                        DeathDoorEnterQueue.Add(heartAttackedUnit);
                    }
                }

                #endregion

                if (DeathDoorEnterQueue.Count != 0 || ResolveCheckQueue.Count != 0 || HeartAttackCheckQueue.Count != 0)
                    executedEvent = true;
            } while (executedEvent);

            for (int i = 0; i < BattleGround.HeroParty.Units.Count; i++)
                BattleGround.HeroParty.Units[i].Character.ApplyAllBuffRules(
                    Rules.GetIdleUnitRules(BattleGround.HeroParty.Units[i]));

            if (waitAfter > 0.0f)
                yield return new WaitForSeconds(waitAfter);

            //TODO: In source code there is state control over this variable
            /*executingEffectEvent = true;*/
        }
    }
}