using System.Collections.Generic;
using UnityEngine;

public class InputHandler : MonoBehaviour
{
    public PlayerMovement playerMovement;
    public PlayerShooting playerShooting;

    //Jumlah maksimal command yang disimpan untuk undo
    public int maxUndoHistory = 300;

    //List command; undo mengambil command terakhir (LIFO)
    readonly LinkedList<Command> commands = new LinkedList<Command>();

    void OnEnable()
    {
        //InputHandler menjadi satu-satunya sumber gerak & tembak agar tidak tereksekusi dua kali
        if (playerMovement != null) playerMovement.externallyDriven = true;
        if (playerShooting != null) playerShooting.externallyDriven = true;
    }

    void OnDisable()
    {
        if (playerMovement != null) playerMovement.externallyDriven = false;
        if (playerShooting != null) playerShooting.externallyDriven = false;
    }

    void FixedUpdate()
    {
        if (!playerMovement.enabled) return;

        //Menghandle input movement
        Command moveCommand = InputMovementHandling();
        if (moveCommand != null)
        {
            commands.AddLast(moveCommand);
            if (commands.Count > maxUndoHistory)
            {
                commands.RemoveFirst();
            }

            moveCommand.Execute();
        }
    }

    void Update()
    {
        //Menghandle shoot
        Command shootCommand = InputShootHandling();
        if (shootCommand != null)
        {
            shootCommand.Execute();
        }
    }

    Command InputMovementHandling()
    {
        if (GameInput.UndoHeld)
        {
            //Undo movement
            return Undo();
        }

        //Input keyboard (WASD / panah) atau stick kiri gamepad, termasuk arah diagonal
        Vector2 move = GameInput.Move;
        return new MoveCommand(playerMovement, move.x, move.y);
    }

    Command Undo()
    {
        //Jika list command tidak kosong, lakukan perintah undo pada command terakhir
        if (commands.Count > 0)
        {
            Command undoCommand = commands.Last.Value;
            commands.RemoveLast();
            undoCommand.UnExecute();
        }
        return null;
    }

    Command InputShootHandling()
    {
        //Jika tombol tembak ditahan dan senjata siap, trigger shoot command
        if (GameInput.FireHeld && playerShooting.CanShoot)
        {
            return new ShootCommand(playerShooting);
        }
        return null;
    }
}
