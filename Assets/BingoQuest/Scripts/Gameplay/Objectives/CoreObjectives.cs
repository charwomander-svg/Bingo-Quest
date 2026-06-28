namespace BingoQuest.Gameplay.Objectives
{
    public class KillEnemiesObjective : ObjectiveBase
    {
        public override ObjectiveCategory Category => ObjectiveCategory.Combat;

        public KillEnemiesObjective(string id, int count)
        {
            ObjectiveId = id;
            RequiredProgress = count;
        }

        protected override void OnProgressUpdate(in ObjectiveEvent evt)
        {
            if (evt.Type == ObjectiveEventType.Kill)
                CurrentProgress++;
        }
    }

    public class DealDamageObjective : ObjectiveBase
    {
        public override ObjectiveCategory Category => ObjectiveCategory.Combat;

        public DealDamageObjective(string id, int damageTarget)
        {
            ObjectiveId = id;
            RequiredProgress = damageTarget;
        }

        protected override void OnProgressUpdate(in ObjectiveEvent evt)
        {
            if (evt.Type == ObjectiveEventType.Damage)
                CurrentProgress += evt.Amount;
        }
    }

    public class CriticalHitsObjective : ObjectiveBase
    {
        public override ObjectiveCategory Category => ObjectiveCategory.Combat;

        public CriticalHitsObjective(string id, int critCount)
        {
            ObjectiveId = id;
            RequiredProgress = critCount;
        }

        protected override void OnProgressUpdate(in ObjectiveEvent evt)
        {
            if (evt.Type == ObjectiveEventType.CriticalHit)
                CurrentProgress++;
        }
    }

    public class UseAbilitiesObjective : ObjectiveBase
    {
        public override ObjectiveCategory Category => ObjectiveCategory.Combat;

        public UseAbilitiesObjective(string id, int abilityCount)
        {
            ObjectiveId = id;
            RequiredProgress = abilityCount;
        }

        protected override void OnProgressUpdate(in ObjectiveEvent evt)
        {
            if (evt.Type == ObjectiveEventType.AbilityUsed)
                CurrentProgress++;
        }
    }

    public class DodgeActionsObjective : ObjectiveBase
    {
        public override ObjectiveCategory Category => ObjectiveCategory.Survive;

        public DodgeActionsObjective(string id, int dodgeCount)
        {
            ObjectiveId = id;
            RequiredProgress = dodgeCount;
        }

        protected override void OnProgressUpdate(in ObjectiveEvent evt)
        {
            if (evt.Type == ObjectiveEventType.Dodge)
                CurrentProgress++;
        }
    }

    public class ApplyStatusEffectsObjective : ObjectiveBase
    {
        private ElementType targetElement;
        public override ObjectiveCategory Category => ObjectiveCategory.Combat;

        public ApplyStatusEffectsObjective(string id, int effectCount, ElementType element = ElementType.Physical)
        {
            ObjectiveId = id;
            RequiredProgress = effectCount;
            targetElement = element;
        }

        protected override void OnProgressUpdate(in ObjectiveEvent evt)
        {
            if (evt.Type == ObjectiveEventType.StatusEffectApplied)
            {
                if (targetElement == ElementType.Physical || evt.Element == targetElement)
                    CurrentProgress++;
            }
        }
    }

    public class LootItemsObjective : ObjectiveBase
    {
        private int minRarity;
        public override ObjectiveCategory Category => ObjectiveCategory.Loot;

        public LootItemsObjective(string id, int itemCount, int minRarityTier = 0)
        {
            ObjectiveId = id;
            RequiredProgress = itemCount;
            minRarity = minRarityTier;
        }

        protected override void OnProgressUpdate(in ObjectiveEvent evt)
        {
            if (evt.Type == ObjectiveEventType.ItemLooted && evt.Amount >= minRarity)
                CurrentProgress++;
        }
    }

    public class DefeatBossObjective : ObjectiveBase
    {
        private string targetBossId;
        public override ObjectiveCategory Category => ObjectiveCategory.Combat;

        public DefeatBossObjective(string id, string bossId = "ANY")
        {
            ObjectiveId = id;
            RequiredProgress = 1;
            targetBossId = bossId;
        }

        protected override void OnProgressUpdate(in ObjectiveEvent evt)
        {
            if (evt.Type == ObjectiveEventType.BossDefeated)
            {
                if (targetBossId == "ANY" || evt.SourceId == targetBossId)
                    CurrentProgress = 1;
            }
        }
    }

    public class OpenChestsObjective : ObjectiveBase
    {
        public override ObjectiveCategory Category => ObjectiveCategory.Loot;

        public OpenChestsObjective(string id, int chestCount)
        {
            ObjectiveId = id;
            RequiredProgress = chestCount;
        }

        protected override void OnProgressUpdate(in ObjectiveEvent evt)
        {
            if (evt.Type == ObjectiveEventType.ChestOpened)
                CurrentProgress++;
        }
    }
}
