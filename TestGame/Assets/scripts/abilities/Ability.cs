using UnityEngine;

// Base ability class
public abstract class Ability : IAbility
{
    public string AbilityName { get; protected set; }

    protected Ability(string name)
    {
        AbilityName = name;
    }

    // check for requirements here (cooldown, mana) before casting ability
    public virtual void Cast(Player caster)
    {
        OnCast(caster);
    }

    protected abstract void OnCast(Player caster);
} 