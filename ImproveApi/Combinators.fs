module ImproveApi.Combinators

open Suave
open Suave.Sscanf

let private combinator condition result =
    let F context =
        match condition context with
        | Some res ->
            result res context
        | None ->
            fail
    F

let pathScanWithBody format func =
    let scan url =
        try
            let scanRes = sscanf format url
            Some scanRes
        with _ -> None

    combinator
        (fun context -> scan context.request.path)
        (fun res context -> func (res, (Mid.contextBody context)) context)

let requestHeader header onValueFunc : WebPart =
    request (fun req -> match req.header(header) with
                          | Choice1Of2 token -> onValueFunc token
                          | _ -> never)

let withSession session onSessionFunc : WebPart =
    context (fun ctx ->
        let user = Mid.getStringSessionValue session ctx
        onSessionFunc user (Mid.contextBody ctx))