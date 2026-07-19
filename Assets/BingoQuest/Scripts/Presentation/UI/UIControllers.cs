using BingoQuest.Gameplay.Bingo;
using BingoQuest.Gameplay.Combat;
using BingoQuest.Gameplay.Loot;
using BingoQuest.Gameplay.Progression;
using UnityEngine;
using UnityEngine.UI;

namespace BingoQuest.Presentation.UI
{
    public class CombatHudController : MonoBehaviour
    {
        [SerializeField] private Combatant playerCombatant;
        [SerializeField] private Text healthText;
        [SerializeField] private Text statusEffectsText;
        [SerializeField] private Image[] abilityCooldownFills = new Image[4];

        private void Update()
        {
            if (playerCombatant == null)
                return;

            var state = CombatHudPresenter.BuildState(playerCombatant.Stats, playerCombatant.ActionBar, playerCombatant.StatusEffects);
            ApplyState(state);
        }

        private void ApplyState(CombatHudState state)
        {
            if (healthText != null)
                healthText.text = state.HealthText;

            if (statusEffectsText != null)
                statusEffectsText.text = state.StatusEffectsText;

            if (abilityCooldownFills == null)
                return;

            int len = Mathf.Min(abilityCooldownFills.Length, state.CooldownPercents.Length);
            for (int i = 0; i < len; i++)
            {
                if (abilityCooldownFills[i] != null)
                    abilityCooldownFills[i].fillAmount = state.CooldownPercents[i];
            }
        }
    }

    public class BingoHudController : MonoBehaviour
    {
        [SerializeField] private BingoSystem bingoSystem;
        [SerializeField] private Text completedText;
        [SerializeField] private Text patternText;
        [SerializeField] private Image[] squareImages = new Image[25];
        [SerializeField] private Color completedColor = new Color(0.25f, 0.8f, 0.35f, 1f);
        [SerializeField] private Color incompleteColor = new Color(0.2f, 0.2f, 0.2f, 1f);

        private void OnEnable()
        {
            if (bingoSystem == null)
                bingoSystem = BingoSystem.Instance;

            if (bingoSystem == null)
                return;

            bingoSystem.OnSquareCompleted += OnSquareCompleted;
            bingoSystem.OnPatternDetected += OnPatternDetected;
            bingoSystem.OnCardReset += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            if (bingoSystem == null)
                return;

            bingoSystem.OnSquareCompleted -= OnSquareCompleted;
            bingoSystem.OnPatternDetected -= OnPatternDetected;
            bingoSystem.OnCardReset -= Refresh;
        }

        private void OnSquareCompleted(int row, int col)
        {
            Refresh();
        }

        private void OnPatternDetected(BingoPattern pattern)
        {
            if (patternText != null)
                patternText.text = $"Pattern: {pattern}";
        }

        public void Refresh()
        {
            if (bingoSystem == null)
                return;

            var state = BingoHudPresenter.BuildState(bingoSystem.GetCard());
            if (completedText != null)
                completedText.text = $"Card: {state.CompletedSquares}/25";

            if (squareImages == null)
                return;

            int len = Mathf.Min(squareImages.Length, state.SquareCompleted.Length);
            for (int i = 0; i < len; i++)
            {
                if (squareImages[i] != null)
                    squareImages[i].color = state.SquareCompleted[i] ? completedColor : incompleteColor;
            }
        }
    }

    public class ProgressionHudController : MonoBehaviour
    {
        [SerializeField] private Text classText;
        [SerializeField] private Text levelText;
        [SerializeField] private Text experienceText;
        [SerializeField] private Text skillPointsText;

        private CharacterProgression progression;

        public void BindProgression(CharacterProgression target)
        {
            Unsubscribe();
            progression = target;
            Subscribe();
            Refresh();
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        private void Subscribe()
        {
            if (progression == null)
                return;

            progression.OnLevelUp += OnProgressionChanged;
            progression.OnSkillPointsChanged += OnSkillPointsChanged;
        }

        private void Unsubscribe()
        {
            if (progression == null)
                return;

            progression.OnLevelUp -= OnProgressionChanged;
            progression.OnSkillPointsChanged -= OnSkillPointsChanged;
        }

        private void OnProgressionChanged(int _)
        {
            Refresh();
        }

        private void OnSkillPointsChanged(int _)
        {
            Refresh();
        }

        public void Refresh()
        {
            var state = ProgressionHudPresenter.BuildState(progression);

            if (classText != null)
                classText.text = state.ClassText;
            if (levelText != null)
                levelText.text = state.LevelText;
            if (experienceText != null)
                experienceText.text = state.ExperienceText;
            if (skillPointsText != null)
                skillPointsText.text = state.SkillPointText;
        }
    }

    public class InventoryHudController : MonoBehaviour
    {
        [SerializeField] private Text itemCountText;
        [SerializeField] private Text rarityBreakdownText;
        [SerializeField] private Text currenciesText;

        private Inventory inventory;

        public void BindInventory(Inventory target)
        {
            inventory = target;
            Refresh();
        }

        public void Refresh()
        {
            var state = InventoryHudPresenter.BuildState(inventory);

            if (itemCountText != null)
                itemCountText.text = state.ItemCountText;
            if (rarityBreakdownText != null)
                rarityBreakdownText.text = state.RarityBreakdownText;
            if (currenciesText != null)
                currenciesText.text = state.CurrenciesText;
        }
    }
}

