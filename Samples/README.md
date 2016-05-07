###About the samples

Objective of this custom tool is to facilitate creating self-contained and stand-alone text generation classes using Razor syntax without external dependencies, in particular, without taking dependency on MVC framework.

Having said that, the generated code needs a base-class with some supporting signatures. Again, short of slapping a full-featured base-class, you can tailor the fat to your needs. 

######Ready-to-use base-classes

If you are new to Razor or not there yet to start tinkering the base-classes, consider leveraging one of the two stock implementations (TextTemplate.cs or HtmlTemplate.cs) packaged into the custom tool. You can grab the source of the base-classes using following simple steps:
+ Create an empty file named TextTemplate.cshtml (or HtmlTemplate.cshtml)
+ Set the custom tool name and generate

Once generated, you can leave the base-classes as-is in your assembly (self-contained) or move them aside to a common assembly. In such case, your templates will take dependency on the common assembly you created. 

###### Before you start

> Following monolog assumes, you had successfully installed "Razor Text Template PreProcessor (RTT)" VSIX and the tool is working. It is OK if you are fishing around. No need to install the tool to follow-along below text.

######Sample01: Absolute minimal template with just enough signatures

Sample 01 shows absolute minimal signatures required for leveraging surprisingly good set of Razor features in a self-contained template. The base-class supporting the template has following signatures.

```cs
protected void WriteLiteral(object something);
protected void Write(object something);
protected abstract void Execute();
```

With this minimal signatures, many of the useful Razor features are not available. One particular feature that will not work is using @helper(s). From here on, you can progressively add additional Razor text and start isolating the breaking point. (It it compiles it should work).

######Few other points on the generated code

+ The @** Remove#Lines *@ directive at the top, as the name suggestes, removes #line directives from generated code. This is useful to review generated code. Nevertheless, doing so will take away ability to single-step through the Razor source while debugging. Try enabling and disabling the option and checkout debugging experience.
+ The generated class is marked  partial, this way, you can add supporting features in a buddy-class
+ The execute method is flagged protected, since it is NOT meant to be invoked directly.
+ The Render() method (not required by Razor) provides an ability to invoke the text generation process.

###### Sample 02

Sample 2 adds support for @helper feature of Razor. Use of @helper would generate code that would require few additional signatures. The Sample02Base.cs has sample implementation of following additional signatures.

```cs
protected static void WriteLiteralTo(TextWriter output, string something);
protected static void WriteTo(TextWriter output, object something);
protected void Write( /* HelperResult */ Action<TextWriter> writeAction)
```

The third signature which takes an `Action<TextWriter>` might look odd. If you are familiar with internals of Razor (MVC), you would expect an HelperResult class here. In order to mimimize the moving parts, the RTT tool uses `Action<TextWriter>` instead. As such, the base-class should support the third signature above. Implementation of these signatures are very basic. Refer `Sample02Base.cs` for details.

###### Sample 03 - Practical view on minimal signatures

The third sample is identical to second, except the base class includes 13 signatures, to give a realistic view. While it is nice to have a minimalist approach, base-classes of first two  samples may prove to be very restrictive. For more practical result, a base class that supports following 13 signatures, with some minimal error handling will help. The Execute() signature is abstract and not implemented by the base class itself. The generated code is responsible for the implementation. 

```cs
  protected static void WriteLiteralTo(TextWriter writer, string value) {}
  protected static void WriteLiteralTo(TextWriter writer, object value) {}
  protected static void WriteLiteralTo(TextWriter writer, /*HelperResult*/ Action<TextWriter> value) { }

  protected static void WriteTo(TextWriter writer, string value) { }
  protected static void WriteTo(TextWriter writer, object value) { }
  protected static void WriteTo(TextWriter writer, /*HelperResult*/ Action<TextWriter> value) { }

  protected void WriteLiteral(string value) { }
  protected void WriteLiteral(object value) { }
  protected void WriteLiteral(/*HelperResult*/ Action<TextWriter> value) { }

  protected void Write(string value) { }
  protected void Write(object value) { }
  protected void Write(/*HelperResult*/ Action<TextWriter> value) { }

  protected abstract void Execute();
```

Refer Sample03Base.cs for complete implementation of above signatures. 

While above signatures are good enough for pretty intense self-contained templates, they are not sufficient to support features like layouts and named sections. The gap is not too wide (refer subsequent samples). Nevertheless, at some point, you should take a practical view. If the objective is to have 100% of Razor features, you should not re-invent the wheel, instead, consider taking dependency on MVC DLLs.

###### Taking a pause...

If you cared to follow through this point, and wondering "why are we doing this", take a look at the references to the assembly containing the templates. It should just read "System". If you see any other references (MVC in particular), you are on a wrong trail.

Objective of this tool is to enable self-contained text generation classes (which may happen to be HTML), using Razor syntax, while avoiding external dependencies. If you are not convinced, probably, you do not have such usecase. In such case, you should revert back to full MVC and stick to the basics.











