using System;
using UnityEngine;

public abstract class Command
{
    public abstract void Execute(HeroController hero);
}

public class MoveCommand : Command
{
    private Vector3 direction;
    public MoveCommand(Vector3 dir) => direction = dir;

    public override void Execute(HeroController hero)
    {
        hero.Move(direction);
    }
}

public class AttackCommand : Command
{
    private Transform target;
    public AttackCommand(Transform target) => this.target = target;

    public override void Execute(HeroController hero)
    {
        hero.AttackTarget(target);
    }
}

public class AbilityCommand : Command
{
    private int abilityIndex;
    public AbilityCommand(int index) => abilityIndex = index;

    public override void Execute(HeroController hero)
    {
        hero.UseAbility(abilityIndex);
    }
}
