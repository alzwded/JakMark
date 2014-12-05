﻿Imports System.IO
Imports System.Text.RegularExpressions

Public Class HtmlVisitor
    Implements IVisitor
    Implements IOutputProvider

    Private _fullHtml As Boolean
    Private _stream As TextWriter
    Private _title As String
    Private _footNotes As Dictionary(Of Integer, String)
    Private _usedHeadingIds As Dictionary(Of String, Integer)
    Private _toc As Boolean
    Private _tocBuilder As List(Of Tuple(Of Integer, String, String))

    Public Sub New(sw As TextWriter, Optional toc As Boolean = False, Optional fullHtml As Boolean = False, Optional title As String = "")
        Me._stream = sw
        _title = title
        _footNotes = New Dictionary(Of Integer, String)
        _usedHeadingIds = New Dictionary(Of String, Integer)
        _fullHtml = fullHtml
        _toc = toc
        If toc Then
            _tocBuilder = New List(Of Tuple(Of Integer, String, String))
        End If
    End Sub

    Public Sub Prologue() Implements IOutputProvider.Prologue
        If Not _fullHtml Then Return
        ' write out html, head, default css etc
        _stream.WriteLine("<!DOCTYPE html>")
        _stream.WriteLine("<html><head>")
        If _title.Length > 0 Then
            _stream.WriteLine("<title>{0}</title>", _title)
        End If
        _stream.WriteLine("<style>{0}</style>",
                          "div.para { margin: 1em 0px; }" & _
                          "code { font-family: monospace; white-space: pre-wrap; page-break-inside: avoid; page-break-before: auto; page-break-after: auto; position: relative}" & _
                          ".footnote { vertical-align: super; font-size: 50%; }" & _
                          "a.small_permalink { text-decoration: none; color:black; }" & _
                          "a.small_permalink span { color: transparent; font-size: 50% }" & _
                          "a.small_permalink:hover span { color:black }" & _
                          "pre { page-break-inside: avoid }" & _
                          ".headingNoPageBreak { page-break-after: avoid; page-break-inside: avoid; position: relative; display: block } ")
        If _toc Then
        End If
        _stream.WriteLine("</head>")
        If _toc Then
            REM _stream.WriteLine("<body onload=""__x_initTOC()"">")
            _stream.WriteLine("<body>")
            _stream.WriteLine("<span id=""__x_toc_1""></span>")
        Else
            _stream.WriteLine("<body>")
        End If
    End Sub

    Public Sub Epilogue() Implements IOutputProvider.Epilogue
        Dim list = _footNotes.ToList().OrderBy(Function(kv As KeyValuePair(Of Integer, String))
                                                   Return kv.Key
                                               End Function).ToList()
        If list.Count > 0 Then
            _stream.WriteLine("<hr>")
        End If
        For Each li In list
            _stream.WriteLine("<div class=""para"" id=""fn:{0}"">{0}. {1} <a href=""#fnref:{0}"">Back</a></p>", li.Key, li.Value)
        Next

        If _toc Then
            _stream.WriteLine("<span id=""__x_toc_2"">")
            _stream.WriteLine("<h1>Contents</h1>")
            _stream.WriteLine("<ol>")
            Dim prev = 1
            For Each i In _tocBuilder
                If i.Item1 > prev Then
                    Dim lvl = prev
                    Do While lvl < i.Item1
                        _stream.WriteLine("<ol>")
                        lvl = lvl + 1
                    Loop
                ElseIf i.Item1 < prev Then
                    Dim lvl = prev
                    Do While lvl > i.Item1
                        _stream.WriteLine("</ol>")
                        lvl = lvl - 1
                    Loop
                End If
                prev = i.Item1
                _stream.WriteLine("<li><a style=""color:black;text-decoration:none""href=""#{1}"">{0}</a></li>", i.Item2, i.Item3)
            Next
            Do While prev > 0
                prev = prev - 1
                _stream.Write("</ol>")
            Loop
            _stream.WriteLine("</span>")
            REM TODO refactore architecture to be able to place TOC
            REM      at the begining of the document without resorting
            REM      to javascript-based hacks.
            REM      It's probably needed to do two passes, one for the TOC
            REM      and one for actually writing the document
            _stream.WriteLine("<script type=""text/javascript"">")
            _stream.WriteLine("" & vbLf & _
                              "    document.getElementById('__x_toc_1').innerHTML = document.getElementById('__x_toc_2').innerHTML" & vbLf & _
                              "    document.getElementById('__x_toc_2').style.visibility = 'hidden'" & vbLf & _
                              "    document.getElementById('__x_toc_2').style.display = 'none'" & vbLf & _
                              "" & vbLf)
            _stream.WriteLine("</script>")
        End If

        If Not _fullHtml Then Return
        _stream.WriteLine("</body></html>")
    End Sub

    Public Sub Process(rootNode As ITreeNode) Implements IOutputProvider.Process
        rootNode.Accept(Me)
    End Sub

    Public Sub Visit(node As Container) Implements IVisitor.Visit
        For Each subNode In node.Children
            subNode.Accept(Me)
        Next
    End Sub

    Public Sub Visit(node As Fenced) Implements IVisitor.Visit
        _stream.WriteLine("<div style=""page-break-inside:avoid""><code>{0}</code></div>", node.Text)
    End Sub

    Public Sub Visit(node As Footnote) Implements IVisitor.Visit
        Dim newId = _footNotes.Count + 1
        _stream.WriteLine("<a class=""footnote"" id=""fnref:{0}"" href=""#fn:{0}"">{0}</a>", newId)
        Dim sw = New StringWriter()
        Dim v = New HtmlVisitor(sw)
        node.Note.Accept(v)
        Dim asText As String = sw.GetStringBuilder().ToString()
        _footNotes.Add(newId, asText)
    End Sub

    Public Sub Visit(node As FormattedText) Implements IVisitor.Visit
        Select Case node.Type
            Case FormattedText.FormattingType.Bold
                _stream.Write("<strong>{0}</strong>", node.Text)
            Case FormattedText.FormattingType.Italic
                _stream.Write("<em>{0}</em>", node.Text)
            Case FormattedText.FormattingType.Monospaced
                _stream.Write("<code>{0}</code>", node.Text)
        End Select
    End Sub

    Private Function GenerateHeadingId(str As String) As String
        Static r = New Regex("[a-zA-Z0-9]")
        Dim chars = From c In str.ToCharArray()
                    Where r.Match(c.ToString()).Success
                    Select s = c.ToString()

        Dim f As System.Func(Of String, String, String) = Function(c As String, s As String) As String
                                                              Return c & s
                                                          End Function
        Dim proposedTitle As String = chars.Aggregate("", f)
        If _usedHeadingIds.ContainsKey(proposedTitle) Then
            _usedHeadingIds(proposedTitle) = _usedHeadingIds(proposedTitle) + 1
            proposedTitle = proposedTitle & "_" & _usedHeadingIds(proposedTitle).ToString()
        Else
            _usedHeadingIds.Add(proposedTitle, 0)
        End If
        Return proposedTitle
    End Function

    Public Sub Visit(node As Heading) Implements IVisitor.Visit
        Dim hid = GenerateHeadingId(node.Text)
        _stream.WriteLine("<div class=""headingNoPageBreak""><a class=""small_permalink"" href=""#{2}""><h{0} class=""jmheading"" id=""{2}"">{1}" & _
                          "<span>&nbsp;&nbsp;&#8734;</span></h{0}></a></div>", node.Level, node.Text, hid)
        If _toc Then
            _tocBuilder.Add(New Tuple(Of Integer, String, String)(node.Level, node.Text, hid))
        End If
    End Sub

    Public Sub Visit(node As Link) Implements IVisitor.Visit
        _stream.Write("<a href=""{0}"">{1}</a>", node.Link, node.Text)
    End Sub

    Public Sub Visit(node As List) Implements IVisitor.Visit
        If node.Numbered Then
            _stream.WriteLine("<ol>")
        Else
            _stream.WriteLine("<ul>")
        End If

        For Each i In node.Items
            _stream.Write("<li>")
            i.Accept(Me)
            _stream.WriteLine("</li>")
        Next

        If node.Numbered Then
            _stream.WriteLine("</ol>")
        Else
            _stream.WriteLine("</ul>")
        End If
    End Sub

    Public Sub Visit(node As PlainText) Implements IVisitor.Visit
        _stream.Write(System.Web.HttpUtility.HtmlEncode(node.Text))
    End Sub

    Public Sub Visit(node As Table) Implements IVisitor.Visit
        _stream.WriteLine("<table>")
        _stream.WriteLine("<thead>")
        _stream.Write("<tr>")
        For Each i In node.Header
            _stream.Write("<th>{0}</th>", i)
        Next
        _stream.WriteLine("</tr>")
        _stream.WriteLine("</thead>")
        _stream.WriteLine("<tbody>")
        For Each i In node.Lines
            _stream.WriteLine("<tr>")
            For Each j In i
                _stream.Write("<td>")
                j.Accept(Me)
                _stream.WriteLine("</td>")
            Next
            _stream.WriteLine("</tr>")
        Next
        _stream.WriteLine("</tbody>")
        _stream.WriteLine("</table>")
    End Sub

    Public Sub Visit(node As Paragraph) Implements IVisitor.Visit
        _stream.Write("<div class=""para"">")
        node.Stuff.Accept(Me)
        _stream.WriteLine("</div>")
    End Sub

    Public Sub Visit(node As LiteralLF) Implements IVisitor.Visit
        _stream.WriteLine("<br/>")
    End Sub

    Public Sub Visit(node As Whitespace) Implements IVisitor.Visit
        _stream.Write(" ")
    End Sub
End Class
