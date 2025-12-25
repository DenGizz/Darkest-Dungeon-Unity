using System.Collections.Generic;
using UnityEngine;

namespace Managers.RaidSceneManagement.EffectsExecution.Routines
{
    public class ExecuteDeath : Command
    {
        private readonly RaidEvents RaidEvents;
        private readonly BattleGround BattleGround;
        private readonly List<FormationUnit> UnitEventQueue;
        private readonly PartyFormationManager Formations;
        private readonly ExecuteDeath _executeDeathCommand;
        private readonly RaidInfo CurrentRaid;
        private readonly List<FormationUnit> ResolveCheckQueue;
        private readonly List<FormationUnit> HeartAttackCheckQueue;
        private readonly List<FormationUnit> DeathDoorEnterQueue;
        private readonly RaidPanel RaidPanel;

        public void Execute(FormationUnit targetUnit)
        {
            if (targetUnit.CombatInfo.IsDead)
            {
                if (targetUnit.Character.IsMonster)
                {
                    if (RaidEvents.MonsterTooltip.Slot == targetUnit.OverlaySlot)
                        RaidEvents.MonsterTooltip.Hide();
                    Monster monster = targetUnit.Character as Monster;

                    if (monster.SkillReaction != null && monster.SkillReaction.WasKilledOtherMonstersEffects.Count > 0)
                    {
                        for (int i = 0; i < targetUnit.Party.Units.Count; i++)
                        for (int j = 0; j < monster.SkillReaction.WasKilledOtherMonstersEffects.Count; j++)
                        for (int k = 0;
                             k < monster.SkillReaction.WasKilledOtherMonstersEffects[j].SubEffects.Count;
                             k++)
                            monster.SkillReaction.WasKilledOtherMonstersEffects[j].SubEffects[k].Apply(
                                targetUnit.Party.Units[i], targetUnit.Party.Units[i],
                                monster.SkillReaction.WasKilledOtherMonstersEffects[j]);
                    }

                    var companionRecord = BattleGround.Companions.Find(record =>
                        record.TargetUnit == targetUnit || record.CompanionUnit == targetUnit);
                    if (companionRecord != null)
                    {
                        BattleGround.Companions.Remove(companionRecord);

                        if (companionRecord.CompanionUnit == targetUnit)
                        {
                            foreach (var buff in companionRecord.CompanionComponent.Buffs)
                                companionRecord.TargetUnit.Character.RemoveSourceBuff(buff, BuffSourceType.Adventure);
                            companionRecord.TargetUnit.OverlaySlot.UpdateOverlay();
                        }
                    }

                    if (monster.Data.ControllerCaptor != null)
                    {
                        var controlRecord = BattleGround.Controls.Find(control => control.ControllUnit == targetUnit);
                        if (controlRecord != null)
                            BattleGround.UncontrolUnit(controlRecord);
                    }

                    if (monster.Data.FullCaptor != null)
                    {
                        var captureRecord = BattleGround.Captures.Find(capture => capture.CaptorUnit == targetUnit);
                        if (captureRecord != null)
                            BattleGround.ReleaseUnit(captureRecord);

                        if (monster.Data.LifeLink != null &&
                            BattleGround.IsLifeLinked(targetUnit, monster.Data.LifeLink))
                        {
                            MonsterData emptyCaptorData =
                                DarkestDungeonManager.Data.Monsters[monster.Data.FullCaptor.EmptyMonsterClass];
                            GameObject unitObject =
                                Resources.Load("Prefabs/Monsters/" + emptyCaptorData.TypeId) as GameObject;
                            BattleGround.ReplaceUnit(emptyCaptorData, targetUnit, unitObject);
                        }
                        else
                        {
                            UnitEventQueue.RemoveAll(item => item == targetUnit);
                            BattleGround.UnitDestroyed(targetUnit);
                            Formations.Monsters.DeleteUnit(targetUnit);
                        }
                    }
                    else if (monster.Data.DeathClass != null)
                    {
                        if (monster.Data.DeathClass.Type == DeathClassType.Corpse)
                        {
                            targetUnit.SetCorpseAnimation(true);
                            BattleGround.UnitCorpsed(targetUnit);
                            Formations.Monsters.SpawnCorpse(targetUnit,
                                new Monster(DarkestDungeonManager.Data.Monsters[monster.Data.DeathClass.CorpseClass]));
                        }
                        else
                        {
                            var deathClass = monster.Data.DeathClass;
                            UnitEventQueue.RemoveAll(item => item == targetUnit);
                            MonsterData replacementData =
                                DarkestDungeonManager.Data.Monsters[monster.Data.DeathClass.CorpseClass];
                            GameObject unitObject =
                                Resources.Load("Prefabs/Monsters/" + replacementData.TypeId) as GameObject;
                            var finalUnit = BattleGround.ReplaceUnit(replacementData, targetUnit, unitObject, false, 1);

                            for (int i = 0; i < deathClass.DeathChangeEffects.Count; i++)
                            for (int j = 0; j < deathClass.DeathChangeEffects[i].SubEffects.Count; j++)
                                deathClass.DeathChangeEffects[i].SubEffects[j].ApplyInstant(finalUnit, finalUnit,
                                    deathClass.DeathChangeEffects[i]);
                        }
                    }
                    else
                    {
                        UnitEventQueue.RemoveAll(item => item == targetUnit);
                        BattleGround.UnitDestroyed(targetUnit);
                        Formations.Monsters.DeleteUnit(targetUnit);

                        if (BattleGround.SharedHealth.IsActive)
                            if (BattleGround.SharedHealth.SharedUnits.Contains(targetUnit))
                                for (int i = 0; i < BattleGround.SharedHealth.SharedUnits.Count; i++)
                                    if (BattleGround.MonsterParty.Units.Contains(
                                            BattleGround.SharedHealth.SharedUnits[i]))
                                        _executeDeathCommand.Execute(BattleGround.SharedHealth.SharedUnits[i]);

                        if (BattleGround.SharedHealth.IsActive)
                            BattleGround.SharedHealth.Reset();
                    }
                }
                else if (targetUnit.Character is Hero)
                {
                    #region Captures and Controls

                    var captureRecord = BattleGround.Captures.Find(capture => capture.PrisonerUnit == targetUnit);
                    if (captureRecord != null)
                    {
                        var monster = captureRecord.CaptorUnit.Character as Monster;
                        BattleGround.ReleaseUnit(captureRecord);

                        if (monster.Data.LifeLink != null &&
                            BattleGround.IsLifeLinked(captureRecord.CaptorUnit, monster.Data.LifeLink))
                        {
                            MonsterData emptyCaptorData =
                                DarkestDungeonManager.Data.Monsters[monster.Data.FullCaptor.EmptyMonsterClass];
                            GameObject unitObject =
                                Resources.Load("Prefabs/Monsters/" + emptyCaptorData.TypeId) as GameObject;
                            BattleGround.ReplaceUnit(emptyCaptorData, captureRecord.CaptorUnit, unitObject);
                        }
                        else
                        {
                            UnitEventQueue.RemoveAll(item => item == targetUnit);
                            BattleGround.UnitDestroyed(targetUnit);
                            Formations.Monsters.DeleteUnit(targetUnit);
                        }
                    }

                    var controlRecord = BattleGround.Controls.Find(control => control.PrisonerUnit == targetUnit);
                    if (controlRecord != null)
                        BattleGround.Controls.Remove(controlRecord);

                    #endregion

                    var heroInfo =
                        CurrentRaid.RaidParty.HeroInfo.Find(info => info.Hero == targetUnit.Character as Hero);
                    heroInfo.IsAlive = false;
                    heroInfo.DeathRecord = new DeathRecord()
                    {
                        HeroClassIndex = heroInfo.Hero.ClassIndexId,
                        HeroName = heroInfo.Hero.Name,
                        KillerName = "necromancer_A",
                        ResolveLevel = heroInfo.Hero.Resolve.Level,
                        Factor = DeathFactor.AttackMonster,
                    };
                    UnitEventQueue.RemoveAll(item => item == targetUnit);
                    ResolveCheckQueue.RemoveAll(item => item == targetUnit);
                    HeartAttackCheckQueue.RemoveAll(item => item == targetUnit);
                    DeathDoorEnterQueue.RemoveAll(item => item == targetUnit);
                    BattleGround.UnitDestroyed(targetUnit);
                    targetUnit.Formation.DeleteUnit(targetUnit);
                    if (RaidPanel.SelectedUnit == targetUnit)
                    {
                        if (Formations.Heroes.Party.Units.Count > 0)
                            Formations.Heroes.Party.Units[0].OverlaySlot.UnitSelected();
                    }

                    foreach (var formationUnit in BattleGround.HeroParty.Units)
                        DarkestDungeonManager.Data.Effects["Stress 2"].ApplyIndependent(formationUnit);
                }
            }
        }
    }
}