Imports System.IO
Imports System.Text.RegularExpressions

Public Class HtmlVisitor
    Implements IVisitor
    Implements IOutputProvider

    Private _fullHtml As Boolean
    Private _stream As TextWriter
    Private _title As String
    Private _footNotes As Dictionary(Of Integer, String)
    Private _usedHeadingIds As Dictionary(Of String, Integer)

    Public Sub New(sw As TextWriter, Optional fullHtml As Boolean = False, Optional title As String = "")
        Me._stream = sw
        _title = title
        _footNotes = New Dictionary(Of Integer, String)
        _usedHeadingIds = New Dictionary(Of String, Integer)
        _fullHtml = fullHtml
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
                          "code { font-family: monospace; white-space: pre-wrap; } .footnote { vertical-align: super; font-size: 50%; } .small_permalink { font-size: 50% ; text-decoration: none; color: black}")
        _stream.WriteLine("</head>")
        _stream.WriteLine("<body>")
    End Sub

    Public Sub Epilogue() Implements IOutputProvider.Epilogue
        Dim list = _footNotes.ToList().OrderBy(Function(kv As KeyValuePair(Of Integer, String))
                                                   Return kv.Key
                                               End Function).ToList()
        If list.Count > 0 Then
            _stream.WriteLine("<hr>")
        End If
        For Each li In list
            _stream.WriteLine("<p id=""fn:{0}"">{0}. {1} <a href=""#fnref:{0}"">Back</a></p>", li.Key, li.Value)
        Next

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
        _stream.WriteLine("<code>{0}</code>", node.Text)
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

    Private Function ExtractHeadingId(str As String) As String
        Dim r = New Regex("[a-zA-Z0-9]")
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
        _stream.WriteLine("<h{0} id=""{2}"">{1}<a class=""small_permalink"" href=""#{2}"">&nbsp;&nbsp;&#8734;</a></h{0}>", node.Level, node.Text, ExtractHeadingId(node.Text))
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
        _stream.Write("<p>")
        node.Stuff.Accept(Me)
        _stream.WriteLine("</p>")
    End Sub

    Public Sub Visit(node As LiteralLF) Implements IVisitor.Visit
        _stream.WriteLine("<br/>")
    End Sub

    Public Sub Visit(node As Whitespace) Implements IVisitor.Visit
        _stream.Write(" ")
    End Sub
End Class
