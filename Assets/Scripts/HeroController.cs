using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(HeroStats))]
[RequireComponent(typeof(HeroCombat))]
public class HeroController : MonoBehaviour
{
    private NavMeshAgent agent;
    private HeroStats stats;
    private HeroCombat combat;
    private Vector3 moveDir;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        stats = GetComponent<HeroStats>();
        combat = GetComponent<HeroCombat>();
    }

    private void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        Vector3 input = new Vector3(
            Keyboard.current.aKey.isPressed ? -1 : Keyboard.current.dKey.isPressed ? 1 : 0,
            0,
            Keyboard.current.sKey.isPressed ? -1 : Keyboard.current.wKey.isPressed ? 1 : 0
        );

        if (input.sqrMagnitude > 0)
        {
            var moveCmd = new MoveCommand(input.normalized);
            moveCmd.Execute(this);
        }

        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                var enemy = hit.collider.GetComponent<Enemy>();
                if (enemy != null)
                {
                    new AttackCommand(enemy.transform).Execute(this);
                }
            }
        }

        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            new AbilityCommand(0).Execute(this);
        }
        if (Keyboard.current.digit2Key.wasPressedThisFrame) new AbilityCommand(1).Execute(this);
        if (Keyboard.current.digit3Key.wasPressedThisFrame) new AbilityCommand(2).Execute(this);
        if (Keyboard.current.digit4Key.wasPressedThisFrame) new AbilityCommand(3).Execute(this);
    }

    public void Move(Vector3 direction)
    {
        Vector3 dest = transform.position + direction * stats.moveSpeed * Time.deltaTime;
        agent.Move(direction * stats.moveSpeed * Time.deltaTime);
        transform.forward = direction;
    }

    public void AttackTarget(Transform target)
    {
        combat.Attack(target);
    }

    public void UseAbility(int index)
    {
        combat.UseAbility(index);
    }
}
