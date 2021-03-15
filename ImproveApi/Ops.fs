module ImproveApi.Ops
open Suave.Operators

let (<=<) a b = b >=> a