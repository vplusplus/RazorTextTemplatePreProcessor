###About the samples

Objective of this custom tool is to facilitate creating self-contained and stand-alone text generation classes using Razor syntax without external dependencies, in particular, without taking dependency on MVC framework.

Having said that, the generated code needs a base-class with some supporting signatures. Again, short of slapping a full-featured base-class, you can tailor the fat to your needs. 

######Ready-to-use base-classes

If you are new to Razor or not there yet to start tinkering the base-classes, consider leveraging one of the two stock implementations (TextTemplate.cs or HtmlTemplate.cs) packaged into the custom tool. You can grab the source of the base-classes using following simple steps:
+ Create an empty file named TextTemplate.cshtml (or HtmlTemplate.cshtml)
+ Set the custom tool name and generate

Once generated, you can leave the base-classes as-is in your assembly (self-contained) or move them aside to a common assembly. In such case, your templates will take dependency on the common assembly you created. 

######Sample01: Absolute minimal template with just enough signatures

