﻿Modules
=======

The Module is the final piece, and is what is needed to move a game forward.

The `MpGameModule` class looks like this:
```cs
public abstract class MpGameModuleBase<TService, TGame, TPlayer> : ModuleBase<ICommandContext>
    where TService : MpGameService<TGame, TPlayer>
    where TGame    : GameBase<TPlayer>
    where TPlayer  : Player
{
    protected MpGameModuleBase(TService gameService);

    protected TGame Game { get; }

    protected bool GameInProgress { get; }

    protected TService GameService { get; }

    protected bool OpenToJoin { get; }

    protected ImmutableHashSet<IUser> PlayerList { get; }

    public abstract Task OpenGameCmd();

    public abstract Task CancelGameCmd();

    public abstract Task JoinGameCmd();

    public abstract Task LeaveGameCmd();

    public abstract Task StartGameCmd();

    public abstract Task NextTurnCmd();

    public abstract Task GameStateCmd();

    public abstract Task EndGameCmd();

    [Command("resend")]
    public async Task ResendCmd();
}
```

Other versions of this class with less generic parameters also exist,
so you don't *need* to supply all the type parameters. Consult
IntelliSense for the details.

There are 8 methods you can implement, corresponding to the
actions needed in most games. When you implement these, you decorate them with `[Command]`
so that the command system recognizes them. There may be methods you don't want or need to
implement, in which case you can omit the `[Command]` attribute so it can't be called.
Likewise, you'll most likely be adding *more* commands in order to control your game.

One command is predefined which will retry sending a DM
to a user after they have been notified to enable DMs.

With your own service class for persistent data, you should derive
from this class as follows:
```cs
public class CardGameModule : MpGameModuleBase<CardGameService, CardGame, CardPlayer>
{
    public CardGameModule(CardGameService service)
        : base(service)
    {
    }

    protected override void BeforeExecute()
    {
        // If you choose to override this method, you *must* call the base version first
        base.BeforeExecute();
        // If you have any persistent data of your own, load
        // the relevant instance from the dictionary
        // in your service class here and store
        // the result in a private field
        GameService.SomeDataDictionary.TryGetValue(Context.Channel.Id, out var _data);
    }
    private DataType _data;
}
```

While having an explicit service class will make it easier to expand in the future,
you *can* omit the type parameter to use the default if you have no other persistent
data to store for your game:
```cs
public class CardGameModule : MpGameModuleBase<CardGame, CardPlayer>
{
    public CardGameModule(MpGameService<CardGame, CardPlayer> service)
        : base(service)
    {
    }
}
```

Example implementations for the abstract methods can be found
[in the Examples project](../../../Examples/MpGame/ExampleModule.cs)

[<- Part 4 - Services](4-Services.md) - Modules - [Part 6 - Final step ->](6-FinalStep.md)