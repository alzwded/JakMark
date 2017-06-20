Imports System.IO
Imports System.Text.RegularExpressions

Public Class WikiPlex
    Implements IVisitor
    Implements IOutputProvider

    Private _stream As TextWriter
    Private _footNotes As Dictionary(Of Integer, String)
    Private _currentIndent = 0
    Private _blankLine = True

    Public Sub New(sw As TextWriter)
        Me._stream = sw
        _footNotes = New Dictionary(Of Integer, String)
    End Sub

    Public Sub Prologue() Implements IOutputProvider.Prologue
    End Sub

    Public Sub Epilogue() Implements IOutputProvider.Epilogue
        Dim list = _footNotes.ToList().OrderBy(Function(kv As KeyValuePair(Of Integer, String))
                                                   Return kv.Key
                                               End Function).ToList()
        If list.Count > 0 Then
            _stream.WriteLine()
            _stream.WriteLine("----")
            _stream.WriteLine()
        End If
        For Each li In list
            _stream.WriteLine("{0}. {1}", li.Key, li.Value)
        Next
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
        If Not _blankLine Then _stream.WriteLine()
        _stream.Write("{{")
        _stream.Write(node.Text)
        _stream.WriteLine("}}")
        _stream.WriteLine()
        _blankLine = True
    End Sub

    Public Sub Visit(node As Footnote) Implements IVisitor.Visit
        Dim newId = _footNotes.Count + 1
        _stream.WriteLine("^^{{""[{0}]""}}^^", newId)
        Dim sw = New StringWriter()
        Dim v = New WikiPlex(sw)
        v._blankLine = False
        node.Note.Accept(v)
        Dim asText As String = sw.GetStringBuilder().ToString()
        _footNotes.Add(newId, asText)
        _blankLine = False
    End Sub

    Public Sub Visit(node As FormattedText) Implements IVisitor.Visit
        Select Case node.Type
            Case FormattedText.FormattingType.Bold
                _stream.Write("*{0}*", node.Text)
            Case FormattedText.FormattingType.Italic
                _stream.Write("_{0}_", node.Text)
            Case FormattedText.FormattingType.Monospaced
                _stream.Write("{{{{{0}}}}}", node.Text)
        End Select
        _blankLine = False
    End Sub

    Public Sub Visit(node As Heading) Implements IVisitor.Visit
        If Not _blankLine Then _stream.WriteLine()
        _stream.WriteLine("{0} {1}", New String("!"c, node.Level), node.Text)
        _stream.WriteLine()
        _blankLine = True
    End Sub

    Public Sub Visit(node As Link) Implements IVisitor.Visit
        _stream.Write("[url:{0}|{1}]", node.Text, node.Link)
        _blankLine = False
    End Sub

    Public Sub Visit(node As Image) Implements IVisitor.Visit
        If node.Wide And Not _blankLine Then _stream.WriteLine()
        _stream.Write("[image:{0}|{1}]", node.Text, node.Path)
        If node.Wide Then
            _stream.WriteLine()
            _stream.WriteLine()
            _blankLine = True
        Else
            _blankLine = False
        End If
    End Sub

    Public Sub Visit(node As List) Implements IVisitor.Visit
        If Not _blankLine Then _stream.WriteLine()
        If node.Numbered Then
            For Each i In node.Items
                _stream.Write(New String("#"c, _currentIndent + 1))
                _blankLine = False
                _currentIndent = (_currentIndent + 1)
                i.Accept(Me)
                _currentIndent = (_currentIndent - 1)
                If Not _blankLine Then _stream.WriteLine()
            Next
        Else
            For Each i In node.Items
                _stream.Write(New String("*"c, _currentIndent + 1))
                _blankLine = False
                _currentIndent = (_currentIndent + 1)
                i.Accept(Me)
                _currentIndent = (_currentIndent - 1)
                If Not _blankLine Then _stream.WriteLine()
            Next
        End If
        _blankLine = True
    End Sub

    Public Sub Visit(node As PlainText) Implements IVisitor.Visit
        _stream.Write("{{""{0}""}}", node.Text)
        _blankLine = False
    End Sub

    Public Sub Visit(node As Table) Implements IVisitor.Visit
        If Not _blankLine Then _stream.WriteLine()
        For Each i In node.Header
            _stream.Write("||{0}", i.Trim())
        Next
        _stream.WriteLine("||")
        For Each i In node.Lines
            For Each j In i
                _stream.Write("|")
                j.Accept(Me)
            Next
            _stream.WriteLine("|")
        Next
        _stream.WriteLine()
        _blankLine = True
    End Sub

    Public Sub Visit(node As Paragraph) Implements IVisitor.Visit
        If Not _blankLine Then _stream.WriteLine()
        node.Stuff.Accept(Me)
        _stream.WriteLine()
        _stream.WriteLine()
        _blankLine = True
    End Sub

    Public Sub Visit(node As LiteralLF) Implements IVisitor.Visit
        _stream.WriteLine("{""" & vbLf & """}")
    End Sub

    Public Sub Visit(node As Whitespace) Implements IVisitor.Visit
        If Not _blankLine Then
            _stream.Write(" ")
            _blankLine = False
        End If
    End Sub
End Class
