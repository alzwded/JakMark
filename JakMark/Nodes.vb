Public Interface IVisitor
    ' line entry-point nodes
    Sub Visit(node As Fenced)    ' has text
    Sub Visit(node As Heading)   ' has text
    ' line entry-point container nodes
    Sub Visit(node As Container) ' has children
    Sub Visit(node As Table)     ' has children
    Sub Visit(node As List)      ' has children
    Sub Visit(node As Paragraph) ' has one child
    ' inline nodes, contained by one of the above
    Sub Visit(node As PlainText)
    Sub Visit(node As FormattedText)
    Sub Visit(node As Link)
    ' Footnote is an inline/entry-point hybrid
    ' e.g. its entry point is at the end of the document for HTML
    '      while it's inlined where declared as an anchor
    Sub Visit(node As Footnote)  ' has one child
End Interface

Public Interface ITreeNode
    Sub Accept(visitor As IVisitor)
End Interface

Public Class PlainText
    Implements ITreeNode

    Public Property Text As String

    Sub New(Text As String)
        Me.Text = Text
    End Sub

    Public Sub Accept(visitor As IVisitor) Implements ITreeNode.Accept
        visitor.Visit(Me)
    End Sub
End Class

Public Class FormattedText
    Implements ITreeNode

    Public Enum FormattingType
        Bold
        Italic
        Monospaced
    End Enum

    Public Property Text As String
    Public Property Type As FormattingType = FormattingType.Bold

    Public Sub Accept(visitor As IVisitor) Implements ITreeNode.Accept
        visitor.Visit(Me)
    End Sub
End Class

Public Class Container
    Implements ITreeNode

    Private _children As List(Of ITreeNode) = New List(Of ITreeNode)

    Public ReadOnly Property Children As List(Of ITreeNode)
        Get
            Return _children
        End Get
    End Property

    Public Sub Accept(visitor As IVisitor) Implements ITreeNode.Accept
        visitor.Visit(Me)
    End Sub
End Class

Public Class Fenced
    Implements ITreeNode

    Public Property Text As String

    Public Sub Accept(visitor As IVisitor) Implements ITreeNode.Accept
        visitor.Visit(Me)
    End Sub
End Class

Public Class List
    Implements ITreeNode

    Private _items As List(Of ITreeNode) = New List(Of ITreeNode)

    Public Property Numbered As Boolean = False
    Public ReadOnly Property Items As List(Of ITreeNode)
        Get
            Return _items
        End Get
    End Property

    Public Sub Accept(visitor As IVisitor) Implements ITreeNode.Accept
        visitor.Visit(Me)
    End Sub
End Class

Public Class Table
    Implements ITreeNode

    Private _header As List(Of String) = New List(Of String)
    Private _lines As List(Of List(Of ITreeNode)) = New List(Of List(Of ITreeNode))

    Public ReadOnly Property Header As List(Of String)
        Get
            Return _header
        End Get
    End Property

    Public ReadOnly Property Lines As List(Of List(Of ITreeNode))
        Get
            Return _lines
        End Get
    End Property

    Public Sub Accept(visitor As IVisitor) Implements ITreeNode.Accept
        visitor.Visit(Me)
    End Sub
End Class

Public Class Link
    Implements ITreeNode

    Public Property Text As String
    Public Property Link As String

    Public Sub Accept(visitor As IVisitor) Implements ITreeNode.Accept
        visitor.Visit(Me)
    End Sub
End Class

Public Class Footnote
    Implements ITreeNode

    Public Property Note As ITreeNode = Nothing

    Public Sub Accept(visitor As IVisitor) Implements ITreeNode.Accept
        visitor.Visit(Me)
    End Sub
End Class

Public Class Heading
    Implements ITreeNode

    Public Property Level As Integer
    Public Property Text As String

    Public Sub Accept(visitor As IVisitor) Implements ITreeNode.Accept
        visitor.Visit(Me)
    End Sub
End Class

Public Class Paragraph
    Implements ITreeNode

    Public Property Stuff As ITreeNode = Nothing

    Public Sub Accept(visitor As IVisitor) Implements ITreeNode.Accept
        visitor.Visit(Me)
    End Sub
End Class