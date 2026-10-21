public class MoveCommand : Command
{
    readonly PlayerMovement playerMovement;
    readonly float h, v;

    public MoveCommand(PlayerMovement _playerMovement, float _h, float _v)
    {
        playerMovement = _playerMovement;
        h = _h;
        v = _v;
    }

    //Trigger perintah movement
    public override void Execute()
    {
        playerMovement.Move(h, v);
        //Menganimasikan player
        playerMovement.Animating(h, v);
    }

    public override void UnExecute()
    {
        //Invers arah dari movement player
        playerMovement.Move(-h, -v);
        //Menganimasikan player
        playerMovement.Animating(h, v);
    }
}
