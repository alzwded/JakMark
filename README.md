JakMark
=======

My very own markup language

The intent of this project is to provide a common language (for me) to write up markups/markdowns for various sites. I love MarkDown, but this project has come up out of my irritation with the incompatibilities between various MarkDown implementations, or sites that don't even use MarkDown in any form. And the fact that CommonMark doesn't support effin tables (except in HTML form which is impossible for a human to read in unrendered form).

Also, I wanted to practise writing a Recursive Descent parser because I've grown bored of LALR parsers.

Writing more output providers should be fairly easy since it's just a matter of traversing the AST which from the first look of it, it's *fitting* enough to support at least GHFMD, CM and wiki markup. If it isn't, well, back to the drawing board.

But how does it look like?
--------------------------

You can find an example and its HTML rendered form over in the [1.0 release](https://github.com/alzwded/JakMark/releases/tag/1.0)

TODO
----

* More output providers
  - [x] GitHub falvoured MarkDown
  - [ ] CommonMark
  - [x] CodePlex wiki markup
* Improvements
  - [ ] Improve Tokenzier to support some intermangling of markups
  - [ ] Improve Parser because right now I add some blank nodes in the AST
  - [ ] Improve both Tokenizer and Parser to give out diagnostics (e.g. `syntax error @row near token x` or something like that)
* Documentation
  - [ ] Write it. Right now it doesn't exist

If you want to use JakMark and you have some feature request (there are no bugs), drop a line.

License
-------

JakMark is released under the terms of the Simplified BSD License.
