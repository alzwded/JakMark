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
    End Enum

    Sub Usage()
        Dim appName = System.Environment.GetCommandLineArgs()(0)
        Console.WriteLine("{0} v{1} Copyright (c) 2014 Vlad Meșco", appName, My.Application.Info.Version.ToString())
        Console.WriteLine("Usage:")
        Console.WriteLine("{0} /?          print this message", appName)
        Console.WriteLine("{0} [if] [of]   reads the JM file <if> and writes html to <of>", appName)
        Dim blanks = New String(" "c, appName.Length)
        Console.WriteLine("{0}             infile or outfile can be ""-"" for stdin/stdout respectively", blanks)
        Console.WriteLine("{0}             if <of> is a file name, the result is a full html document", blanks)
    End Sub

    Sub Main(ByVal args() As String)
        Console.OutputEncoding = System.Text.Encoding.UTF8
        Dim swIn As System.IO.StreamReader
        Dim otherData = New Dictionary(Of String, Object)

        If args.Length > 0 Then
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
        If args.Length > 1 Then
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
                REM TODO .md type output
            ElseIf args(1) = ".md" Then
                sw = New StreamWriter(Console.OpenStandardOutput())
                ot = OutputType.GHMarkDown
            ElseIf args(1).EndsWith(".md") Then
                sw = New StreamWriter(args(1))
                ot = OutputType.GHMarkDown
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
                If otherData.ContainsKey("full") Then fullHtml = otherData("full")
                If otherData.ContainsKey("title") Then title = otherData("title")
                Return New HtmlVisitor(sw, fullHtml, title)
            Case OutputType.GHMarkDown
                Return New GHMarkDown(sw)
            Case Else
                Throw New Exception("Only doing this lest vb gives a warning")
        End Select
    End Function

End Module
