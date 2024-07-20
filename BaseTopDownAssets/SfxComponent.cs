using Godot;
using System;

[GlobalClass]//, Tool] //TODO: ADD WARNING IN PARENT
public partial class SfxComponent : AudioStreamPlayer2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        var phonicStream = new AudioStreamPolyphonic();
		phonicStream.Polyphony = 64;
		Stream = phonicStream;
		Play();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void PlayStream(AudioStream stream)
	{

	}
}
