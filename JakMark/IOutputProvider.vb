Public Interface IOutputProvider
    Sub Prologue()
    Sub Process(rootNode As ITreeNode)
    Sub Epilogue()
End Interface
