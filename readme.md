# FlashDevelop macro scripting

A collection of C# macro scripts for FlashDevelop.

## Why use C# macros?

C# macros are really powerful: scripts have unlimited access to the loaded libraries and plugins, and are a lot lighter than full-blown plugins. The only constraint is that they must fit into one C# file and you can't load DLLs to extend FlashDevelop functionalities. Also the syntax is limited to something like C# 2 (eg. no `var foo` inference).

Scripts are loaded and compiled on first use, which keeps FlashDevelop startup lean. You'll "feel" it as the first execution of a macro includes a little pause where the script is loaded and compiled. It is afterwards as fast as C# can be!

- [Script macros tutorial and examples](http://www.flashdevelop.org/community/viewtopic.php?f=20&t=5846)


## Installation

[Download the FDZ](https://github.com/elsassph/fdMacros/blob/master/dist/FDMacros.fdz?raw=true) and double-click to install.

If FlashDevelop was already running you'll have to restart it to see the macros appear in the Macros menu.


## Included macros

### Xml and Json Pretty Print

*Default shortcut: Ctrl+9*

Reformats (well-formed) XML and JSON; uses text selection or full document. Detection is rudimentary: XML text starts with a `<` and JSON starts with either `{` or `[`.

Notes: 

- XML prettify keeps eventual XML header (`<?xml ... ?>`) as-is,
- JSON indentation follows your editor defaults (tab/spaces) but not XML (always 2 spaces).


### Quick Trace

*Default shortcut: Ctrl+0*

Generate `trace('expr = ' + expr);`, using either selected text (so you can choose an expression) or the word at cursor position. 

Code is generated on the next line, unless a function declaration is detected, in which case the line will be tentatively inserted inside the function body.


### Copy as HTML

Select text in the editor and execute the macro - the corresponding HTML code, including CSS, will be copied in your clipboard.


## More scripting?

1. Create pull-requests to submit useful macros.
2. Create an issue to suggest a macro, but keep in mind that the scope of the macros must be limited to one C# file.