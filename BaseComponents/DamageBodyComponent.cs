using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

[GlobalClass]//, Tool]
public partial class DamageBodyComponent : Node2D
{
    public const string DamageBodyComponentSceneName = "DamageBodyComponent";
    public const string DamageBodyComponentGroupName = "DamageBodies";

    [Export]
    public PhysicsBody2D PhysicsBody { get; private set; }
    [Export]
    public HitboxAttack CurrentAttack { get; private set; }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        AddToGroup(DamageBodyComponentGroupName);
        if (Engine.IsEditorHint())
        {
            Name = DamageBodyComponentSceneName;
        }
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Engine.IsEditorHint())
		{
			UpdateConfigurationWarnings();
			if (Name != DamageBodyComponentSceneName)
            {
                Name = DamageBodyComponentSceneName;
            }
        }
	}

    public override string[] _GetConfigurationWarnings()
    {
		var warnings = new List<string>() { };
        if (GetParent() is not PhysicsBody2D) // must be static, char, or rigid body
        {
			warnings.Add("Component will have no affect if it isn't a direct child of a PhysicsBody2D!");
        }
        return warnings.ToArray();
    }
}
