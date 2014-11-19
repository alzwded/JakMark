Imports System.IO
Imports System.Text.RegularExpressions

Structure Token
    Enum TokenType
        Whitespace
        LineFeed
        Hash
        Fence
        Backtick
        Escape
        Star
        Pipe
        SquareOpen
        SquareClose
        Note
        NoteRef
        HrefClose
        LParenClose
        TableOpen
        TableClose
        ListOpen
        ListClose
        DoubleSpace
        Text
    End Enum

    Public Type As TokenType
    Public Text As String

    Public Overrides Function ToString() As String
        Return "Token[" & Type.ToString() & ", " & Text & "]"
    End Function
End Structure


Public Class Parser
    Private _stream As TextReader = Nothing
    Private _tokens As List(Of Token)
    Public Property RootNode As ITreeNode = Nothing
    Public ReadOnly Property Tokens As List(Of String)
        Get
            Return (From tok In _tokens
                    Select stringVal = tok.ToString()).ToList()
        End Get
    End Property

    Public Sub New(sw As TextReader)
        _stream = sw
        Parse()
    End Sub

    Private Sub Tokenize()
        Dim whitespaceTokens() = {" ", "" & vbTab}
        Dim lineFeedTokens() = {"" & vbLf, "" & vbCr, "" & vbCrLf}
        Dim interjectingDict As Dictionary(Of String, Token.TokenType) = New Dictionary(Of String, Token.TokenType) From {
            {"#", Token.TokenType.Hash},
            {"```", Token.TokenType.Fence},
            {"`", Token.TokenType.Backtick},
            {"|", Token.TokenType.Pipe},
            {"\!", Token.TokenType.Escape},
            {"*", Token.TokenType.Star},
            {"[", Token.TokenType.SquareOpen},
            {"]", Token.TokenType.SquareClose},
            {"[^", Token.TokenType.Note},
            {"]:", Token.TokenType.NoteRef},
            {"](", Token.TokenType.HrefClose},
            {")", Token.TokenType.LParenClose},
            {"<table>", Token.TokenType.TableOpen},
            {"</table>", Token.TokenType.TableClose},
            {"<list>", Token.TokenType.ListOpen},
            {"</list>", Token.TokenType.ListClose},
            {"  ", Token.TokenType.DoubleSpace}
            }
        ' Dim interjectingTokens() = {"#", "`", "\!", "*", "|", "[", "]", "[^", "]:", 
        ' "](", ")", "<table>", "</table>", "<list>", "</list>", "```", "  "}

        _tokens = New List(Of Token)

        If _stream.Peek() < 0 Then
            Exit Sub
        End If

        Dim line = _stream.ReadLine()
        Do While _stream.Peek() > -1
            If line.Length = 0 Then
                _tokens.Add(New Token With {.Type = Token.TokenType.LineFeed})

                If _stream.Peek() > -1 Then
                    line = _stream.ReadLine()
                    Continue Do
                Else
                    Exit Do
                End If
            End If

            For Each kv In interjectingDict
                If line.StartsWith(kv.Key) Then
                    _tokens.Add(New Token With {.Type = kv.Value})
                    line = line.Substring(kv.Key.Length)

                    If kv.Value = Token.TokenType.Fence _
                        OrElse kv.Value = Token.TokenType.Escape Then
                        Dim text = ""
                        ' consume input until ``` is found
                        Do While Not line.IndexOf(kv.Key) >= 0
                            text = text & line & vbLf
                            If _stream.Peek() < 0 Then
                                line = ""
                                Exit Do
                            End If
                            line = _stream.ReadLine
                        Loop

                        Dim pos = line.IndexOf(kv.Key)
                        If pos >= 0 Then
                            text = text & line.Substring(0, pos)
                            line = line.Substring(pos + kv.Key.Length)
                        ElseIf line.Length > kv.Key.Length Then
                            line = line.Substring(pos + kv.Key.Length)
                        Else
                            line = ""
                        End If

                        _tokens.Add(New Token With {.Type = Token.TokenType.Text, .Text = text})
                        _tokens.Add(New Token With {.Type = kv.Value})
                    End If

                    Continue Do
                End If
            Next

            Dim wsFound = False
            Do
                For Each tok In whitespaceTokens
                    If line.StartsWith(tok) Then
                        If Not wsFound Then _
                            _tokens.Add(New Token With {.Type = Token.TokenType.Whitespace})
                        line = line.Substring(tok.Length)
                        wsFound = True
                        Continue Do
                    End If
                Next
                Exit Do
            Loop
            If wsFound Then Continue Do

            For i = 0 To line.Length - 1
                If line(i) = " "c OrElse line(i) = vbTab Then
                    Dim text = line.Substring(0, i)
                    line = line.Substring(i)
                    _tokens.Add(New Token With {.Type = Token.TokenType.Text, .Text = text})
                    Continue Do
                End If
            Next
            _tokens.Add(New Token With {.Type = Token.TokenType.Text, .Text = line})
            line = ""
        Loop
    End Sub

    Private Sub Parse()
        Tokenize()
        For Each tok In _tokens
            Console.WriteLine(tok.ToString())
        Next
    End Sub

End Class
