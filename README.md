VerseCompiler
=============

A C# verse compiler

Verse is a programming language based around poems. This compiler compiler turns Verse into a simple intermediate language, and then interprets that. 

Below is a definition of the language of Verse:

Overview
--------

A program written in verse (or an anthology) is a collection of functions (poems). A poem must start on a new line and the first poem in an anthology is the 'main' function. Verse has variable decleration, assigment, if conditionals and while loops. There are a few inbuilt poems as a part of the language.

Poems
-----

A poem must start with a title (which is its signature). A title is of the form `(~)+(|)?(WORD)*(|)?(~)*`. Word is any word in the english language. The `~` symbol is used to denote the start of a poem title, you ay use as many as you like to make the title look pretty. A `|` may also be included at the end of string of `~` symbols, this means the poem has a return value.

The words in the title also have meaning (unless they are considered 'short' words in which case they are ignored). The first word in the title is the identifer for the poem. The other words are its arguments. A captial at the start of the word means the value is passed by reference, otherwise it is passed by value.

For example, one of the inbuilt poems is `Say` which would have the title `~~~Say Words~~~`. Say is a poem that has no return value and a single argument. Another is add, which adds to variables together, it would have the title `~~~|Add Eggs Ham|~~~`.

On the next line after the title comes the poem body, which is a series of words (again ignoring short words). A line ends after a newline, or any of the following synbols: `.`, `?`, `:`, `;`, `!`.

A line has both an L and R value. The L value of a line is the last thing in the line that had an L value (and similar for R). For example, if a line has two declerations, the line has an L value of the last variable in the second decleration.

Declaring variables
-------------------

To declare a variable you must use alliteration. Two words alliterate of their first constanant sound is the same. The first word in a string of alliteration is not made a variable. e.g. little lumpy lizards would create two new variables: lump and lizards (case insensitive). If a decleration is followed by a literal, all the variables have an initial value of that literal.

Literals
--------

Literals are surround with double qoutes. They will be treated as the most specific type possible (bool before int before float before string). `TRUE` and `FALSE` (any case) are booleans. You may type `"true"` or `"false"` without quotes and they will still be literals.

Assignment
----------

If two lines rhyme, then the R value of the second is assigned to the L value of the first. Be aware that if lines A B C rhyme, then the following assignments are generated: A = B, B = C. NOT B = C, A = B as you might expect. Also, if there is a poem of the form A B C, and lines A and C rhyme, the assignment happens AFTER any side effects of line B. i.e. assignment happens at the point of the second line that rhymes.

Truth
-----

A boolean is true if it is true.
An int or float are true if not 0.
A string is true if it is not the empty string.

If
----

An if is of the form

`A ? B.`                  (if A then B)
`or A ? B ; C.`           (if A then B else C)

A is a single line, and its Lvalue is checked in the condition. B and C are any number of lines. If it is true, then B is executed (else C is you use a ;).

An example:

    First we give the line an left value of "true" ? 
    this line will execute say "hello world"
    thing thing;
    this line will not say "secret password"
    words words.
    and then continues here say "hello back"

will print out hello world and then hello back.

While
-----

A while loop is of the form `A : B`. (while A do B).

A is a single line, B is any number of lines. B will execute while the Lvalue of A is true.

Calling Poems
-------------

If you mention the name of a poem in a line, it will be called. The words following the name are treated as arguments (they may also be function calls themselves).

Short words
-----------

`"IN"`, `"OF"`, `"THE"`, `"A"`, `"AT"`, `"AM"`, `"I"`, `"ODE"`, `"TO"`, `"MY"`, `"HER"`, `"HIS"`, `"IS"`, `"AND"`.

Inbuilt poems
-------------

`Say Word`

Aliases: `Say`, `Declare`

Will print out Word.

`Read`

Aliases: `Read`

Will read in and return a line from the command line.

`Add Eggs Ham`

Aliases: `Add`, `Sum`, `Join`

Will perform Eggs + Ham (or concat if either are strings) and return the result.

`Take Eggs Ham`

Aliases: `Take`

Will perform Eggs - Ham and return the result.

`Multiply Eggs Ham`

Aliases: `Multiply`

Will perform Eggs * Ham and return the result.

`Divide Eggs Ham`

Aliases: `Divide`

Will perform Eggs / Ham and return the result.

`Remain Eggs Ham`

Aliases: `Remain`

Will perform Eggs % Ham and return the result.

`Both Eggs Ham`

Aliases: `Both`

Will perform Eggs and Ham and return the result. Logic if bools, bitwise if ints.

`Either Eggs Ham`

Aliases: `Either`

Will perform Eggs or Ham and return the result. Logic if bools, bitwise if ints.

`Exclusively Eggs Ham`

Aliases: `Exclusively`

Will perform Eggs xor Ham and return the result. Logic if bools, bitwise if ints.

`Not Eggs`

Aliases: `Not`

Will perform not Eggsand return the result. Logic if bools, bitwise if ints.

`And Eggs Ham`
