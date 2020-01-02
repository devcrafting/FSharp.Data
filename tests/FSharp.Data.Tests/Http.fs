#if INTERACTIVE
#r "../../bin/lib/net45/FSharp.Data.dll"
#r "../../packages/test/NUnit/lib/net45/nunit.framework.dll"
#r "../../packages/test/FsUnit/lib/net46/FsUnit.NUnit.dll"
#else
module FSharp.Data.Tests.Http
#endif

open FsUnit
open NUnit.Framework
open System
open System.Net
open FSharp.Data
open FSharp.Data.HttpRequestHeaders
open System.Text

[<Test>]
let ``Don't throw exceptions on http error`` () =
    let response = Http.Request("http://httpstat.us/401", silentHttpErrors = true)
    response.StatusCode |> should equal 401

[<Test>]
let ``Throw exceptions on http error`` () =
    let exceptionThrown =
        try
            Http.RequestString("http://api.themoviedb.org/3/search/movie") |> ignore
            false
        with e ->
            true

    exceptionThrown |> should equal true

[<Test>]
let ``If the same header is added multiple times, throws an exception`` () =
    (fun () -> Http.RequestString("http://www.google.com", headers = [ UserAgent "ua1"; UserAgent "ua2" ]) |> ignore)
    |> should throw typeof<Exception>

[<Test>]
let ``If a custom header with the same name is added multiple times, an exception is thrown`` () =
    (fun () -> Http.RequestString("http://www.google.com", headers = [ "c1", "v1"; "c1", "v2" ]) |> ignore)
    |> should throw typeof<Exception>

[<Test>]
let ``Two custom header with different names don't throw an exception`` () =
    Http.RequestString("http://www.google.com", headers = [ "c1", "v1"; "c2", "v2" ]) |> ignore

[<Test>]
let ``A request with an invalid url throws an exception`` () =
    (fun() -> Http.Request "www.google.com" |> ignore) |> should throw typeof<UriFormatException>

[<Test>]
let ``Cookies with commas are parsed correctly`` () =
    let uri = Uri "http://www.nasdaq.com/symbol/ibm/dividend-history"
    let cookieHeader = "selectedsymboltype=IBM,COMMON STOCK,NYSE; domain=.nasdaq.com; expires=Sun, 21-May-2017 15:29:03 GMT; path=/,selectedsymbolindustry=IBM,technology; domain=.nasdaq.com; expires=Sun, 21-May-2017 15:29:03 GMT; path=/,NSC_W.TJUFEFGFOEFS.OBTEBR.80=ffffffffc3a08e3045525d5f4f58455e445a4a423660;expires=Sat, 21-May-2016 15:39:03 GMT;path=/;httponly"
    let cookies =
        CookieHandling.getAllCookiesFromHeader cookieHeader uri
        |> Array.map (snd >> (fun c -> c.Name, c.Value))
    cookies |> should equal
        [| "selectedsymboltype", "IBM,COMMON STOCK,NYSE"
           "selectedsymbolindustry", "IBM,technology"
           "NSC_W.TJUFEFGFOEFS.OBTEBR.80", "ffffffffc3a08e3045525d5f4f58455e445a4a423660" |]

[<Test>]
let ``Cookies with '=' are parsed correctly`` () =
    let uri = Uri "http://nevermind.com"
    let cookieHeader = "IdSession=Qze9H7HpvcVoh/ANulOl6Z1P8Omd1cLb9FOLL5o2aOlDMn/dFJi+RWDAPHZ4nDmWeno2puyOTFM/P4yzZMQsPoJ1gvEaJqG54kerM6WW4bv6ql72/Tn3NnCZTaokm8uaboLICgckUM2J7KOx5iL8uGyTN/g04/jZKlP1HgyatQL6kCG4qCQUMrdqZjqkbgW3eCpeyeI9rXF1bNC8hsKaqJ37Du/oBvbIMUbgenogfjzmlCgtAzv4la2Eo8+3cvDHkKPnksCP8kt8JbyXECyXBOjPrpFjtYv9UUGfyhwqRTRNTmH5+5UAsDDFrYe+vonYiDwXel8TfK3AZhQGXcF598AVPVfB1RO5S/mt7faDS7cEfz14nUsYtaNAZcwH7gwm06VJUX5eWiZzlBGx4SVBkNzP0QhLM9AqNP889y9BmZ2JaGb3fJtCWL3MfzM23mbwSemcERkoV3v1rIH8mb6ZgGm0hyEbbtu/RegkLAgNO+YB6c0Os6Pv6OnK0So4xlNakchaWhl1eMfOf4Gx0miJv4o2XbmAmbSNYkybi3n8vz4="
    let cookies =
        CookieHandling.getAllCookiesFromHeader cookieHeader uri
        |> Array.map (snd >> (fun c -> c.Name, c.Value))
    cookies |> should equal
        [| "IdSession", "Qze9H7HpvcVoh/ANulOl6Z1P8Omd1cLb9FOLL5o2aOlDMn/dFJi+RWDAPHZ4nDmWeno2puyOTFM/P4yzZMQsPoJ1gvEaJqG54kerM6WW4bv6ql72/Tn3NnCZTaokm8uaboLICgckUM2J7KOx5iL8uGyTN/g04/jZKlP1HgyatQL6kCG4qCQUMrdqZjqkbgW3eCpeyeI9rXF1bNC8hsKaqJ37Du/oBvbIMUbgenogfjzmlCgtAzv4la2Eo8+3cvDHkKPnksCP8kt8JbyXECyXBOjPrpFjtYv9UUGfyhwqRTRNTmH5+5UAsDDFrYe+vonYiDwXel8TfK3AZhQGXcF598AVPVfB1RO5S/mt7faDS7cEfz14nUsYtaNAZcwH7gwm06VJUX5eWiZzlBGx4SVBkNzP0QhLM9AqNP889y9BmZ2JaGb3fJtCWL3MfzM23mbwSemcERkoV3v1rIH8mb6ZgGm0hyEbbtu/RegkLAgNO+YB6c0Os6Pv6OnK0So4xlNakchaWhl1eMfOf4Gx0miJv4o2XbmAmbSNYkybi3n8vz4="|]

