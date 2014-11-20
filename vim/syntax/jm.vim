" Vim syntax file
" Language: JakMark
" Version: 1.0

set syntax=

syn match JAKMARK_heading "^[#]\+.*$"
syn match JAKMARK_monospaced "`[^`]*`"
syn match JAKMARK_italic "\*\{1\}[^*]*\*\{1\}"
syn match JAKMARK_bold "\*\{2\}[^*]*\*\{2\}"

syn match JAKMARK_list "<list>"
syn match JAKMARK_list_e "</list>"
syn match JAKMARK_table "<list>"
syn match JAKMARK_table_e "</list>"
syn region JAKMARK_listg start="<list>" end="</list>" contains=ALL,JAKMARK_listitems keepend
syn match JAKMARK_listitems "[-#]" containedin=JAKMARK_listg contained
syn region JAKMARK_tableg start="<table>" end="</table>" contains=ALL,JAKMARK_pipe keepend
syn match JAKMARK_pipe "[|]" containedin=JAKMARK_tableg contained

syn region JAKMARK_fenced matchgroup=JAKMARK_fenced start="```" end="```" keepend
syn region JAKMARK_quoted matchgroup=JAKMARK_quoted start="[\\][!]" end="[\\][!]" keepend
syn match JAKMARK_note "\[^[^\]]*\]"
syn match JAKMARK_link1 "\[[^^][^\]]*\]\(([^)]*)\)\?"
syn match JAKMARK_linkref "^\s*:\[[^\]]*\]:"
syn region JAKMARK_comment matchgroup=JAKMARK_comment start="<!--" end="-->" keepend
syn match JAKMARK_lineFeed "[ ]\{2\}$"

hi link JAKMARK_heading Structure
hi link JAKMARK_quoted Identifier
"hi def JAKMARK_quoted guifg=Red term=NONE cterm=NONE gui=NONE
hi link JAKMARK_comment Comment

hi link JAKMARK_fenced Operator
"hi def JAKMARK_fenced term=underline cterm=underline gui=underline

"hi def JAKMARK_monospaced term=underline cterm=underline gui=underline
hi link JAKMARK_monospaced Operator
hi def JAKMARK_bold term=bold cterm=bold gui=bold
hi def JAKMARK_italic term=italic cterm=italic gui=italic
hi def JAKMARK_list term=inverse guifg=LightYellow guibg=Black cterm=inverse gui=inverse
hi link JAKMARK_list_e JAKMARK_list
hi link JAKMARK_table JAKMARK_list
hi link JAKMARK_table_e JAKMARK_list
hi link JAKMARK_note Underlined
"hi def JAKMARK_note term=inverse guifg=LightGreen guibg=Black cterm=inverse gui=inverse
"hi def JAKMARK_link1 term=underline cterm=underline gui=underline
hi link JAKMARK_link1 Underlined
hi link JAKMARK_linkref JAKMARK_note
hi def JAKMARK_listitems term=inverse cterm=inverse gui=inverse guifg=LightYellow guibg=Black
hi link JAKMARK_pipe JAKMARK_listitems

hi def JAKMARK_lineFeed term=inverse cterm=inverse gui=inverse guifg=LightBlue guibg=Black
