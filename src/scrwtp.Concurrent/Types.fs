namespace scrwtp.Concurrent

open System
open System.Collections.Generic

type Resolved<'a> = 
    | Kept of 'a
    | Broken of string

type PromiseState<'a> = 
    | Pending
    | Resolved of Resolved<'a>

type PromiseChange<'a> = 
    | Resolve of Resolved<'a>
    | AddKeeper of ('a -> unit)
    | AddBreaker of (string -> unit)
    | Value of AsyncReplyChannel<Either<string,'a>>

type IPromise<'a> =
    abstract Keep: 'a -> unit
    abstract Break: string -> unit

type IFuture<'a> = 
    abstract WhenKept: ('a -> unit) -> unit
    abstract WhenBroken: (string -> unit) -> unit
    abstract ValueAsync : Async<Either<string, 'a>>
    abstract Value : Either<string, 'a>
