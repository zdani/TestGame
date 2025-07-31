using UnityEngine;

// Interface for abilities that can be cast
public interface IAbility
{
    string AbilityName { get; }
    void Cast(Player caster);
}
