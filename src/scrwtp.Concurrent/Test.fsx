#I "./bin/debug/"
#r "scrwtp.Concurrent.dll"

open scrwtp.Concurrent

let promise, future = Vow.make<string>()

future.WhenKept(fun name -> printfn "Hello, %s" name)
future.WhenBroken(fun msg -> printfn "How rude, %s" msg)

promise.Keep("Tomasz")
promise.Break("staying silent like this!")

let promise', future' = Vow.make<string>()

let background = 
    async {
        printfn "Running in the background..."
        let! result = future'.ValueAsync
        printfn "Just got the value!"
        match result with 
        | Left msg -> printfn "How rude, %s" msg
        | Right name -> printfn "Hello, %s" name
    }

background |> Async.Start

promise'.Keep("Tomasz")
promise'.Break("staying silent like this!")

let promise'', future'' = Vow.make<string>()

let background' = 
    async {
        printfn "Running in the background..."
        let result = future''.Value
        printfn "Just got the value!"
        match result with 
        | Left msg -> printfn "How rude, %s" msg
        | Right name -> printfn "Hello, %s" name
    }

background' |> Async.Start

promise''.Keep("Tomasz")
promise''.Break("say something!")    
    