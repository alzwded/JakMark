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
        DoubleStar
        Pipe
        SquareOpen
        SquareClose
        Note
        NoteRefOpen
        RefOpen
        RefClose
        LParenOpen
        LParenClose
        TableOpen
        TableClose
        ListOpen
        ListClose
        DoubleSpace
        Dash
        Text
        CommentOpen
        CommentClose
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
        Tokenize()
    End Sub

    Private Sub Tokenize()
        Dim whitespaceTokens() = {" ", "" & vbTab}
        Dim lineFeedTokens() = {"" & vbLf, "" & vbCr, "" & vbCrLf}
        Dim interjectingDict As Dictionary(Of String, Token.TokenType) = New Dictionary(Of String, Token.TokenType) From {
            {"#", Token.TokenType.Hash},
            {"```", Token.TokenType.Fence},
            {"`", Token.TokenType.Backtick},
            {"\!", Token.TokenType.Escape},
            {"**", Token.TokenType.DoubleStar},
            {"*", Token.TokenType.Star},
            {"[^", Token.TokenType.Note},
            {":[^", Token.TokenType.NoteRefOpen},
            {":[", Token.TokenType.RefOpen},
            {"]:", Token.TokenType.RefClose},
            {"[", Token.TokenType.SquareOpen},
            {"]", Token.TokenType.SquareClose},
            {"(", Token.TokenType.LParenOpen},
            {")", Token.TokenType.LParenClose},
            {"<table>", Token.TokenType.TableOpen},
            {"|", Token.TokenType.Pipe},
            {"</table>", Token.TokenType.TableClose},
            {"<list>", Token.TokenType.ListOpen},
            {"-", Token.TokenType.Dash},
            {"</list>", Token.TokenType.ListClose},
            {"<!--", Token.TokenType.CommentOpen},
            {"-->", Token.TokenType.CommentClose}
            }
        Dim matchingTokens() = {Token.TokenType.Fence, Token.TokenType.Backtick, _
                                Token.TokenType.Escape, Token.TokenType.Star,
                                Token.TokenType.DoubleStar}
        Dim asymetricTokens = _
            New Dictionary(Of Token.TokenType, String) From {
                {Token.TokenType.SquareOpen, "]"},
                {Token.TokenType.Note, "]"},
                {Token.TokenType.NoteRefOpen, "]:"},
                {Token.TokenType.RefOpen, "]:"},
                {Token.TokenType.LParenOpen, ")"},
                {Token.TokenType.CommentOpen, "-->"}
                }

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

            If line = "  " Then
                _tokens.Add(New Token With {.Type = Token.TokenType.DoubleSpace})
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

                    Dim matchingToken As String = ""
                    Dim matchingTokenToken As Token.TokenType = Token.TokenType.Dash

                    If asymetricTokens.ContainsKey(kv.Value) Then
                        matchingToken = asymetricTokens(kv.Value)
                        matchingTokenToken = interjectingDict(matchingToken)
                    ElseIf matchingTokens.Contains(kv.Value) Then
                        matchingToken = kv.Key
                        matchingTokenToken = kv.Value
                    End If
                    If matchingToken.Length > 0 Then
                        Dim text = ""
                        ' consume input until ``` is found
                        Do While Not line.IndexOf(matchingToken) >= 0
                            text = text & line & vbLf
                            If _stream.Peek() < 0 Then
                                line = ""
                                Exit Do
                            End If
                            line = _stream.ReadLine
                        Loop

                        Dim pos = line.IndexOf(matchingToken)
                        If pos >= 0 Then
                            text = text & line.Substring(0, pos)
                            line = line.Substring(pos + matchingToken.Length)
                        ElseIf line.Length > kv.Key.Length Then
                            line = line.Substring(pos + matchingToken.Length)
                        Else
                            line = ""
                        End If

                        _tokens.Add(New Token With {.Type = Token.TokenType.Text, .Text = text})
                        _tokens.Add(New Token With {.Type = matchingTokenToken})
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
                ' Find first token...
                REM TODO fix this
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

    Private Function ParseHeading() As ITreeNode

    End Function

    Private Function ParseRef() As ITreeNode

    End Function

    Private Function ParseNoteRef() As ITreeNode

    End Function

    Private Function ParseParagraph() As ITreeNode

    End Function

    Private Function ParseEntryLevel() As ITreeNode
        Dim ret = New Container

        Do While _tokens.Count > 0
            Select Case _tokens.First().Type
                Case Token.TokenType.Whitespace
                    _tokens.Remove(_tokens.First())
                Case Token.TokenType.Hash
                    ret.Children.Add(ParseHeading())
                Case Token.TokenType.RefOpen
                    ret.Children.Add(ParseRef())
                Case Token.TokenType.NoteRefOpen
                    ret.Children.Add(ParseNoteRef())
                Case Else
                    ret.Children.Add(ParseParagraph())
            End Select
        Loop

        Return ret
    End Function

    Public Sub Parse()
        RootNode = ParseEntryLevel()
    End Sub

End Class
