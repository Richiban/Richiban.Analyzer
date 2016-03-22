# Richiban.Analyzer

I've finally decided to get in on that sweet, sweet Roslyn action and write myself an analyzer (please forgive the American spelling--since I'm programming against a series of types defined in American I've decided to use their spelling).

You'll probably know I'm a big fan of functional programming and I'm trying to write my C# in a more functional style; I've found that you can get many of the benefits of functional programming even though you're not writing in a functional language.
As it turns out, sometimes removing certain features from your programming language (or, at least, disallowing certain actions) can actually improve your code. An easy example of this is the goto statement; despite the fact that the C# language supports goto nobody uses it because it's widely regarded as a harmful thing to do.

Since disabling certain features of the language is quite easy to do (you simply need to make something that used to be legal into an error) I decided it would be a good place to start with my first Roslyn Analyzer.

Immutable values
------------

First order of business: mutable variables. In F# there are two types of variable: immutable (called "values") and mutable (called either "variables" or simply "mutable values"). This distinction helps the reader reason about the code you have written; it's surprisingly helpful to know that when you see a declaration such as let x = 5 you know that, as you read on, the value of x cannot change as you continue through the method. Similarly, if you see let mutable y = 10 then you are aware of the fact that y can change and you should be cognizant of that fact. Below you can see a screenshot of the finished product in use:

As you can see, we have successfully generated a compiler error from the reassignment of value x, an error that we do not see at the original declaration / assignment. In the Analyzer we can do this by checking the ExpressionStatement object to see if it is an AssignmentExpression. Note that declarations, such as var x = 5 do not come under the hierarchy of assignment statements in Roslyn.

From there we can decide whether the value is mutable or not. Since we cannot add new keywords to the language I have chosen to simply examine the name of the variable; I use a simple naming convention to determine whether the variable is mutable or not (I chose to use the underscore '_' because this simultaneously solves the issue of fields). I didn't want to make all fields or local variables immutable because this would make your programming life very hard indeed!

Here is the code for the analysis work going on here. Note that for my own understanding I have a chain of short methods with meaningful names, but you could easily condense this down to a single method that wouldn't be that long.

Unused return values
---------

Next is the issue that really prompted me to start writing an analyzer in the first place, and again we're replicating a feature from F#. When writing code in a modern, high-level style, we often adhere to a number of software principals, the most famous of which is SOLID. A lesser-known one, but equally important to me, is CQS (no, not CQRS. That's something slightly different).

CQS states that all functions (whether they are methods or otherwise) must take the form of either a command--functionality that causes state to change in the system--or a query--a procedure that will fetch, and possible map or reduce some data. So functions must alter state or fetch state but never both. This means that, in an OO language such as C#, all methods that have side effects must be void and all methods that return a value must have no side effects.

Given this, it seems reasonable to adopt F#'s compiler rule that any statement calling a function that returns data must use that data somehow (either by assigning it to a variable, awaiting it, passing it to another function call etc). After all, if methods that return data have no side effects then it doesn't make sense to call a method and ignore its result. Take the following example:

This is obviously a mistake on the part of the programmer--the result of GetData() isn't used which makes the call redundant.

What prompted me to highlight this issue in the first place revolves around Tasks and asynchronous programming. I recently spent some time upgrading one of our libraries to have an asynchronous API, that is: commands that used to be <em>void</em> now returned a <em>Task</em>. I found that, in the consumer of that library, it was incredibly difficult to find instances where an asynchronous method had been called but the resulting <em>Task</em> was ignored. Combining this with the principal of CQS I decided it was in my interest to be able to flag a compiler warning in each of the cases when this occurred. Thus, I created the ReturnValueUsageAnalyzer.

The process was relatively simple: for every ExpressionStatement (basically a line of code) we first determine if it is an Expression (which is usually is) and we then inspect the <em>SemanticModel</em> to see if the expression evaluates to a type that is not <em>void</em>. If it does then we have an unused return value!

There is one exception to this, which is the legacy-style modify operators such as +=, -=, ++ etc. These modify a variable <em>and</em> return its value (either the original value or the modified value depending on whether they are called prefix or postfix respectively). Since these operators have side effects I allow them. Since they all come under the umbrella of <em>AssignmentExpression</em> I can exclude them with this simple check:

Unlike the immutable value analyzer before, this only generates a warning, since it's not actually incorrect it's just likely a mistake, but at least we now have a way of hunting down these occurrences in our solution.


You can find the blog post that started this all at http://richiban.uk/2016/03/17/roslyn-analyzers/
