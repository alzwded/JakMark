Partial Public Class Parser


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
            {"{", Token.TokenType.LParenOpen},
            {"}", Token.TokenType.LParenClose},
            {"<table>", Token.TokenType.TableOpen},
            {"|", Token.TokenType.Pipe},
            {"</table>", Token.TokenType.TableClose},
            {"<list>", Token.TokenType.ListOpen},
            {"+", Token.TokenType.Dash},
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
                {Token.TokenType.LParenOpen, "}"},
                {Token.TokenType.CommentOpen, "-->"}
                }

        _tokens = New List(Of Token)

        If _stream.Peek() < 0 Then
            Exit Sub
        End If

        Dim line = _stream.ReadLine()
        Do While line.Length > 0 OrElse _stream.Peek() > -1
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

        If _tokens.Count > 0 _
            AndAlso Not _tokens(_tokens.Count - 1).Type = Token.TokenType.LineFeed Then
            _tokens.Add(New Token With {.Type = Token.TokenType.LineFeed})
        End If
    End Sub


End Class
