using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AbilityUI : MonoBehaviour
{

    private Dictionary<AbilityType, Image> iconMap;

    private void Awake()
    {
        var iconContainer = this.gameObject;
        iconMap = new Dictionary<AbilityType, Image>();
        
        
        foreach (var iconImage in iconContainer.GetComponentsInChildren<Image>(true))
        {
            
            if (iconImage.gameObject == iconContainer) continue;

            Debug.Log($"Parsing icon name: {iconImage.gameObject.name}");
            if (Enum.TryParse(iconImage.gameObject.name, out AbilityType abilityType))
            {
                iconMap[abilityType] = iconImage;
            }
        }

        GameEvents.Instance.OnAbilityLearned += OnAbilityLearned;
    }

    private void OnDestroy()
    {
        if (GameEvents.Instance != null)
        {
            GameEvents.Instance.OnAbilityLearned -= OnAbilityLearned;
        }
    }

    private void Start()
    {
        Player player = FindFirstObjectByType<Player>();
        if (player != null)
        {
            foreach (var ability in iconMap.Keys)
            {
                bool isLearned = player.Abilities.Contains(ability);
                UpdateIconVisuals(ability, isLearned);
            }
        }
    }

    private void OnAbilityLearned(AbilityType abilityType)
    {
        UpdateIconVisuals(abilityType, true);
    }

    private void UpdateIconVisuals(AbilityType abilityType, bool isLearned)
    {
        if (iconMap.TryGetValue(abilityType, out Image iconImage))
        {
            string hexColor = isLearned ? "#FFFFFF" : "#272727";
            if (ColorUtility.TryParseHtmlString(hexColor, out Color newColor))
            {
                iconImage.color = newColor;
            }
        }
    }
} 