using UnityEngine;

// Runtime canonical player stats. If a CharacterData SO is assigned, it overrides
// the values pulled from sibling components at Awake. Upgrades go through this
// component so changes propagate back into Entity / TopDownPlayerController.
public class PlayerStats : MonoBehaviour
{
    [Header("Optional Character Profile")]
    public CharacterData characterData;

    [Header("Runtime Values")]
    public float maxHp;
    public float walkSpeed;
    public float mightMultiplier;
    public float pickupRange;
    public float attackRange;

    private PlayerAttack playerAttack;
    private TopDownPlayerController controller;

    void Awake()
    {
        playerAttack = GetComponent<PlayerAttack>();
        controller = GetComponent<TopDownPlayerController>();
        InitializeBaseValues();
        PushToComponents(syncHp: true);
    }

    private void InitializeBaseValues()
    {
        if (characterData != null)
        {
            maxHp = characterData.baseMaxHp;
            walkSpeed = characterData.baseWalkSpeed;
            mightMultiplier = characterData.baseMightMultiplier;
            pickupRange = characterData.basePickupRange;
            attackRange = characterData.baseAttackRange;
            return;
        }

        // No SO assigned — seed from whatever the Inspector has on sibling components
        maxHp = playerAttack != null ? playerAttack.maxHp : 100f;
        mightMultiplier = playerAttack != null ? playerAttack.mightMultiplier : 1f;
        pickupRange = playerAttack != null ? playerAttack.pickupRange : 3f;
        attackRange = playerAttack != null ? playerAttack.attackRange : 10f;
        walkSpeed = controller != null ? controller.moveSpeed : 6f;
    }

    private void PushToComponents(bool syncHp)
    {
        if (playerAttack != null)
        {
            if (syncHp)
            {
                playerAttack.maxHp = maxHp;
                playerAttack.currentHp = maxHp;
            }
            playerAttack.mightMultiplier = mightMultiplier;
            playerAttack.pickupRange = pickupRange;
            playerAttack.attackRange = attackRange;
        }
        if (controller != null)
        {
            controller.moveSpeed = walkSpeed;
        }
    }

    public void SetMaxHp(float newMax, bool fillDelta)
    {
        float delta = newMax - maxHp;
        maxHp = newMax;
        if (playerAttack != null)
        {
            playerAttack.maxHp = newMax;
            if (fillDelta)
            {
                playerAttack.currentHp = Mathf.Min(newMax, playerAttack.currentHp + delta);
            }
            else
            {
                playerAttack.currentHp = Mathf.Min(newMax, playerAttack.currentHp);
            }
        }
    }

    public void SetWalkSpeed(float newSpeed)
    {
        walkSpeed = newSpeed;
        if (controller != null) controller.moveSpeed = newSpeed;
    }

    public void SetMightMultiplier(float newMult)
    {
        mightMultiplier = newMult;
        if (playerAttack != null) playerAttack.mightMultiplier = newMult;
    }

    public void SetPickupRange(float newRange)
    {
        pickupRange = newRange;
        if (playerAttack != null) playerAttack.pickupRange = newRange;
    }

    public void SetAttackRange(float newRange)
    {
        attackRange = newRange;
        if (playerAttack != null) playerAttack.attackRange = newRange;
    }
}
