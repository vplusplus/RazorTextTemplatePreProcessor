###About the samples

Objective of this custom tool is to facilitate creating self-contained and stand-alone text generation classes using Razor syntax without external dependencies, in particular, without taking dependency on MVC framework.

Having said that, the generated code needs a base-class with some supporting signatures. Again, short of slapping a full-featured base-class, you can tailor the fat to your needs. 

######Ready-to-use base-classes

If you are new to Razor or not there yet to start tinkering the base-classes, consider leveraging one of the two stock implementations (TextTemplate.cs or HtmlTemplate.cs) packaged into the custom tool. You can grab the source of the base-classes using following simple steps:
+ Create an empty file named TextTemplate.cshtml (or HtmlTemplate.cshtml)
+ Set the custom tool name and generate

Once generated, you can leave the base-classes as-is in your assembly (self-contained) or move them aside to a common assembly. In such case, your templates will take dependency on the common assembly you created. 

######Sample01: Absolute minimal template with just enough signatures

Sample 01 shows absolute minimal signatures required for leveraging surprisingly good set of Razor features in a self-contained template. The base-class supporting the template has following signatures.

```cs
protected void WriteLiteral(object something);
protected void Write(object something);
protected abstract void Execute();
```

With this minimal signatures, many of the useful Razor features are not available. One particular feature that will not work is using @helper(s). From here on, you can progressively add additional Razor text and start isolating the breaking point. (It it compiles it should work).

##### Few other points on the Sample 01

1. The @** Remove#Lines *@ directive at the top, as the name suggestes, removed #line directives from generated code. This is useful to review generated code. Nevertheless, doing so will take away ability to single-step through the Razor source while debugging. Try enabling and disabling the option and checkout debugging experience.
2. The generated class is marked  partial, this way, you can add supporting features in a buddy-class




