JakMark
=======

My very own markup language

The intent of this project is to provide a common language (for me) to write up markups/markdowns for various sites. I love MarkDown, but this project has come up out of my irritation with the incompatibilities between various MarkDown implementations, or sites that don't even use MarkDown in any form. And the fact that CommonMark doesn't support effin tables (except in HTML form which is impossible for a human to read in unrendered form).

Also, I wanted to practise writing a Recursive Descent parser because I've grown bored of LALR parsers.

Writing more output providers should be fairly easy since it's just a matter of traversing the AST which from the first look of it, it's *fitting* enough to support at least GHFMD, CM and wiki markup. If it isn't, well, back to the drawing board.

But how does it look like?
--------------------------

You can find an example and its HTML rendered form over in the [1.1 release](https://github.com/alzwded/JakMark/releases/tag/1.1)

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
  - [ ] Make output providers plugin based
  - [x] Image tag
* Documentation
  - [ ] Write it. Right now it doesn't exist
* Portability
  - [x] Make it run on the latest stable version of [Mono](http://www.mono-project.com/download/#download-lin)  
        Well, it runs. But it won't compile.
  - [ ] Make it compile on mono.  
        This is a problem. Mono's VBNC is only at VB.Net 8, which lacks LINQ, New-With, Lambdas, automatic properties, among others. Unfortunately, I use those extensively. Either a) I dumb it down to VB.Net 8.0 level, or b) automagically convert the whole project to C# and hope for the best. Both involve effort. Somehow, I like how the VB code looks for this project, I can't say I prefer either alternative. Maybe mono will update their VB compiler and/or MS will release theirs? Who knows.  
        Suits me for using a proprietary language to develop an open source app.

If you want to use JakMark and you have some feature request (there are no bugs), drop a line.

License
-------

JakMark is released under the terms of the Simplified BSD License.
