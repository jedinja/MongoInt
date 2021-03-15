module ImproveApi.LoginController

open BCrypt.Net
open Domain
open Models
open Combinators
open MongoInt.MongoEntityFactory

open Suave

let private verifyPass (model: Entity<User>) (user: UserForLogin) =
    let dbUserOpt = model.all { Name = user.Name } |> List.tryHead
    match dbUserOpt with
    | None -> Mid.BAD
    | Some dbUser ->
        match BCrypt.EnhancedVerify(user.Password, dbUser.Record.Password) with
        | true -> { Token = Token.Build dbUser.Id } |> Mid.JSON
        | false -> Mid.BAD

let private loginUser (model: Entity<User>) =
    Mid.requestBody >>
    Mid.FromJSON<UserForLogin> >>
    verifyPass model

let login :Factory->WebPart = getUserModel >> loginUser >> request

let SESSION_USER = "userId"

let LOGGED_IN =
    requestHeader
        "Authorization"
        (fun token -> match Token.ValidateAndGetUserId token with
                        | Some userId -> Mid.setSessionValue SESSION_USER userId
                        | _ -> never)

let withUser = withSession SESSION_USER


