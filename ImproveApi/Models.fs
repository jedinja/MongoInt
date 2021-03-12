module ImproveApi.Models

open ImproveApi.Domain
open MongoInt.MongoEntityFactory

let mutable private _user : Entity<User> option = None

let internal getUserModel (factory: Factory) =
    if _user.IsNone then _user <- Some (factory.CreateEntity<User> "user")
    _user.Value