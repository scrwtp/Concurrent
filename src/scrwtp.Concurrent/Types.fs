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

type IVow<'a> =
    abstract Keep: 'a -> unit
    abstract Break: string -> unit   

type Promise<'a> () = 
    let mailbox = MailboxProcessor.Start(fun inbox ->
        let rec loop (status, keepers, breakers) =
            async {
                let! msg = inbox.Receive()
                // match instead of if here - compiler tries to restrict 'a with equality
                match status with
                | Pending ->
                    // if the promise is pending, either resolve it or enqueue a handler
                    match msg with
                    | AddKeeper f -> 
                        return! loop (status, f :: keepers, breakers)
                    | AddBreaker f -> 
                        return! loop (status, keepers, f :: breakers)
                    | Resolve result -> 
                        match result with
                        | Kept value -> List.iter (fun f -> f value) keepers
                        | Broken msg -> List.iter (fun f -> f msg) breakers
                        return! loop (Resolved result, [], [])
                | _ ->
                    // ...otherwise the promise is resolved - we only handle AddKeeper if it's kept and AddBreaker if it's broken. 
                    match status, msg with
                    | Resolved (Kept value), AddKeeper f -> f value
                    | Resolved (Broken msg), AddBreaker f -> f msg
                    | _, _ -> ()
                    return! loop (status, [], [])
            }
        loop (Pending, [], []))

    member this.Keep(result) = 
        mailbox.Post(Resolve (Kept result))

    member this.Break(msg) = 
        mailbox.Post(Resolve (Broken msg))

    member this.WhenKept(f) = 
        mailbox.Post(AddKeeper f)

    member this.WhenBroken(f) = 
        mailbox.Post(AddBreaker f)

    interface IVow<'a> with
        member this.Keep(result) = this.Keep(result)
        member this.Break(msg) = this.Break(msg)