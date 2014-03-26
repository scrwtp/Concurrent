namespace scrwtp.Concurrent

open System
open System.Collections.Generic

type Vow<'a> () = 
    let mailbox = MailboxProcessor.Start(fun inbox ->
        let rec loop (status, keepers, breakers, channels) =
            async {
                let! msg = inbox.Receive()
                // match instead of if here - compiler tries to restrict 'a with equality
                match status with
                | Pending ->
                    // if the promise is pending, either resolve it or enqueue a handler
                    match msg with
                    | AddKeeper f -> 
                        return! loop (status, f :: keepers, breakers, channels)
                    | AddBreaker f -> 
                        return! loop (status, keepers, f :: breakers, channels)
                    | Value channel -> 
                        return! loop (status, keepers, breakers, channel :: channels)
                    | Resolve result -> 
                        match result with
                        | Kept value -> 
                            keepers  |> List.iter (fun f -> f value)
                            channels |> List.iter (fun c -> c.Reply(Right value))
                        | Broken msg -> 
                            breakers |> List.iter (fun f -> f msg)
                            channels |> List.iter (fun c -> c.Reply(Left msg))
                        return! loop (Resolved result, [], [], [])
                | _ ->
                    // ...otherwise the promise is resolved - we only handle AddKeeper if it's kept and AddBreaker if it's broken. 
                    match status, msg with
                    | Resolved (Kept value), AddKeeper f    -> f value
                    | Resolved (Kept value), Value channel  -> channel.Reply(Right value)
                    | Resolved (Broken msg), AddBreaker f   -> f msg
                    | Resolved (Broken msg), Value channel  -> channel.Reply(Left msg)
                    | _, _ -> ()
                    return! loop (status, [], [], [])
            }
        loop (Pending, [], [], []))

    let keepVow result  = mailbox.Post(Resolve (Kept result))
    let breakVow msg    = mailbox.Post(Resolve (Broken msg))
    let whenKept f      = mailbox.Post(AddKeeper f)
    let whenBroken f    = mailbox.Post(AddBreaker f)

    let value ()        = mailbox.PostAndReply(fun channel -> Value channel)
    let valueAsync ()   = mailbox.PostAndAsyncReply(fun channel -> Value channel)

    member this.Promise = this :> IPromise<'a>
    member this.Future  = this :> IFuture<'a>

    interface IPromise<'a> with
        member this.Keep(result) = keepVow(result)
        member this.Break(msg) = breakVow(msg)

    interface IFuture<'a> with
        member this.WhenKept(f) = whenKept(f)
        member this.WhenBroken(f) = whenBroken(f)
        member this.Value = value()
        member this.ValueAsync = valueAsync()

module Vow = 
    let make<'a> () =
        let vow = Vow<'a>()
        vow.Promise, vow.Future
    