[<Test>]
let ``An empty cookie header is parsed correctly`` () =
    let uri = Uri "https://news.google.com/news?hl=en&ned=us&ie=UTF8&nolr=1&output=rss&q=FSharp&num=1000"
    let cookieHeader = ""
    let cookies = CookieHandling.getAllCookiesFromHeader cookieHeader uri
    cookies |> should equal [| |]

[<Test>]
let ``Cookies in CookieContainer are returned`` () =
    let cookieContainer = System.Net.CookieContainer()
    let someUri = Uri "http://nevermind.com"
    cookieContainer.Add(someUri, System.Net.Cookie("key", "value"))
    let header = Map.empty
    let cookies = CookieHandling.getCookiesAndManageCookieContainer someUri someUri header cookieContainer true false
    cookies |> should haveCount cookieContainer.Count

[<Test>]
let ``Cookies in header are added to CookieContainer and returned`` () =
    let cookieContainer = System.Net.CookieContainer()
    let someUri = Uri "http://nevermind.com"
    cookieContainer.Add(someUri, System.Net.Cookie("key1", "value1"))
    let header = Map.ofList [(HttpResponseHeaders.SetCookie, ("key2=value2"))]
    let cookies = CookieHandling.getCookiesAndManageCookieContainer someUri someUri header cookieContainer true false
    cookieContainer.Count |> should equal 2
    cookies |> should haveCount cookieContainer.Count

[<Test>]
let ``Cookies in header already existing in CookieContainer are added twice, and only new value is returned`` () =
    let cookieContainer = System.Net.CookieContainer()
    let someUri = Uri "http://nevermind.com"
    cookieContainer.Add(someUri, System.Net.Cookie("key", "value1"))
    let header = Map.ofList [(HttpResponseHeaders.SetCookie, ("key=value2"))]
    let cookies = CookieHandling.getCookiesAndManageCookieContainer someUri someUri header cookieContainer true false
    cookieContainer.Count |> should equal 2
    cookies |> should haveCount 1
    cookies.["key"] |> should equal "value2"

