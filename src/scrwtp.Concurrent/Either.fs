namespace scrwtp.Concurrent

open System
open System.Collections.Generic

type Either<'a, 'b> =
    | Left of 'a
    | Right of 'b

module Either =
    let isLeft  = function 
        | Left _ -> true  
        | Right _ -> false
    
    let isRight = function 
        | Left _ -> false 
        | Right _ -> true
    
    let map lf rf = function 
        | Left value -> Left (lf value) 
        | Right value -> Right (rf value)

    let partition lst = 
        let lefts, rights = 
            List.fold (fun (lefts, rights) either -> 
                match either with
                | Left value -> (value :: lefts, rights)
                | Right value -> (lefts, value :: rights)) ([],[]) lst
        List.rev lefts, List.rev rights

