using BingoQuest.Gameplay.Bingo;
using BingoQuest.Gameplay.Combat;
using BingoQuest.Gameplay.Loot;
using BingoQuest.Gameplay.Progression;
using BingoQuest.Presentation.UI;
using NUnit.Framework;

namespace BingoQuest.Tests.PlayMode
{
    public class UiPresenterTests
    {
        [Test]
        public void CombatHudPresenter_BuildsHealthCooldownAndStatusText()
        {
            var stats = new CharacterStats { MaxHealth = 120, Health = 90 };
            var actionBar = new ActionBar();
            var ability = new Ability(new AbilityDefinition { Name = "Slash", Cooldown = 2f });
            actionBar.SetAbility(AbilitySlot.Primary, ability);
            ability.TryExecute();

            var effects = new StatusEffectManager();
            effects.ApplyEffect(StatusEffectType.Burn, 3f, 5f);

            var state = CombatHudPresenter.BuildState(stats, actionBar, effects);

            Assert.AreEqual("HP: 90/120", state.HealthText);
            Assert.AreEqual(4, state.CooldownPercents.Length);
            Assert.Less(state.CooldownPercents[0], 1f);
            StringAssert.Contains("Burn", state.StatusEffectsText);
        }

        [Test]
        public void BingoHudPresenter_MapsCardSquares()
        {
            var card = new BingoCard();
            for (int row = 0; row < BingoCard.GRID_SIZE; row++)
            {
                for (int col = 0; col < BingoCard.GRID_SIZE; col++)
                {
                    card.SetSquare(row, col, new BingoSquare(row, col, $"obj_{row}_{col}"));
                }
            }

            card.CompleteSquare(0, 0);
            card.CompleteSquare(4, 4);

            var state = BingoHudPresenter.BuildState(card);
            Assert.AreEqual(2, state.CompletedSquares);
            Assert.IsTrue(state.SquareCompleted[0]);
            Assert.IsTrue(state.SquareCompleted[24]);
            Assert.AreEqual("obj_0_0", state.ObjectiveIds[0]);
        }

        [Test]
        public void ProgressionHudPresenter_ShowsClassAndLevelData()
        {
            var tree = SkillTreeFactory.CreateWarriorTree();
            var progression = new CharacterProgression(BuiltInClasses.Warrior, tree);
            progression.GainExperience(150);

            var state = ProgressionHudPresenter.BuildState(progression);
            StringAssert.Contains("Warrior", state.ClassText);
            StringAssert.Contains("Level: 2", state.LevelText);
            StringAssert.Contains("Skill Points", state.SkillPointText);
        }

        [Test]
        public void InventoryHudPresenter_ShowsItemsRarityAndCurrencies()
        {
            var inventory = new Inventory();
            inventory.AddCurrency("fate_shards", 100);
            inventory.AddItem(new ItemInstance
            {
                Definition = new ItemDefinition { ItemId = "iron_sword", DisplayName = "Iron Sword", Type = ItemType.Weapon, BasePower = 4 },
                Rarity = ItemRarity.Rare,
                ItemLevel = 3,
                RolledPower = 5
            });

            var state = InventoryHudPresenter.BuildState(inventory);
            Assert.AreEqual("Items: 1", state.ItemCountText);
            StringAssert.Contains("Rare: 1", state.RarityBreakdownText);
            StringAssert.Contains("fate_shards=100", state.CurrenciesText);
        }
    }
}

