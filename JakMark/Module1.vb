Imports System.IO

Module Module1
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

    Sub Usage()

    End Sub

    Sub Main(ByVal args() As String)
        REM If args.Count <> 2 Then
        REM     Usage()
        REM     Environment.Exit(255)
        REM End If

        Dim title = "Document"
        Dim swIn As System.IO.StreamReader
        If args.Length > 0 Then
            swIn = New StreamReader(args(0))
            title = args(1)
        Else
            swIn = New StreamReader(Console.OpenStandardInput())
        End If

        Dim sw As StreamWriter
        If args.Length > 1 Then
            sw = New StreamWriter(args(1))
        Else
            sw = New StreamWriter(Console.OpenStandardOutput())

        End If

        Dim prs = New Parser(swIn)

        ' For Each tok In prs.Tokens
        '     sw.WriteLine(tok)
        ' Next

        prs.Parse()
        Dim rn = prs.RootNode

        Dim htmlVisitor = New HtmlVisitor(sw, title)

        htmlVisitor.Prologue()
        htmlVisitor.Process(rn)
        htmlVisitor.Epilogue()

        swIn.Close()
        sw.Close()
    End Sub

End Module
