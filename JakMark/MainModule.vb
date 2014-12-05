Imports System.IO

Module MainModule
    ' Features I'd like to support
    ' * bold, italic, monospaced                **bold** *italic* `monospaced`
    ' * bulletted, numbered lists with multiple levels  <list>* line1 \n * line 2</list> <list># line1 \n line 2</list>
    ' * tables                                  <table> head1 | head 2 \n cell1 | cell 2 </table>
    ' * fenced code                             ```
    ' * footnotes, links                        [^footnote], [link](lalala) or like footnote
    ' * headings                                # ## ###
    ' * continuing thingies                     two spaces at end
    ' * comments                                <!--you can't see this-->
    ' * emdash                                  --
    ' * escapes                                 \!escaped text\!

    ' Contexts
    ' * Heading
    ' * Table
    ' * List
    ' * Formated
    ' * Fenced
    ' * Comment
    ' * Normal
    ' * Escaped

    Enum OutputType
        Html
        GHMarkDown
        WikiPlex
    End Enum

    Sub Usage()
        Dim appName = System.Environment.GetCommandLineArgs()(0)
        Console.WriteLine("{0} v{1} Copyright (c) 2014 Vlad Meșco", appName, My.Application.Info.Version.ToString())
        Console.WriteLine("Usage:")
        Console.WriteLine("JakMark /?          print this message")
        Console.WriteLine("JakMark [options] [if] [of]   reads the JM file <if> and writes to <of>")
        Dim blanks = New String(" "c, appName.Length)
        Console.WriteLine("JakMark             infile or outfile can be ""-"" for stdin/stdout respectively")
        Console.WriteLine("JakMark             if <of> is a file name, the result is a full html document")
        Console.WriteLine()
        Console.WriteLine("Options:")
        Console.WriteLine("--toc               for html, it creates a table of contents")
        Console.WriteLine()
        Console.WriteLine("Available output targets for <of>:")
        Console.WriteLine("HTML               .html, <file>.html, <file>.htm")
        Console.WriteLine("GitHub Markdown    .md, <file>.md")
        Console.WriteLine("WikiPlex Markup    .wplex, <file>.wplex")
        Console.WriteLine()
        Console.WriteLine("Only giving the extension as <of> will write out a full document to stdout")
    End Sub

    Sub ProcessArgs(ByRef args As List(Of String), _
                    ByRef otherData As Dictionary(Of String, Object))
        Dim i As Integer = 0
        Do While (i < args.Count)
            If args(i) = "/?" OrElse args(i) = "-h" Then
                args.RemoveAt(i)
                Continue Do
            ElseIf args(i) = "--" Then
                args.RemoveAt(i)
                Exit Sub
            ElseIf args(i).StartsWith("--") Then
                Dim s = args(i).Substring(2)
                Select Case s
                    Case "toc"
                        otherData.Add("toc", True)
                        args.RemoveAt(i)
                        Continue Do
                    Case "help"
                        Usage()
                        Environment.Exit(255)
                End Select
            End If
            i = i + 1
        Loop
    End Sub

    Sub Main(ByVal argsRaw() As String)
        Console.OutputEncoding = System.Text.Encoding.UTF8
        Dim swIn As System.IO.StreamReader
        Dim otherData = New Dictionary(Of String, Object)

        Dim args = New List(Of String)(argsRaw)

        ProcessArgs(args, otherData)

        If args.Count > 0 Then
            If args(0) = "/?" OrElse args(0) = "-h" Then
                Usage()
                Environment.Exit(255)
                swIn = Nothing
            ElseIf args(0) = "-" Then
                swIn = New StreamReader(Console.OpenStandardInput())
            Else
                swIn = New StreamReader(args(0))
                otherData.Add("title", args(0))
            End If
        Else
            swIn = New StreamReader(Console.OpenStandardInput())
        End If

        Dim ot = OutputType.Html
        Dim sw As StreamWriter = Nothing
        If args.Count > 1 Then
            If args(1) = "-" Then
                sw = New StreamWriter(Console.OpenStandardOutput())
                ot = OutputType.Html
            ElseIf args(1) = ".html" Then
                sw = New StreamWriter(Console.OpenStandardOutput())
                ot = OutputType.Html
                otherData.Add("full", True)
            ElseIf args(1).EndsWith(".html") OrElse args(1).EndsWith(".htm") Then
                sw = New StreamWriter(args(1))
                ot = OutputType.Html
                otherData.Add("full", True)
            ElseIf args(1) = ".md" Then
                sw = New StreamWriter(Console.OpenStandardOutput())
                ot = OutputType.GHMarkDown
            ElseIf args(1).EndsWith(".md") Then
                sw = New StreamWriter(args(1))
                ot = OutputType.GHMarkDown
            ElseIf args(1) = ".wplex" Then
                sw = New StreamWriter(Console.OpenStandardOutput())
                ot = OutputType.WikiPlex
            ElseIf args(1).EndsWith(".wplex") Then
                sw = New StreamWriter(args(1))
                ot = OutputType.WikiPlex
            Else
                sw = New StreamWriter(args(1))
                ot = OutputType.Html
            End If
        Else
            sw = New StreamWriter(Console.OpenStandardOutput())
        End If

        Dim prs = New Parser(swIn)

        ' For Each tok In prs.Tokens
        '     sw.WriteLine(tok)
        ' Next

        prs.Parse()
        Dim rn = prs.RootNode

        Dim visitor As IOutputProvider = OutputProviderFactory(ot, sw, otherData)

        REM TODO figure out some better and more portable way
        REM      of generating the TOC. Having it intertwined with
        REM      regular traversal is stupid.
        visitor.Prologue()
        visitor.Process(rn)
        visitor.Epilogue()

        swIn.Close()
        sw.Close()
    End Sub

    Private Function OutputProviderFactory(ot As OutputType, _
                                           sw As TextWriter, _
                                           otherData As Dictionary(Of String, Object)) _
                                       As IOutputProvider
        Select Case ot
            Case OutputType.Html
                Dim fullHtml = False
                Dim title = "Document"
                Dim toc = False
                If otherData.ContainsKey("full") Then fullHtml = otherData("full")
                If otherData.ContainsKey("title") Then title = otherData("title")
                If otherData.ContainsKey("toc") Then toc = otherData("toc")
                Return New HtmlVisitor(sw, toc, fullHtml, title)
            Case OutputType.GHMarkDown
                Return New GHMarkDown(sw)
            Case OutputType.WikiPlex
                Return New WikiPlex(sw)
            Case Else
                Throw New Exception("Only doing this lest vb gives a warning")
        End Select
    End Function

End Module
