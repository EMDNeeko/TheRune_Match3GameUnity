using System.Collections.Generic;
using Match3Game.Assets.Project.Scripts.Data;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Match3Game.Managers
{
    [System.Serializable]
    public class CharacterData
    {
        public string characterName;
        public Sprite avatarSprite;
    }
    public class LobbyManager : MonoBehaviour
    {
        [Header("Data List")]
        public List<CharacterData> availableHeroes = new List<CharacterData>();
        public List<CharacterData> availableEnemies = new List<CharacterData>();
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        [Header("Panels")]
        public GameObject heroSelectionPanel;
        public GameObject bossSelectionPanel;
        public GameObject priorityStatPanel;

        [Header("Scroll View Contents")]
        public Transform heroContentTransform;
        public Transform enemyContentTransform;
        public GameObject characterButtonPrefab;

        [Header("UI Text")]
        public Text selectedHeroText;
        public Text selectedBossText;

        [Header("Button")]
        public Button battleButton;
        public Button confirmButton;

        private string tempHero = "";
        private string tempBoss = "";

        void Start()
        {
            heroSelectionPanel.SetActive(true);
            bossSelectionPanel.SetActive(true);
            priorityStatPanel.SetActive(false);
            confirmButton.gameObject.SetActive(true);
            battleButton.gameObject.SetActive(false);

            PopulateList(availableHeroes, heroContentTransform, true);
            PopulateList(availableEnemies, enemyContentTransform, false);
        }

        private void PopulateList(List<CharacterData> characters, Transform contentParent, bool isHero)
        {
            foreach (CharacterData charName in characters)
            {
                GameObject btnObj = Instantiate(characterButtonPrefab, contentParent);

                Text btnText = btnObj.GetComponentInChildren<Text>();
                if (btnText != null)
                {
                    btnText.text = charName.characterName;
                }
                Transform avatarTransform = btnObj.transform.Find("Avatar");
                if (avatarTransform != null)
                {
                    Image avatarImage = avatarTransform.GetComponent<Image>();
                    if (avatarImage != null && charName.avatarSprite != null)
                    {
                        avatarImage.sprite = charName.avatarSprite;
                    }
                }
                Button btn = btnObj.GetComponent<Button>();
                btn.onClick.AddListener(() =>
                {
                    if (isHero) SelectHero(charName.characterName);
                    else SelectEnemy(charName.characterName);
                });
            }
        }

        public void SelectHero(string heroName)
        {
            tempHero = heroName;
            selectedHeroText.text = "Hero: " + heroName;
            Debug.Log("Choosing: " + heroName);
        }

        public void SelectEnemy(string bossName)
        {
            tempBoss = bossName;
            selectedBossText.text = "Boss: " + bossName;
            Debug.Log("Choosing: " + bossName);
        }

        public void ConfirmSelection()
        {
            if (string.IsNullOrEmpty(tempHero) || string.IsNullOrEmpty(tempBoss))
            {
                Debug.Log("Vui long chon hero va boss");
                return;
            }


            priorityStatPanel.SetActive(true);
            battleButton.gameObject.SetActive(true);
        }

        public void SelectPriorityStat(int statIndex)
        {
            //0. None, 1. Physical, 2. Magical, 3. HP, 4.Mana
            GameSession.selectedPriorityStat = (PriorityStat)statIndex;
            GameSession.selectedHero = tempHero;
            GameSession.selectedEnemy = tempBoss;

            Debug.Log("Priority Stat: " + GameSession.selectedPriorityStat);

        }

        public void ConfirmBattleClicked()
        {
            if (tempHero == null || tempBoss == null)
            {
                Debug.Log("Please choose hero and boss");
                return;
            }

            SceneManager.LoadScene("CombatScene");
        }

    }

}
