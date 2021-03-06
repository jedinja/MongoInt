module ImproveApi.Mid

open Suave
open Suave.State.CookieStateStore
open Newtonsoft.Json

type JsonWebPart = obj -> WebPart

let JSON:JsonWebPart = JsonConvert.SerializeObject >> Successful.OK
let FromJSON<'a> = JsonConvert.DeserializeObject<'a>

let requestBody (req : HttpRequest) =
    let getString (rawForm: byte []) = System.Text.Encoding.UTF8.GetString(rawForm)
    req.rawForm
    |> getString

let noFilter (a: HttpContext) = None

let contextBody (a: HttpContext) = a.request |> requestBody

let setSessionValue (key : string) (value : 'T) : WebPart =
    context (fun ctx ->
        match HttpContext.state ctx with
        | Some state ->
            state.set key value
        | _ ->
            never
        )

let getSessionValue (ctx : HttpContext) (key : string) : 'T option =
    match HttpContext.state ctx with
    | Some state ->
        state.get key
    | _ ->
        None

/// This a convenience function that turns a None string result into an empty string
let getStringSessionValue (key : string) (ctx : HttpContext)  : string =
    defaultArg (getSessionValue ctx key) ""

let tupleTrack (funcA, funcB) (a,b) = (funcA a, funcB b)

let tupleToParams func (a,b) = 
    func a b

let fromUnit a = (fun _ -> a)

let pass act a =
    act a
    a

let BAD = RequestErrors.BAD_REQUEST ""
let SUCCESS = Successful.OK "{}"
