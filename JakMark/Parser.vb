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
        ImageOpen
        WideImageOpen
    End Enum

    Public Type As TokenType
    Public Text As String

    Public Overrides Function ToString() As String
        Return "Token[" & Type.ToString() & ", " & Text & "]"
    End Function
End Structure


Partial Public Class Parser
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

    Public Sub Parse()
        ' Private _notes As Dictionary(Of String, Footnote)
        ' Private _links As Dictionary(Of String, Link)
        _notes = New Dictionary(Of String, Footnote)
        _links = New Dictionary(Of String, Link)
        RootNode = ParseEntryLevel()
    End Sub

End Class
