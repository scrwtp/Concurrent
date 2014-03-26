namespace scrwtp.Concurrent

open System
open System.Collections.Generic

type KeeperType<'a> = 'a -> unit
type BreakerType = string -> unit

type Resolved<'a> = 
    | Kept of 'a
    | Broken of string

type PromiseState<'a> = 
    | Pending
    | Resolved of Resolved<'a>

type PromiseChange<'a> = 
    | Resolve of Resolved<'a>
    | AddKeeper of KeeperType<'a>
    | AddBreaker of BreakerType
    | Value of AsyncReplyChannel<Either<string,'a>>

type MailboxState<'a> = 
    {
        Status:     PromiseState<'a>
        Keepers:    KeeperType<'a> list
        Breakers:   BreakerType list
        Channels:   AsyncReplyChannel<Either<string, 'a>> list
    }
    static member Empty : MailboxState<'a> = 
        {
            Status   = Pending
            Keepers  = List.empty
            Breakers = List.empty
            Channels = List.empty
        }
    static member EmptyWithStatus(status) : MailboxState<'a> = 
        {
            Status   = status
            Keepers  = List.empty
            Breakers = List.empty
            Channels = List.empty
        }

type IPromise<'a> =
    abstract Keep: 'a -> unit
    abstract Break: string -> unit

type IFuture<'a> = 
    abstract WhenKept: KeeperType<'a> -> unit
    abstract WhenBroken: BreakerType -> unit
    abstract ValueAsync : Async<Either<string, 'a>>
    abstract Value : Either<string, 'a>
