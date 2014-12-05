Partial Public Class Parser

    Private _notes As Dictionary(Of String, Footnote)
    Private _links As Dictionary(Of String, Link)

    Private Function ParseHeading() As ITreeNode
        Dim level = 0

        Do While _tokens.First().Type = Token.TokenType.Hash
            _tokens.Remove(_tokens.First())
            level = level + 1
            Do While _tokens.First().Type = Token.TokenType.Whitespace
                _tokens.Remove(_tokens.First())
            Loop
        Loop

        Dim text = New System.Text.StringBuilder()
        Do Until _tokens.First().Type = Token.TokenType.LineFeed
            Select Case _tokens.First().Type
                Case Token.TokenType.Whitespace
                    text.Append(" ")
                    _tokens.Remove(_tokens.First())
                Case Token.TokenType.Text
                    text.Append(_tokens.First().Text)
                    _tokens.Remove(_tokens.First())
            End Select
        Loop
        _tokens.Remove(_tokens.First())

        Return New Heading() With {
            .Level = level,
            .Text = text.ToString()
            }
    End Function

    Private Sub ConsumeComment()
        Do
            If _tokens.Count <= 0 Then Throw New SyntaxErrorException("Could not find closing comment token -->")
            Dim tok = _tokens.First()
            _tokens.Remove(tok)
            If tok.Type = Token.TokenType.CommentClose Then Return
        Loop
    End Sub

    Private Sub ParseRef()
        _tokens.Remove(_tokens.First())
        Dim text = _tokens.First()
        _tokens.Remove(text)
        If text.Type <> Token.TokenType.Text Then Throw New SyntaxErrorException()
        If _tokens.First().Type <> Token.TokenType.RefClose Then Throw New SyntaxErrorException()
        _tokens.Remove(_tokens.First())
        Do While _tokens.First().Type = Token.TokenType.Whitespace
            _tokens.Remove(_tokens.First())
        Loop
        Dim link = _tokens.First()
        _tokens.Remove(link)
        If link.Type <> Token.TokenType.Text Then Throw New SyntaxErrorException()
        If _links.ContainsKey(text.Text) Then
            _links(text.Text).Link = link.Text
        Else
            _links.Add(text.Text, New Link() With {
                       .Text = text.Text,
                       .Link = link.Text
                       })
        End If
        If _tokens.First().Type <> Token.TokenType.LineFeed Then Throw New SyntaxErrorException()
        _tokens.Remove(_tokens.First())
    End Sub
    Private Sub ParseNoteRef()
        _tokens.Remove(_tokens.First())
        Dim text = _tokens.First()
        _tokens.Remove(text)
        If text.Type <> Token.TokenType.Text Then Throw New SyntaxErrorException()
        If _tokens.First().Type <> Token.TokenType.RefClose Then Throw New SyntaxErrorException()
        _tokens.Remove(_tokens.First())
        Do While _tokens.First().Type = Token.TokenType.Whitespace
            _tokens.Remove(_tokens.First())
        Loop

        Dim collection = New Container()
        Do Until _tokens.First().Type = Token.TokenType.LineFeed
            collection.Children.Add(ParseNext())
        Loop
        _tokens.Remove(_tokens.First())

        If _notes.ContainsKey(text.Text) Then
            _notes(text.Text).Note = collection
        Else
            _notes.Add(text.Text, New Footnote() With {
                       .Note = collection
                       })
        End If
    End Sub
    Private Function ParseEscapedText() As ITreeNode
        _tokens.Remove(_tokens.First())
        Dim tok = _tokens.First()
        _tokens.Remove(tok)
        _tokens.Remove(_tokens.First())

        Return New PlainText(tok.Text)
    End Function
    Private Function ParseBoldText() As ITreeNode
        _tokens.Remove(_tokens.First())
        Dim tok = _tokens.First()
        _tokens.Remove(tok)
        _tokens.Remove(_tokens.First())

        Return New FormattedText With {
            .Type = FormattedText.FormattingType.Bold,
            .Text = tok.Text
            }
    End Function
    Private Function ParseItalicText() As ITreeNode
        _tokens.Remove(_tokens.First())
        Dim tok = _tokens.First()
        _tokens.Remove(tok)
        _tokens.Remove(_tokens.First())

        Return New FormattedText With {
            .Type = FormattedText.FormattingType.Italic,
            .Text = tok.Text
            }
    End Function
    Private Function ParseMonoText() As ITreeNode
        _tokens.Remove(_tokens.First())
        Dim tok = _tokens.First()
        _tokens.Remove(tok)
        _tokens.Remove(_tokens.First())

        Return New FormattedText With {
            .Type = FormattedText.FormattingType.Monospaced,
            .Text = tok.Text
            }
    End Function
    Private Function ParseLink() As ITreeNode
        _tokens.Remove(_tokens.First())
        Dim text = _tokens.First()
        _tokens.Remove(text)
        If text.Type <> Token.TokenType.Text Then Throw New SyntaxErrorException()
        If _tokens.First().Type <> Token.TokenType.SquareClose Then Throw New SyntaxErrorException()
        _tokens.Remove(_tokens.First())
        If _tokens.First().Type = Token.TokenType.LParenOpen Then
            _tokens.Remove(_tokens.First())
            Dim linkk = _tokens.First()
            _tokens.Remove(linkk)
            If _tokens.First().Type <> Token.TokenType.LParenClose Then Throw New SyntaxErrorException()
            _tokens.Remove(_tokens.First())

            Return New Link() With {
                .Text = text.Text,
                .Link = linkk.Text}
        Else
            Dim l = New Link() With {
                .Text = text.Text,
                .Link = ""
                }
            If Not _links.ContainsKey(text.Text) Then
                _links.Add(text.Text, l)
            End If
            Return l
        End If
    End Function
    Private Function ParseNote() As ITreeNode
        _tokens.Remove(_tokens.First())
        Dim text = _tokens.First()
        _tokens.Remove(text)
        If text.Type <> Token.TokenType.Text Then Throw New SyntaxErrorException()
        If _tokens.First().Type <> Token.TokenType.SquareClose Then Throw New SyntaxErrorException()
        _tokens.Remove(_tokens.First())
        
        Dim l = New Footnote() With {
            .Note = New Container()
            }
        _notes.Add(text.Text, l)
        Return l
    End Function

    Private Function ParseTableHeaderColumn() As String
        REM remove initial whitespace
        Do While _tokens.First().Type = Token.TokenType.Whitespace
            _tokens.Remove(_tokens.First())
        Loop
        Dim ret = New System.Text.StringBuilder()
        Do Until _tokens.First().Type = Token.TokenType.Pipe _
            OrElse _tokens.First().Type = Token.TokenType.LineFeed
            Dim thisOne = _tokens.First()
            _tokens.Remove(thisOne)
            Select Case thisOne.Type
                Case Token.TokenType.Text
                    ret.Append(thisOne.Text)
                Case Token.TokenType.Whitespace
                    ret.Append(" ")
                Case Else
                    Throw New SyntaxErrorException()
            End Select
        Loop
        If _tokens.First().Type = Token.TokenType.Pipe Then
            _tokens.Remove(_tokens.First())
        End If
        Return ret.ToString()
    End Function
    Private Function ParseTableColumn() As ITreeNode
        Dim collection = New Container()
        Do Until _tokens.First().Type = Token.TokenType.Pipe _
            OrElse _tokens.First().Type = Token.TokenType.LineFeed _
            OrElse _tokens.First().Type = Token.TokenType.TableClose
            Dim tok = ParseNext()
            collection.Children.Add(tok)
        Loop
        If _tokens.First().Type = Token.TokenType.Pipe Then _tokens.Remove(_tokens.First())
        Return collection
    End Function
    Private Function ParseTableLine() As List(Of ITreeNode)
        Dim ret = New List(Of ITreeNode)
        Do Until _tokens.First().Type = Token.TokenType.LineFeed _
            OrElse _tokens.First().Type = Token.TokenType.TableClose
            Dim node = ParseTableColumn()
            ret.Add(node)
        Loop
        If _tokens.First().Type = Token.TokenType.LineFeed Then _tokens.Remove(_tokens.First())
        Return ret
    End Function
    Private Function ParseTable() As ITreeNode
        _tokens.Remove(_tokens.First())
        Do While _tokens.First().Type = Token.TokenType.LineFeed
            _tokens.Remove(_tokens.First())
        Loop

        Dim table = New Table()

        REM parse header
        Do Until _tokens.First().Type = Token.TokenType.LineFeed
            Dim colName = ParseTableHeaderColumn()
            table.Header.Add(colName)
        Loop
        _tokens.Remove(_tokens.First())

        Do Until _tokens.First().Type = Token.TokenType.TableClose
            table.Lines.Add(ParseTableLine())
        Loop
        _tokens.Remove(_tokens.First())

        Return table
    End Function
    Private Function ParseList() As ITreeNode
        _tokens.Remove(_tokens.First())
        Do While _tokens.First().Type = Token.TokenType.LineFeed _
            OrElse _tokens.First().Type = Token.TokenType.Whitespace
            _tokens.Remove(_tokens.First())
        Loop

        Dim list = New List()
        list.Numbered = (_tokens.First().Type = Token.TokenType.Hash)

        Do
            If _tokens.First().Type = Token.TokenType.ListClose Then Exit Do
            If _tokens.First().Type = Token.TokenType.Hash OrElse
                _tokens.First().Type = Token.TokenType.Dash Then
                list.Items.Add(New Container())
                _tokens.Remove(_tokens.First())
            End If

            If _tokens.First().Type = Token.TokenType.LineFeed Then
                _tokens.Remove(_tokens.First())
                Continue Do
            Else
                Dim tok = ParseNext()
                CType(list.Items(list.Items.Count - 1), Container).Children.Add(tok)
            End If
        Loop
        _tokens.Remove(_tokens.First())

        Return list
    End Function
    Private Function ParseFence() As ITreeNode
        _tokens.Remove(_tokens.First())
        Dim text = _tokens.First()
        _tokens.Remove(text)
        If text.Type <> Token.TokenType.Text Then Throw New SyntaxErrorException()
        If _tokens.First().Type <> Token.TokenType.Fence Then Throw New SyntaxErrorException()
        _tokens.Remove(_tokens.First())

        Return New Fenced() With {
            .Text = text.Text
            }
    End Function

    Private Function ParseNext() As ITreeNode
        Do
            If _tokens.Count <= 0 Then Throw New SyntaxErrorException("Could not find closing comment token -->")
            Dim tok = _tokens.First()
            Select Case tok.Type
                Case Token.TokenType.Text
                    _tokens.Remove(tok)
                    Return New PlainText(tok.Text)
                Case Token.TokenType.Whitespace
                    _tokens.Remove(tok)
                    Return New Whitespace()
                Case Token.TokenType.CommentOpen
                    ConsumeComment()
                Case Token.TokenType.Escape
                    Return ParseEscapedText()
                Case Token.TokenType.DoubleStar
                    Return ParseBoldText()
                Case Token.TokenType.Star
                    Return ParseItalicText()
                Case Token.TokenType.Backtick
                    Return ParseMonoText()
                Case Token.TokenType.SquareOpen
                    Return ParseLink()
                Case Token.TokenType.Note
                    Return ParseNote()
                Case Token.TokenType.TableOpen
                    Return ParseTable()
                Case Token.TokenType.ListOpen
                    Return ParseList()
                Case Token.TokenType.Fence
                    Return ParseFence()
                Case Token.TokenType.Pipe
                    _tokens.Remove(tok)
                    Return New PlainText("|")
                Case Token.TokenType.Hash
                    _tokens.Remove(tok)
                    Return New PlainText("#")
                Case Token.TokenType.Dash
                    _tokens.Remove(tok)
                    Return New PlainText("-")
                Case Token.TokenType.LParenOpen
                    _tokens.Remove(tok)
                    Return New PlainText("(")
                Case Token.TokenType.LParenClose
                    _tokens.Remove(tok)
                    Return New PlainText(")")
                Case Token.TokenType.DoubleSpace
                    _tokens.Remove(tok)
                    Return New LiteralLF()
                Case Else
                    REM FIXME this shouldn't be necessary...
                    REM       probably something stupid with weird chars
                    Return New Container()
            End Select
        Loop
    End Function

    Private Function ParseParagraph() As ITreeNode
        Dim collection = New Container

        REM purge initial whitespace
        Do While _tokens.First().Type = Token.TokenType.Whitespace
            _tokens.Remove(_tokens.First())
        Loop
        If _tokens.First().Type = Token.TokenType.LineFeed Then
            _tokens.Remove(_tokens.First())
            Return collection
        End If

        Do Until _tokens.Count = 0 _
            OrElse _tokens.First().Type = Token.TokenType.LineFeed
            If collection.Children.Count > 0 Then collection.Children.Add(New Whitespace())
            Do Until _tokens.First().Type = Token.TokenType.LineFeed
                collection.Children.Add(ParseNext())
            Loop
            _tokens.Remove(_tokens.First())
        Loop
        _tokens.Remove(_tokens.First())

        Return New Paragraph With {.Stuff = collection}
    End Function

    Private Function ParseEntryLevel() As ITreeNode
        Dim ret = New Container

        Do While _tokens.Count > 0
            Select Case _tokens.First().Type
                Case Token.TokenType.Whitespace
                    ret.Children.Add(New Whitespace())
                    _tokens.Remove(_tokens.First())
                Case Token.TokenType.LineFeed
                    _tokens.Remove(_tokens.First())
                Case Token.TokenType.Hash
                    ret.Children.Add(ParseHeading())
                Case Token.TokenType.RefOpen
                    ParseRef()
                Case Token.TokenType.NoteRefOpen
                    ParseNoteRef()
                Case Token.TokenType.CommentOpen
                    ConsumeComment()
                    'Case Token.TokenType.ListOpen
                    '    REM apparently a list can't be embedded inside a P tag
                    '    ret.Children.Add(ParseList())
                    'Case Token.TokenType.TableOpen
                    '    REM apparently a table can't be embedded inside a P tag
                    '    ret.Children.Add(ParseTable())
                Case Else
                    ret.Children.Add(ParseParagraph())
            End Select
        Loop

        Return ret
    End Function

End Class