[<Test>]
let ``Cookies with unescaped JSON raise a CookieException (need to avoid cookieContainer parameter or ignoreCookieErrors`` () =
    let uri = Uri "http://nevermind.com"
    let header = Map.ofList [(HttpResponseHeaders.SetCookie, "hab={\"echanges\":1,\"notifications\":1,\"messages\":1}")]
    let cookieContainer = System.Net.CookieContainer()
    (fun () -> CookieHandling.getCookiesAndManageCookieContainer uri uri header cookieContainer true false |> ignore) |> should throw typeof<System.Net.CookieException>

[<Test>]
let ``Cookies with unescaped JSON is not added in cookieContainer but is still returned when ignoreCookieErrors parameter is true`` () =
    let uri = Uri "http://nevermind.com"
    let header = Map.ofList [(HttpResponseHeaders.SetCookie, "hab={\"echanges\":1,\"notifications\":1,\"messages\":1}")]
    let cookieContainer = System.Net.CookieContainer()
    let cookies = CookieHandling.getCookiesAndManageCookieContainer uri uri header cookieContainer true true
    cookieContainer.Count |> should equal 0
    cookies |> should haveCount 1

[<Test>]
let ``Cookies is not added in cookieContainer but is still returned when addCookieInCookieContainer parameter is false (deducted from option cookieContainer passed in InnerRequest method)`` () =
    let uri = Uri "http://nevermind.com"
    let header = Map.ofList [(HttpResponseHeaders.SetCookie, "hab={\"echanges\":1,\"notifications\":1,\"messages\":1}")]
    let cookieContainer = System.Net.CookieContainer()
    let cookies = CookieHandling.getCookiesAndManageCookieContainer uri uri header cookieContainer false false
    cookieContainer.Count |> should equal 0
    cookies |> should haveCount 1

let host = "http://localhost:8080/"

let fakeLocalOneRequestListener (handler:(HttpListenerRequest->HttpListenerResponse->Async<unit>)) (cancellationToken: System.Threading.CancellationTokenSource) =
    let hl = new HttpListener()
    hl.Prefixes.Add host
    hl.Start()
    let cancelAction = fun () -> hl.Close()
    let task = Async.FromBeginEnd(hl.BeginGetContext, hl.EndGetContext, cancelAction)
    let handler req resp =
        async {
            do! handler req resp
            hl.Close()
        }
    let asyncTask = async {
        let! context = task
        Async.Start(handler context.Request context.Response, cancellationToken.Token)
    }
    Async.Start(asyncTask, cancellationToken.Token)

[<Test>]
let ``Cookies are added to cookieContainer passed in parameters (if not already done by dotnet framework - for example because it still implements Path check against URI)`` () =
    use cancellationToken = new System.Threading.CancellationTokenSource()
    fakeLocalOneRequestListener (fun req resp ->
        async {
            let txt = Encoding.ASCII.GetBytes("Cookie passed")
            resp.ContentType <- "text/html"
            resp.Headers.Add("Set-Cookie", "TEST=1234; Path=/cookie-path/")
            resp.OutputStream.Write(txt, 0, txt.Length)
            resp.OutputStream.Close()
        }) cancellationToken

    try
        let cookieContainer = System.Net.CookieContainer()
        Http.Request("http://localhost:8080/another-path-than-cookie-path-rejected-by-dotnet-framework/", cookieContainer = cookieContainer) |> ignore
        Uri("http://localhost:8080/cookie-path/") |> cookieContainer.GetCookies
        |> should haveCount 1
    with
    | _ -> cancellationToken.Cancel(); reraise()

[<Test>]
let ``Web request's timeout is used`` () =
    let exc = Assert.Throws<System.Net.WebException> (fun () ->
        Http.Request("http://httpstat.us/200?sleep=1000", customizeHttpRequest = (fun req -> req.Timeout <- 1; req)) |> ignore)
    Assert.AreEqual(typeof<TimeoutException>, exc.InnerException.GetType())

[<Test>]
let ``Timeout argument is used`` () =
    let exc = Assert.Throws<System.Net.WebException> (fun () ->
        Http.Request("http://httpstat.us/200?sleep=1000", timeout = 1) |> ignore)
    Assert.AreEqual(typeof<TimeoutException>, exc.InnerException.GetType())

[<Test>]
let ``Setting timeout in customizeHttpRequest overrides timeout argument`` () =
    let response =
        Http.Request("http://httpstat.us/401?sleep=1000", silentHttpErrors = true,
            customizeHttpRequest = (fun req -> req.Timeout <- Threading.Timeout.Infinite; req), timeout = 1)

    response.StatusCode |> should equal 401

let testFormDataSizesInBytes = [
    4000    // previous test size
    20000   // previous test size
    40000   // > 80k, reported by user @danyx23 on full-framework
    100000  // > 200k, reported by user danyx23 on .net core
    200000  // > 400k, just future-proofing
]

[<Test; TestCaseSource("testFormDataSizesInBytes")>]
let testFormDataBodySize (size: int) = 
    let bodyString = seq {for i in 0..size -> "x\n"} |> String.concat ""
    let body = FormValues([("input", bodyString)])
    Assert.DoesNotThrowAsync(fun () -> Http.AsyncRequest (url="http://httpstat.us/200", httpMethod="POST", body=body, timeout = 10000) |> Async.Ignore |> Async.StartAsTask :> _)

[<Test; TestCaseSource("testFormDataSizesInBytes")>]
let testMultipartFormDataBodySize (size: int) = 
    let bodyString = seq {for i in 0..size -> "x\n"} |> String.concat ""
    let multipartItem = [ MultipartItem("input", "input.txt", new IO.MemoryStream(Encoding.UTF8.GetBytes(bodyString)) :> IO.Stream) ]
    let body = Multipart(Guid.NewGuid().ToString(), multipartItem)

    Assert.DoesNotThrowAsync(fun () -> Http.AsyncRequest (url="http://httpstat.us/200", httpMethod="POST", body=body, timeout = 10000) |> Async.Ignore |> Async.StartAsTask :> _)

[<Test>]
let ``escaping of url parameters`` () =
    let url = "https://graph.microsoft.com/beta/me/insights/shared"
    let queryParams = [
        "$select", "Property1,Property2/SubProperty,*"
        "$filter", "Subject eq 'A? & #B = C/D'"
        "$top", "10"
        "$expand", "ExtendedProperties($filter=PropertyId eq 'String {36FF76DC-215F-4246-9544-DAB709259CE8} Name Some/Property2.0')"
        "$orderby", "Property2 desc"
    ]

    Http.AppendQueryToUrl(url, queryParams)
    |> should equal "https://graph.microsoft.com/beta/me/insights/shared?$select=Property1,Property2/SubProperty,*&$filter=Subject%20eq%20'A?%20%26%20%23B%20=%20C/D'&$top=10&$expand=ExtendedProperties($filter=PropertyId%20eq%20'String%20%7B36FF76DC-215F-4246-9544-DAB709259CE8%7D%20Name%20Some/Property2.0')&$orderby=Property2%20desc"
