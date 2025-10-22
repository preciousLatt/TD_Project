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
        if (GameManager.Instance != null && GameManager.Instance.IsPaused)
            return;

        HandleInput();
    }

    private void HandleInput()
    {
        var inputHandler = InputHandler.Instance; 
        Vector3 input = new Vector3(
            (Input.GetKey(inputHandler.GetKey("MoveLeft")) ? -1 : 0) +
            (Input.GetKey(inputHandler.GetKey("MoveRight")) ? 1 : 0),
            0,
            (Input.GetKey(inputHandler.GetKey("MoveDown")) ? -1 : 0) +
            (Input.GetKey(inputHandler.GetKey("MoveUp")) ? 1 : 0)
        );

        if (input.sqrMagnitude > 0)
        {
            var moveCmd = new MoveCommand(input.normalized);
            moveCmd.Execute(this);
        }

        if (Input.GetKeyDown(inputHandler.GetKey("Attack")))
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

        if (Input.GetKeyDown(inputHandler.GetKey("Ability1"))) new AbilityCommand(0).Execute(this);
        if (Input.GetKeyDown(inputHandler.GetKey("Ability2"))) new AbilityCommand(1).Execute(this);
        if (Input.GetKeyDown(inputHandler.GetKey("Ability3"))) new AbilityCommand(2).Execute(this);
        if (Input.GetKeyDown(inputHandler.GetKey("Ability4"))) new AbilityCommand(3).Execute(this);
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
