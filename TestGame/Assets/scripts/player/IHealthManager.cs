
// Interface for health management
public interface IHealthManager
{
    float CurrentHealth { get; }
    float MaxHealth { get; }
    bool IsAlive { get; }
    void TakeDamage(float damage);
    void Heal(float amount);
    void SetMaxHealth(float maxHealth);
}
